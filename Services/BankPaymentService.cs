using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    /// <summary>Một hóa đơn (nhóm dòng HoaDonDauRa theo số nội bộ + khách) dùng để khớp / chọn gắn tay.</summary>
    public record InvoiceOption(string Noibo, Guid CustomerId, string CustomerName, string? EInvoiceNo, double TotalAfterTax, string? Currency, bool Paid)
    {
        public string Display => $"{EInvoiceNo} | {Noibo} | {CustomerName} | {TotalAfterTax:#,##0} {Currency}";
    }

    /// <summary>
    /// Nhận biến động số dư từ SePay, khớp với Phiếu thu theo số phiếu trong nội dung chuyển khoản,
    /// kiểm tra số tiền và tự tick "Đã thanh toán". Không phụ thuộc IJSRuntime nên gọi được từ controller.
    /// </summary>
    public class BankPaymentService(
        AppDbContext db,
        BkavInvoiceService bkav,
        BankPaymentNotifier notifier,
        IOptions<SePaySettings> options,
        ILogger<BankPaymentService> logger)
    {
        private const string Provider = "SePay";
        private static readonly Regex ReceiptTokenRegex = new(@"\b[A-Z]{0,6}PT\d{5,}\b", RegexOptions.Compiled);
        private static readonly Regex PaymentTokenRegex = new(@"\b[A-Z]{0,6}PC\d{5,}\b", RegexOptions.Compiled);

        /// <summary>Xử lý 1 webhook SePay. Ném exception nếu lỗi DB để controller trả 500 (SePay sẽ gửi lại).</summary>
        public async Task ProcessSePayAsync(SePayWebhookPayload p, string rawJson)
        {
            var isIn = string.Equals(p.TransferType, "in", StringComparison.OrdinalIgnoreCase);
            var isOut = string.Equals(p.TransferType, "out", StringComparison.OrdinalIgnoreCase);
            if (!isIn && !isOut)
                return;

            var configuredAccount = options.Value.AccountNumber;
            if (!string.IsNullOrWhiteSpace(configuredAccount) &&
                !string.Equals(configuredAccount.Trim(), p.AccountNumber?.Trim(), StringComparison.OrdinalIgnoreCase))
                return;

            var txnId = p.Id.ToString(CultureInfo.InvariantCulture);
            if (await db.BankTransaction.AnyAsync(x => x.Provider == Provider && x.ProviderTxnId == txnId))
                return;

            var txn = new M_BankTransaction
            {
                Id = Guid.NewGuid(),
                Provider = Provider,
                ProviderTxnId = txnId,
                Gateway = p.Gateway,
                AccountNumber = p.AccountNumber,
                SubAccount = p.SubAccount,
                TransactionDate = DateTime.TryParse(p.TransactionDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null,
                TransferType = p.TransferType,
                Amount = p.TransferAmount,
                Content = Truncate(p.Content ?? p.Description, 1000),
                ReferenceCode = Truncate(p.ReferenceCode, 100),
                RawJson = rawJson,
                MatchStatus = BankMatchStatus.Unmatched,
                ReceivedAt = DateTime.Now
            };
            db.BankTransaction.Add(txn);

            M_PhieuThu? receipt = null;
            InvoiceOption? invoice = null;
            M_PhieuChi? payment = null;
            var paidNow = false;
            var invoicePaidNow = false;
            var paymentPaidNow = false;

            if (isIn)
            {
                receipt = await FindReceiptAsync(txn.Content);
                invoice = receipt == null ? await FindInvoiceAsync(txn.Content, txn.Amount) : null;
            }
            else // isOut: công ty chuyển khoản cho khách — khớp với Phiếu chi
            {
                payment = await FindPaymentAsync(txn.Content);
            }

            if (invoice != null)
            {
                txn.HoaDonNoibo = invoice.Noibo;
                txn.HoaDonCustomerId = invoice.CustomerId;

                if (invoice.Paid)
                {
                    txn.MatchStatus = BankMatchStatus.AlreadyPaid;
                }
                else
                {
                    // Dùng đúng hàm tick tay của 5.14 để đồng bộ công nợ tự động.
                    var rs = await bkav.SetDaThanhToanAsync(invoice.Noibo, invoice.CustomerId, true, "SePay");
                    if (!rs.Success)
                        throw new InvalidOperationException($"Không tick được hóa đơn {invoice.EInvoiceNo}: {rs.Message}");
                    txn.MatchStatus = BankMatchStatus.Matched;
                    invoicePaidNow = true;
                }

                AddInvoiceLog("Bank Payment", invoice.Noibo, invoice.CustomerId, new
                {
                    Provider,
                    TxnId = txnId,
                    txn.Amount,
                    Expected = invoice.TotalAfterTax,
                    invoice.EInvoiceNo,
                    txn.MatchStatus,
                    txn.Content
                }, "SePay");
            }

            if (receipt != null)
            {
                txn.PhieuthuID = receipt.PhieuthuID;

                if (receipt.dathanhtoan == true)
                {
                    txn.MatchStatus = BankMatchStatus.AlreadyPaid;
                }
                else if (IsExactAmount(receipt.Currency, receipt.Sotien, txn.Amount))
                {
                    txn.MatchStatus = BankMatchStatus.Matched;
                    receipt.dathanhtoan = true;
                    receipt.NgayThanhToan = txn.TransactionDate ?? DateTime.Now;
                    paidNow = true;
                }
                else
                {
                    txn.MatchStatus = BankMatchStatus.WrongAmount;
                }

                AddLog("Bank Payment", receipt, new
                {
                    Provider,
                    TxnId = txnId,
                    txn.Amount,
                    Expected = receipt.Sotien,
                    txn.MatchStatus,
                    txn.Content
                }, "SePay");
            }

            if (payment != null)
            {
                txn.PhieuchiID = payment.PhieuchiID;

                if (payment.dathanhtoan == true)
                {
                    txn.MatchStatus = BankMatchStatus.AlreadyPaid;
                }
                else if (IsExactAmount(payment.Currency, payment.Sotien, txn.Amount))
                {
                    txn.MatchStatus = BankMatchStatus.Matched;
                    payment.dathanhtoan = true;
                    payment.NgayThanhToan = txn.TransactionDate ?? DateTime.Now;
                    paymentPaidNow = true;
                }
                else
                {
                    txn.MatchStatus = BankMatchStatus.WrongAmount;
                }

                AddPaymentLog("Bank Payment", payment, new
                {
                    Provider,
                    TxnId = txnId,
                    txn.Amount,
                    Expected = payment.Sotien,
                    txn.MatchStatus,
                    txn.Content
                }, "SePay");
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Webhook đến trùng đồng thời: giao dịch đã được xử lý bởi request kia.
                if (await IsDuplicateAsync(txnId)) return;
                throw;
            }

            if (paidNow && receipt != null)
                await notifier.PublishAsync(receipt.PhieuthuID, receipt.SoPhieuthu);
            if (invoicePaidNow && invoice != null)
                await notifier.PublishInvoicePaidAsync(invoice.Noibo, invoice.CustomerId, invoice.EInvoiceNo);
            if (paymentPaidNow && payment != null)
                await notifier.PublishPaymentPaidAsync(payment.PhieuchiID, payment.Sophieuchi);
        }

        /// <summary>Kế toán gắn tay 1 giao dịch vào phiếu thu. Tự tick nếu đúng số tiền và phiếu chưa thanh toán.</summary>
        public async Task<(bool ok, string message)> LinkToReceiptAsync(Guid txnId, Guid phieuthuId, string user)
        {
            var txn = await db.BankTransaction.FirstOrDefaultAsync(x => x.Id == txnId);
            var receipt = await db.Phieuthu.FirstOrDefaultAsync(x => x.PhieuthuID == phieuthuId);
            if (txn == null || receipt == null)
                return (false, "Không tìm thấy giao dịch hoặc phiếu thu");

            txn.PhieuthuID = phieuthuId;
            txn.HoaDonNoibo = null;
            txn.HoaDonCustomerId = null;
            txn.PhieuchiID = null;
            txn.MatchStatus = BankMatchStatus.Manual;

            var paidNow = false;
            var message = "Đã gắn giao dịch vào phiếu thu";
            if (receipt.dathanhtoan != true && IsExactAmount(receipt.Currency, receipt.Sotien, txn.Amount))
            {
                receipt.dathanhtoan = true;
                receipt.NgayThanhToan = txn.TransactionDate ?? DateTime.Now;
                paidNow = true;
                message = "Đã gắn giao dịch và đánh dấu phiếu thu Đã thanh toán";
            }
            else if (receipt.dathanhtoan != true)
            {
                message = $"Đã gắn giao dịch. Số tiền nhận {txn.Amount:#,##0} khác số tiền phiếu {receipt.Sotien:#,##0}, cần kiểm tra và tick tay";
            }

            AddLog("Bank Payment Link", receipt, new { txnId, txn.Amount, Expected = receipt.Sotien, PaidNow = paidNow }, user);
            await db.SaveChangesAsync();

            if (paidNow)
                await notifier.PublishAsync(receipt.PhieuthuID, receipt.SoPhieuthu);

            return (true, message);
        }

        /// <summary>Kế toán gắn tay 1 giao dịch vào hóa đơn (5.14). Tự tick nếu đúng Amount (VAT) và hóa đơn chưa thanh toán.</summary>
        public async Task<(bool ok, string message)> LinkToInvoiceAsync(Guid txnId, string noibo, Guid customerId, string user)
        {
            var txn = await db.BankTransaction.FirstOrDefaultAsync(x => x.Id == txnId);
            var invoice = await GetInvoiceAsync(noibo, customerId);
            if (txn == null || invoice == null)
                return (false, "Không tìm thấy giao dịch hoặc hóa đơn");

            txn.HoaDonNoibo = invoice.Noibo;
            txn.HoaDonCustomerId = invoice.CustomerId;
            txn.PhieuthuID = null;
            txn.PhieuchiID = null;
            txn.MatchStatus = BankMatchStatus.Manual;

            var paidNow = false;
            var message = "Đã gắn giao dịch vào hóa đơn";
            if (!invoice.Paid && IsInvoiceAmountExact(invoice, txn.Amount))
            {
                var rs = await bkav.SetDaThanhToanAsync(invoice.Noibo, invoice.CustomerId, true, user);
                if (!rs.Success)
                    return (false, rs.Message);
                paidNow = true;
                message = "Đã gắn giao dịch và đánh dấu hóa đơn Đã thanh toán";
            }
            else if (!invoice.Paid)
            {
                message = $"Đã gắn giao dịch. Số tiền nhận {txn.Amount:#,##0} khác Amount (VAT) {invoice.TotalAfterTax:#,##0}, cần kiểm tra và tick tay ở 5.14";
            }

            AddInvoiceLog("Bank Payment Link", invoice.Noibo, invoice.CustomerId,
                new { txnId, txn.Amount, Expected = invoice.TotalAfterTax, invoice.EInvoiceNo, PaidNow = paidNow }, user);
            await db.SaveChangesAsync();

            if (paidNow)
                await notifier.PublishInvoicePaidAsync(invoice.Noibo, invoice.CustomerId, invoice.EInvoiceNo);

            return (true, message);
        }

        /// <summary>Kế toán gắn tay 1 giao dịch vào phiếu chi. Tự tick nếu đúng số tiền và phiếu chưa thanh toán.</summary>
        public async Task<(bool ok, string message)> LinkToPaymentAsync(Guid txnId, Guid phieuchiId, string user)
        {
            var txn = await db.BankTransaction.FirstOrDefaultAsync(x => x.Id == txnId);
            var payment = await db.Phieuchi.FirstOrDefaultAsync(x => x.PhieuchiID == phieuchiId);
            if (txn == null || payment == null)
                return (false, "Không tìm thấy giao dịch hoặc phiếu chi");

            txn.PhieuchiID = phieuchiId;
            txn.PhieuthuID = null;
            txn.HoaDonNoibo = null;
            txn.HoaDonCustomerId = null;
            txn.MatchStatus = BankMatchStatus.Manual;

            var paidNow = false;
            var message = "Đã gắn giao dịch vào phiếu chi";
            if (payment.dathanhtoan != true && IsExactAmount(payment.Currency, payment.Sotien, txn.Amount))
            {
                payment.dathanhtoan = true;
                payment.NgayThanhToan = txn.TransactionDate ?? DateTime.Now;
                paidNow = true;
                message = "Đã gắn giao dịch và đánh dấu phiếu chi Đã thanh toán";
            }
            else if (payment.dathanhtoan != true)
            {
                message = $"Đã gắn giao dịch. Số tiền chuyển {txn.Amount:#,##0} khác số tiền phiếu {payment.Sotien:#,##0}, cần kiểm tra và tick tay";
            }

            AddPaymentLog("Bank Payment Link", payment, new { txnId, txn.Amount, Expected = payment.Sotien, PaidNow = paidNow }, user);
            await db.SaveChangesAsync();

            if (paidNow)
                await notifier.PublishPaymentPaidAsync(payment.PhieuchiID, payment.Sophieuchi);

            return (true, message);
        }

        /// <summary>Tìm hóa đơn để chọn khi gắn tay: theo số hóa đơn điện tử, số nội bộ hoặc tên khách.</summary>
        public async Task<List<InvoiceOption>> SearchInvoicesAsync(string? text, int take = 20)
        {
            var q =
                from h in db.HoaDonDauRa.AsNoTracking()
                join c in db.Customer.AsNoTracking() on h.customerid equals c.Customer_ID
                where h.continued == true && h.sohoadonNoibo != null && h.sohoadonNoibo != ""
                select new { h, c.COMPANY };

            if (!string.IsNullOrWhiteSpace(text))
            {
                var t = text.Trim();
                q = q.Where(x => (x.h.sohoadonDientu != null && x.h.sohoadonDientu.Contains(t))
                              || x.h.sohoadonNoibo!.Contains(t)
                              || (x.COMPANY != null && x.COMPANY.Contains(t)));
            }

            var rows = await q
                .OrderByDescending(x => x.h.ngayphathanhhoadonDientu)
                .Take(300)
                .Select(x => new InvoiceLine(x.h.sohoadonNoibo!, x.h.customerid, x.COMPANY, x.h.sohoadonDientu,
                                             x.h.thanhtiensauthue, x.h.tiente, x.h.dathanhtoan))
                .ToListAsync();

            return GroupInvoices(rows).Take(take).ToList();
        }

        /// <summary>Tổng tiền đã gắn cho từng hóa đơn (số nội bộ + khách hàng).</summary>
        public async Task<Dictionary<(string Noibo, Guid CustomerId), decimal>> GetReceivedByInvoiceAsync()
        {
            var rows = await db.BankTransaction.AsNoTracking()
                .Where(x => x.HoaDonNoibo != null && x.HoaDonCustomerId != null)
                .GroupBy(x => new { x.HoaDonNoibo, x.HoaDonCustomerId })
                .Select(g => new { g.Key.HoaDonNoibo, g.Key.HoaDonCustomerId, Total = g.Sum(x => x.Amount) })
                .ToListAsync();
            return rows.ToDictionary(x => (x.HoaDonNoibo!.Trim(), x.HoaDonCustomerId!.Value), x => x.Total);
        }

        /// <summary>Tick / bỏ tick tay "Đã thanh toán". Chỉ đổi 2 cột này để không ghi đè phần còn lại của phiếu.</summary>
        public async Task<bool> SetPaidAsync(Guid phieuthuId, bool paid, string user)
        {
            var paidDate = paid ? DateTime.Now : (DateTime?)null;
            var affected = await db.Phieuthu
                .Where(x => x.PhieuthuID == phieuthuId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.dathanhtoan, paid)
                    .SetProperty(x => x.NgayThanhToan, paidDate));
            if (affected == 0) return false;

            var no = await db.Phieuthu.AsNoTracking()
                .Where(x => x.PhieuthuID == phieuthuId)
                .Select(x => x.SoPhieuthu)
                .FirstOrDefaultAsync();
            db.HistoryLogs.Add(new HistoryLog
            {
                Id = Guid.NewGuid(),
                UserName = user,
                Action = paid ? "Mark Paid" : "Unmark Paid",
                EntityName = "Phieuthu",
                EntityId = phieuthuId,
                No = no,
                Timestamp = DateTime.Now
            });
            await db.SaveChangesAsync();
            return true;
        }

        /// <summary>Tick / bỏ tick tay "Đã thanh toán" cho phiếu chi. Chỉ đổi 2 cột này để không ghi đè phần còn lại của phiếu.</summary>
        public async Task<bool> SetPaymentPaidAsync(Guid phieuchiId, bool paid, string user)
        {
            var paidDate = paid ? DateTime.Now : (DateTime?)null;
            var affected = await db.Phieuchi
                .Where(x => x.PhieuchiID == phieuchiId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.dathanhtoan, paid)
                    .SetProperty(x => x.NgayThanhToan, paidDate));
            if (affected == 0) return false;

            var no = await db.Phieuchi.AsNoTracking()
                .Where(x => x.PhieuchiID == phieuchiId)
                .Select(x => x.Sophieuchi)
                .FirstOrDefaultAsync();
            db.HistoryLogs.Add(new HistoryLog
            {
                Id = Guid.NewGuid(),
                UserName = user,
                Action = paid ? "Mark Paid" : "Unmark Paid",
                EntityName = "Phieuchi",
                EntityId = phieuchiId,
                No = no,
                Timestamp = DateTime.Now
            });
            await db.SaveChangesAsync();
            return true;
        }

        /// <summary>Mọi giao dịch đã gắn vào một phiếu thu (dùng cho chip "Sai số tiền" và dòng chi tiết của lưới).</summary>
        public async Task<List<M_BankTransaction>> GetLinkedTransactionsAsync()
        {
            return await db.BankTransaction.AsNoTracking()
                .Where(x => x.PhieuthuID != null)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }

        /// <summary>Mọi giao dịch đã gắn vào một phiếu chi (dùng cho chip "Sai số tiền" và dòng chi tiết của lưới 10.2).</summary>
        public async Task<List<M_BankTransaction>> GetLinkedPaymentTransactionsAsync()
        {
            return await db.BankTransaction.AsNoTracking()
                .Where(x => x.PhieuchiID != null)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }

        /// <summary>Tổng tiền đã gắn cho từng phiếu chi (dùng cho chip trên 10.2, cùng cách với GetReceivedByInvoiceAsync).</summary>
        public async Task<Dictionary<Guid, decimal>> GetReceivedByPhieuChiAsync()
        {
            return await db.BankTransaction.AsNoTracking()
                .Where(x => x.PhieuchiID != null)
                .GroupBy(x => x.PhieuchiID!.Value)
                .Select(g => new { Id = g.Key, Total = g.Sum(x => x.Amount) })
                .ToDictionaryAsync(x => x.Id, x => x.Total);
        }

        /// <summary>Danh sách giao dịch cho 10.1.1 (direction "in") và 10.2.1 (direction "out"). direction = null trả cả hai chiều.</summary>
        public async Task<List<M_BankTransaction>> GetTransactionsAsync(bool onlyNeedReview, string? direction = null)
        {
            var q = db.BankTransaction.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(direction))
                q = q.Where(x => x.TransferType != null && x.TransferType.ToLower() == direction.ToLower());
            if (onlyNeedReview)
                q = q.Where(x => x.MatchStatus == BankMatchStatus.Unmatched
                              || x.MatchStatus == BankMatchStatus.WrongAmount
                              || x.MatchStatus == BankMatchStatus.AlreadyPaid);
            var list = await q.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.ReceivedAt).ToListAsync();
            await FillLinkedLabelsAsync(list);
            return list;
        }

        private async Task FillLinkedLabelsAsync(List<M_BankTransaction> list)
        {
            // JOIN thay vì Contains(list): DB mức tương thích thấp không hỗ trợ OPENJSON.
            var receiptNos = await (
                from t in db.BankTransaction.AsNoTracking()
                where t.PhieuthuID != null
                join p in db.Phieuthu.AsNoTracking() on t.PhieuthuID equals p.PhieuthuID
                select new { t.Id, p.SoPhieuthu }).ToListAsync();
            var receiptById = receiptNos.ToDictionary(x => x.Id, x => x.SoPhieuthu);

            var paymentNos = await (
                from t in db.BankTransaction.AsNoTracking()
                where t.PhieuchiID != null
                join c in db.Phieuchi.AsNoTracking() on t.PhieuchiID equals c.PhieuchiID
                select new { t.Id, c.Sophieuchi }).ToListAsync();
            var paymentById = paymentNos.ToDictionary(x => x.Id, x => x.Sophieuchi);

            var invoiceLabels = new Dictionary<(string, Guid), string>();
            foreach (var t in list.Where(x => x.HoaDonNoibo != null && x.HoaDonCustomerId != null))
            {
                var key = (t.HoaDonNoibo!.Trim(), t.HoaDonCustomerId!.Value);
                if (!invoiceLabels.ContainsKey(key))
                {
                    var invoice = await GetInvoiceAsync(key.Item1, key.Item2);
                    invoiceLabels[key] = $"HĐ {invoice?.EInvoiceNo ?? key.Item1}";
                }
                t.LinkedLabel = invoiceLabels[key];
            }

            foreach (var t in list.Where(x => x.PhieuthuID != null))
                if (receiptById.TryGetValue(t.Id, out var no))
                    t.LinkedLabel = no;

            foreach (var t in list.Where(x => x.PhieuchiID != null))
                if (paymentById.TryGetValue(t.Id, out var no))
                    t.LinkedLabel = no;
        }

        /// <summary>Danh sách phiếu thu để chọn khi gắn tay (số phiếu + khách hàng + số tiền).</summary>
        public async Task<List<M_PhieuThu>> SearchReceiptsAsync(string? text, int take = 20)
        {
            var q = db.Phieuthu.AsNoTracking().Where(x => x.Loaiphieu == "PT");
            if (!string.IsNullOrWhiteSpace(text))
            {
                var t = text.Trim();
                q = q.Where(x => (x.SoPhieuthu != null && x.SoPhieuthu.Contains(t))
                              || (x.Customername != null && x.Customername.Contains(t))
                              || (x.Nguoinoptien != null && x.Nguoinoptien.Contains(t)));
            }
            return await q.OrderByDescending(x => x.SoPhieuthu).Take(take).ToListAsync();
        }

        /// <summary>Danh sách phiếu chi để chọn khi gắn tay (số phiếu + người nhận + số tiền).</summary>
        public async Task<List<M_PhieuChi>> SearchPaymentsAsync(string? text, int take = 20)
        {
            var q = db.Phieuchi.AsNoTracking().Where(x => x.Loaiphieu == "PC");
            if (!string.IsNullOrWhiteSpace(text))
            {
                var t = text.Trim();
                q = q.Where(x => (x.Sophieuchi != null && x.Sophieuchi.Contains(t))
                              || (x.Nguoinoptien != null && x.Nguoinoptien.Contains(t))
                              || (x.Noidung != null && x.Noidung.Contains(t)));
            }
            return await q.OrderByDescending(x => x.Sophieuchi).Take(take).ToListAsync();
        }

        // ---- matching ----

        private async Task<M_PhieuThu?> FindReceiptAsync(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            var tokens = NormalizeTokens(content);
            var candidates = ReceiptTokenRegex.Matches(tokens).Select(m => m.Value).Distinct().ToList();

            if (candidates.Count > 0)
            {
                // Không dùng candidates.Contains(...): EF Core 8 dịch thành OPENJSON, DB này compatibility level < 130 không hỗ trợ.
                var found = new List<M_PhieuThu>();
                foreach (var candidate in candidates.Take(5))
                {
                    var rows = await db.Phieuthu
                        .Where(x => x.Loaiphieu == "PT" && x.SoPhieuthu != null
                            && x.SoPhieuthu.Replace("_", "").Replace("-", "").Replace(" ", "") == candidate)
                        .Take(2)
                        .ToListAsync();
                    found.AddRange(rows.Where(h => found.All(f => f.PhieuthuID != h.PhieuthuID)));
                }
                if (found.Count == 1) return found[0];
                if (found.Count > 1) return null; // mơ hồ, để kế toán gắn tay
            }

            // Ngân hàng đôi khi dính liền số phiếu với chữ khác: thử tìm số phiếu nằm trong chuỗi liền không dấu cách.
            var squashed = tokens.Replace(" ", "");
            var unpaid = await db.Phieuthu
                .Where(x => x.Loaiphieu == "PT" && x.SoPhieuthu != null && x.dathanhtoan != true)
                .Select(x => new { x.PhieuthuID, x.SoPhieuthu })
                .ToListAsync();
            var hits = unpaid
                .Where(x => x.SoPhieuthu!.Replace("_", "").Replace("-", "").Replace(" ", "").Length >= 8
                         && squashed.Contains(x.SoPhieuthu!.Replace("_", "").Replace("-", "").Replace(" ", "").ToUpperInvariant()))
                .ToList();
            if (hits.Count == 1)
                return await db.Phieuthu.FirstOrDefaultAsync(x => x.PhieuthuID == hits[0].PhieuthuID);

            return null;
        }

        /// <summary>Khớp phiếu chi theo số phiếu trong nội dung chuyển khoản đi — cùng thuật toán với FindReceiptAsync.</summary>
        private async Task<M_PhieuChi?> FindPaymentAsync(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            var tokens = NormalizeTokens(content);
            var candidates = PaymentTokenRegex.Matches(tokens).Select(m => m.Value).Distinct().ToList();

            if (candidates.Count > 0)
            {
                // Không dùng candidates.Contains(...): EF Core 8 dịch thành OPENJSON, DB này compatibility level < 130 không hỗ trợ.
                var found = new List<M_PhieuChi>();
                foreach (var candidate in candidates.Take(5))
                {
                    var rows = await db.Phieuchi
                        .Where(x => x.Loaiphieu == "PC" && x.Sophieuchi != null
                            && x.Sophieuchi.Replace("_", "").Replace("-", "").Replace(" ", "") == candidate)
                        .Take(2)
                        .ToListAsync();
                    found.AddRange(rows.Where(h => found.All(f => f.PhieuchiID != h.PhieuchiID)));
                }
                if (found.Count == 1) return found[0];
                if (found.Count > 1) return null; // mơ hồ, để kế toán gắn tay
            }

            // Ngân hàng đôi khi dính liền số phiếu với chữ khác: thử tìm số phiếu nằm trong chuỗi liền không dấu cách.
            var squashed = tokens.Replace(" ", "");
            var unpaid = await db.Phieuchi
                .Where(x => x.Loaiphieu == "PC" && x.Sophieuchi != null && x.dathanhtoan != true)
                .Select(x => new { x.PhieuchiID, x.Sophieuchi })
                .ToListAsync();
            var hits2 = unpaid
                .Where(x => x.Sophieuchi!.Replace("_", "").Replace("-", "").Replace(" ", "").Length >= 8
                         && squashed.Contains(x.Sophieuchi!.Replace("_", "").Replace("-", "").Replace(" ", "").ToUpperInvariant()))
                .ToList();
            if (hits2.Count == 1)
                return await db.Phieuchi.FirstOrDefaultAsync(x => x.PhieuchiID == hits2[0].PhieuchiID);

            return null;
        }

        private sealed record InvoiceLine(string Noibo, Guid CustomerId, string? Customer, string? EInvoiceNo, double? AfterTax, string? Currency, bool? Paid);

        private static List<InvoiceOption> GroupInvoices(IEnumerable<InvoiceLine> lines) =>
            lines
                .GroupBy(x => (Noibo: x.Noibo.Trim(), x.CustomerId))
                .Select(g => new InvoiceOption(
                    g.Key.Noibo,
                    g.Key.CustomerId,
                    g.Select(x => x.Customer).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "",
                    g.Select(x => x.EInvoiceNo?.Trim()).FirstOrDefault(x => !string.IsNullOrEmpty(x)),
                    g.Sum(x => x.AfterTax ?? 0),
                    g.Select(x => x.Currency).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                    g.All(x => x.Paid == true)))
                .ToList();

        /// <summary>Một hóa đơn theo khóa nhóm (số nội bộ + khách), cùng công thức tổng với 5.14.</summary>
        private async Task<InvoiceOption?> GetInvoiceAsync(string noibo, Guid customerId)
        {
            var key = noibo.Trim();
            var lines = await db.HoaDonDauRa.AsNoTracking()
                .Where(x => x.continued == true && x.customerid == customerId && x.sohoadonNoibo != null && x.sohoadonNoibo.Trim() == key)
                .Select(x => new InvoiceLine(x.sohoadonNoibo!, x.customerid, null, x.sohoadonDientu, x.thanhtiensauthue, x.tiente, x.dathanhtoan))
                .ToListAsync();
            return GroupInvoices(lines).FirstOrDefault();
        }

        /// <summary>
        /// Khớp hóa đơn theo E-Invoice No: mọi dãy số trong nội dung (bỏ số 0 đầu) bằng số hóa đơn điện tử VÀ tiền nhận bằng Amount (VAT).
        /// Vì xét mọi con số nên số tiền là điều kiện bắt buộc; 0 hoặc nhiều hóa đơn khớp thì để kế toán gắn tay.
        /// </summary>
        private async Task<InvoiceOption?> FindInvoiceAsync(string? content, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            var numbers = Regex.Matches(NormalizeTokens(content), @"\d+")
                .Select(m => m.Value.TrimStart('0'))
                .Where(x => x.Length > 0)
                .ToHashSet();
            if (numbers.Count == 0) return null;

            // Lọc trong bộ nhớ: DB mức tương thích thấp không hỗ trợ Contains(list) (OPENJSON).
            var rows = await db.HoaDonDauRa.AsNoTracking()
                .Where(x => x.continued == true
                    && x.sohoadonDientu != null && x.sohoadonDientu != ""
                    && x.sohoadonNoibo != null && x.sohoadonNoibo != "")
                .Select(x => new InvoiceLine(x.sohoadonNoibo!, x.customerid, null, x.sohoadonDientu, x.thanhtiensauthue, x.tiente, x.dathanhtoan))
                .ToListAsync();

            var matchedKeys = rows
                .Where(x => x.EInvoiceNo != null && Regex.IsMatch(x.EInvoiceNo.Trim(), @"^\d+$")
                            && numbers.Contains(x.EInvoiceNo.Trim().TrimStart('0')))
                .Select(x => (x.Noibo.Trim(), x.CustomerId))
                .ToHashSet();
            if (matchedKeys.Count == 0) return null;

            var hits = GroupInvoices(rows.Where(x => matchedKeys.Contains((x.Noibo.Trim(), x.CustomerId))))
                .Where(x => IsInvoiceAmountExact(x, amount))
                .ToList();

            if (hits.Count > 1)
                logger.LogInformation("Bank content '{Content}' matches {Count} invoices with same amount, left for manual link", content, hits.Count);

            return hits.Count == 1 ? hits[0] : null;
        }

        private static bool IsInvoiceAmountExact(InvoiceOption invoice, decimal amount)
        {
            var currency = invoice.Currency?.Trim();
            var currencyOk = string.IsNullOrEmpty(currency) || currency.Equals("VND", StringComparison.OrdinalIgnoreCase);
            return currencyOk && invoice.TotalAfterTax > 0 && Math.Abs((decimal)invoice.TotalAfterTax - amount) < 1m;
        }

        private void AddInvoiceLog(string action, string noibo, Guid customerId, object changes, string user)
        {
            db.HistoryLogs.Add(new HistoryLog
            {
                Id = Guid.NewGuid(),
                UserName = user,
                Action = action,
                EntityName = "HoaDonDauRa",
                EntityId = customerId,
                No = noibo,
                Changes = JsonSerializer.Serialize(changes),
                Timestamp = DateTime.Now
            });
        }

        /// <summary>Bỏ dấu, viết hoa, bỏ '_' và '-', các ký tự khác thành dấu cách (giữ ranh giới từ).</summary>
        internal static string NormalizeTokens(string s)
        {
            var d = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(d.Length);
            foreach (var ch in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                if (ch == 'đ' || ch == 'Đ') { sb.Append('D'); continue; }
                if (ch == '_' || ch == '-') continue;
                sb.Append(char.IsLetterOrDigit(ch) ? char.ToUpperInvariant(ch) : ' ');
            }
            return sb.ToString();
        }

        /// <summary>Đúng số tiền: VND (hoặc chưa ghi loại tiền) và tiền nhận/chuyển bằng số tiền chứng từ (lệch dưới 1 đồng do làm tròn thì vẫn coi là bằng). Dùng chung cho Phiếu thu và Phiếu chi.</summary>
        private bool IsExactAmount(string? currency, double? expectedAmount, decimal amount)
        {
            var cur = currency?.Trim();
            var currencyOk = string.IsNullOrEmpty(cur) || cur.Equals("VND", StringComparison.OrdinalIgnoreCase);
            var expected = (decimal)(expectedAmount ?? 0);
            var amountOk = expected > 0 && Math.Abs(expected - amount) < 1m;

            if (!currencyOk || !amountOk)
                logger.LogInformation(
                    "Bank amount check failed: Currency='{Currency}' (ok={CurrencyOk}), Expected={Expected}, received={Amount} (ok={AmountOk})",
                    currency, currencyOk, expectedAmount, amount, amountOk);

            return currencyOk && amountOk;
        }

        private async Task<bool> IsDuplicateAsync(string txnId)
        {
            db.ChangeTracker.Clear();
            return await db.BankTransaction.AnyAsync(x => x.Provider == Provider && x.ProviderTxnId == txnId);
        }

        private void AddLog(string action, M_PhieuThu receipt, object changes, string user)
        {
            db.HistoryLogs.Add(new HistoryLog
            {
                Id = Guid.NewGuid(),
                UserName = user,
                Action = action,
                EntityName = "Phieuthu",
                EntityId = receipt.PhieuthuID,
                No = receipt.SoPhieuthu,
                Changes = JsonSerializer.Serialize(changes),
                Timestamp = DateTime.Now
            });
        }

        private void AddPaymentLog(string action, M_PhieuChi payment, object changes, string user)
        {
            db.HistoryLogs.Add(new HistoryLog
            {
                Id = Guid.NewGuid(),
                UserName = user,
                Action = action,
                EntityName = "Phieuchi",
                EntityId = payment.PhieuchiID,
                No = payment.Sophieuchi,
                Changes = JsonSerializer.Serialize(changes),
                Timestamp = DateTime.Now
            });
        }

        private static string? Truncate(string? s, int max) =>
            s == null ? null : (s.Length <= max ? s : s[..max]);
    }
}
