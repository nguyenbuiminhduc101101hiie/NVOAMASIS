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
    /// <summary>
    /// Nhận biến động số dư từ SePay, khớp với Phiếu thu theo số phiếu trong nội dung chuyển khoản,
    /// kiểm tra số tiền và tự tick "Đã thanh toán". Không phụ thuộc IJSRuntime nên gọi được từ controller.
    /// </summary>
    public class BankPaymentService(
        AppDbContext db,
        BankPaymentNotifier notifier,
        IOptions<SePaySettings> options,
        ILogger<BankPaymentService> logger)
    {
        private const string Provider = "SePay";
        private static readonly Regex ReceiptTokenRegex = new(@"\b[A-Z]{0,6}PT\d{5,}\b", RegexOptions.Compiled);

        /// <summary>Xử lý 1 webhook SePay. Ném exception nếu lỗi DB để controller trả 500 (SePay sẽ gửi lại).</summary>
        public async Task ProcessSePayAsync(SePayWebhookPayload p, string rawJson)
        {
            if (!string.Equals(p.TransferType, "in", StringComparison.OrdinalIgnoreCase))
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

            M_PhieuThu? receipt = await FindReceiptAsync(txn.Content);
            var paidNow = false;

            if (receipt != null)
            {
                txn.PhieuthuID = receipt.PhieuthuID;

                if (receipt.dathanhtoan == true)
                {
                    txn.MatchStatus = BankMatchStatus.AlreadyPaid;
                }
                else if (IsExactAmount(receipt, txn.Amount))
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
        }

        /// <summary>Kế toán gắn tay 1 giao dịch vào phiếu thu. Tự tick nếu đúng số tiền và phiếu chưa thanh toán.</summary>
        public async Task<(bool ok, string message)> LinkToReceiptAsync(Guid txnId, Guid phieuthuId, string user)
        {
            var txn = await db.BankTransaction.FirstOrDefaultAsync(x => x.Id == txnId);
            var receipt = await db.Phieuthu.FirstOrDefaultAsync(x => x.PhieuthuID == phieuthuId);
            if (txn == null || receipt == null)
                return (false, "Không tìm thấy giao dịch hoặc phiếu thu");

            txn.PhieuthuID = phieuthuId;
            txn.MatchStatus = BankMatchStatus.Manual;

            var paidNow = false;
            var message = "Đã gắn giao dịch vào phiếu thu";
            if (receipt.dathanhtoan != true && IsExactAmount(receipt, txn.Amount))
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

        /// <summary>Mọi giao dịch đã gắn vào một phiếu thu (dùng cho chip "Sai số tiền" và dòng chi tiết của lưới).</summary>
        public async Task<List<M_BankTransaction>> GetLinkedTransactionsAsync()
        {
            return await db.BankTransaction.AsNoTracking()
                .Where(x => x.PhieuthuID != null)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<M_BankTransaction>> GetTransactionsAsync(bool onlyNeedReview)
        {
            var q = db.BankTransaction.AsNoTracking().AsQueryable();
            if (onlyNeedReview)
                q = q.Where(x => x.MatchStatus == BankMatchStatus.Unmatched
                              || x.MatchStatus == BankMatchStatus.WrongAmount
                              || x.MatchStatus == BankMatchStatus.AlreadyPaid);
            return await q.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.ReceivedAt).ToListAsync();
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

        /// <summary>Đúng số tiền: phiếu VND (hoặc chưa ghi loại tiền) và tiền nhận bằng số tiền phiếu (lệch dưới 1 đồng do làm tròn thì vẫn coi là bằng).</summary>
        private bool IsExactAmount(M_PhieuThu receipt, decimal amount)
        {
            var currency = receipt.Currency?.Trim();
            var currencyOk = string.IsNullOrEmpty(currency) || currency.Equals("VND", StringComparison.OrdinalIgnoreCase);
            var expected = (decimal)(receipt.Sotien ?? 0);
            var amountOk = expected > 0 && Math.Abs(expected - amount) < 1m;

            if (!currencyOk || !amountOk)
                logger.LogInformation(
                    "Bank amount check failed for receipt {No}: Currency='{Currency}' (ok={CurrencyOk}), Sotien={Expected}, received={Amount} (ok={AmountOk})",
                    receipt.SoPhieuthu, receipt.Currency, currencyOk, receipt.Sotien, amount, amountOk);

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

        private static string? Truncate(string? s, int max) =>
            s == null ? null : (s.Length <= max ? s : s[..max]);
    }
}
