using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using System.Globalization;
using System.Text;

namespace NVOAMASIS.Services
{
    /// <summary>
    /// Tổng hợp tình trạng thanh toán / hạch toán của từng lô hàng (HBL):
    /// - Tick "đã thanh toán" của các dòng Debit / Credit.
    /// - Phiếu thu (Debit.PhieuthuID hoặc BillNo chứa số HBL) / phiếu chi (ô HBL chứa số HBL):
    ///   đã duyệt, đã thanh toán, đã hạch toán (có chứng từ kế toán ghi sổ, AccountingVouchers.SourceId = Id phiếu).
    /// - Hoá đơn đầu ra: đã phát hành (có số HĐ điện tử), đã hạch toán (chứng từ BILLING, SourceId = "số HĐ nội bộ|customerId").
    /// Kết quả ghi vào HBL.financeNote (ghi chú tài chính của lô) và hiện thành cột ở lưới HBL / MBL / Job.
    /// </summary>
    public sealed class ShipmentFinanceStatusService
    {
        const int ChunkSize = 1000;
        const int VoucherCancelled = 3;
        const string BillingModule = "BILLING";

        readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ShipmentFinanceStatusService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

        public sealed record DocState(string No, bool Approved, bool Paid, bool Posted);

        public sealed class HblFinanceStatus
        {
            public Guid HblId { get; init; }
            public string HblNo { get; init; } = string.Empty;
            public int DebitLines { get; set; }
            public int DebitPaid { get; set; }
            public int CreditLines { get; set; }
            public int CreditPaid { get; set; }
            public Dictionary<string, (double Total, double Paid)> DebitByCur { get; } = new(StringComparer.OrdinalIgnoreCase);
            public Dictionary<string, (double Total, double Paid)> CreditByCur { get; } = new(StringComparer.OrdinalIgnoreCase);
            public List<DocState> Invoices { get; } = new();
            public List<DocState> Receipts { get; } = new();
            public List<DocState> Payments { get; } = new();

            public bool HasData => DebitLines + CreditLines + Invoices.Count + Receipts.Count + Payments.Count > 0;

            /// <summary>Đủ: mọi dòng debit/credit đã tick thanh toán, mọi HĐ / phiếu đã hạch toán.</summary>
            public bool Completed => HasData
                && DebitPaid == DebitLines && CreditPaid == CreditLines
                && Invoices.All(x => x.Posted) && Receipts.All(x => x.Posted) && Payments.All(x => x.Posted);

            /// <summary>Chuỗi ngắn hiện trên lưới.</summary>
            public string Short => !HasData ? "-" : (Completed ? "✔ " : "") + string.Join(" · ", ShortParts(this));

            /// <summary>Ghi chú chi tiết lưu vào HBL.financeNote.</summary>
            public string Note(DateTime at)
            {
                if (!HasData) return $"[{at:dd/MM/yyyy HH:mm}] Chưa có Debit/Credit, hoá đơn, phiếu thu/chi.";
                var sb = new StringBuilder();
                sb.Append($"[{at:dd/MM/yyyy HH:mm}] Tình trạng: {(Completed ? "ĐÃ HOÀN TẤT thanh toán & hạch toán" : "CHƯA HOÀN TẤT")}");
                sb.Append($"\nDebit: đã TT {DebitPaid}/{DebitLines} dòng{Money(DebitByCur)}");
                sb.Append($"\nCredit: đã TT {CreditPaid}/{CreditLines} dòng{Money(CreditByCur)}");
                sb.Append($"\nHoá đơn đã phát hành: {Invoices.Count}{DocList(Invoices, false)}");
                sb.Append($"\nPhiếu thu: {Receipts.Count}{DocList(Receipts, true)}");
                sb.Append($"\nPhiếu chi: {Payments.Count}{DocList(Payments, true)}");
                return sb.ToString();
            }

            static string Money(Dictionary<string, (double Total, double Paid)> map) =>
                map.Count == 0 ? "" : " (" + string.Join("; ", map.OrderBy(k => k.Key)
                    .Select(k => $"{k.Key} {Fmt(k.Value.Paid)}/{Fmt(k.Value.Total)}")) + ")";

            static string Fmt(double v) => v.ToString("#,##0.##", CultureInfo.InvariantCulture);

            static string DocList(List<DocState> docs, bool withPayState)
            {
                if (docs.Count == 0) return "";
                var posted = docs.Count(x => x.Posted);
                var head = withPayState
                    ? $" — duyệt {docs.Count(x => x.Approved)}, đã TT {docs.Count(x => x.Paid)}, hạch toán {posted}/{docs.Count}"
                    : $" — hạch toán {posted}/{docs.Count}";
                var items = docs.OrderBy(x => x.No, StringComparer.OrdinalIgnoreCase).Select(d =>
                {
                    var flags = new List<string>();
                    if (withPayState) { if (d.Approved) flags.Add("duyệt"); if (d.Paid) flags.Add("TT"); }
                    flags.Add(d.Posted ? "HT" : "chưa HT");
                    return $"{d.No} ({string.Join(", ", flags)})";
                });
                return head + ": " + string.Join(", ", items);
            }
        }

        static IEnumerable<string> ShortParts(HblFinanceStatus s)
        {
            if (s.DebitLines > 0) yield return $"Debit TT {s.DebitPaid}/{s.DebitLines}";
            if (s.CreditLines > 0) yield return $"Credit TT {s.CreditPaid}/{s.CreditLines}";
            if (s.Invoices.Count > 0) yield return $"HĐ {s.Invoices.Count} (HT {s.Invoices.Count(x => x.Posted)})";
            if (s.Receipts.Count > 0) yield return $"PT {s.Receipts.Count} (TT {s.Receipts.Count(x => x.Paid)}, HT {s.Receipts.Count(x => x.Posted)})";
            if (s.Payments.Count > 0) yield return $"PC {s.Payments.Count} (TT {s.Payments.Count(x => x.Paid)}, HT {s.Payments.Count(x => x.Posted)})";
        }

        /// <summary>Tổng hợp nhiều HBL (dùng cho dòng MBL / Job).</summary>
        public static string Aggregate(IEnumerable<HblFinanceStatus?> items)
        {
            var list = items.Where(x => x != null).Select(x => x!).ToList();
            if (list.Count == 0 || !list.Any(x => x.HasData)) return "-";
            var parts = new List<string> { $"HBL xong {list.Count(x => x.Completed)}/{list.Count}" };
            int dl = list.Sum(x => x.DebitLines), dp = list.Sum(x => x.DebitPaid);
            int cl = list.Sum(x => x.CreditLines), cp = list.Sum(x => x.CreditPaid);
            if (dl > 0) parts.Add($"Debit TT {dp}/{dl}");
            if (cl > 0) parts.Add($"Credit TT {cp}/{cl}");
            var inv = list.SelectMany(x => x.Invoices).DistinctBy(x => x.No).ToList();
            var rec = list.SelectMany(x => x.Receipts).DistinctBy(x => x.No).ToList();
            var pay = list.SelectMany(x => x.Payments).DistinctBy(x => x.No).ToList();
            if (inv.Count > 0) parts.Add($"HĐ {inv.Count} (HT {inv.Count(x => x.Posted)})");
            if (rec.Count > 0) parts.Add($"PT {rec.Count} (TT {rec.Count(x => x.Paid)}, HT {rec.Count(x => x.Posted)})");
            if (pay.Count > 0) parts.Add($"PC {pay.Count} (TT {pay.Count(x => x.Paid)}, HT {pay.Count(x => x.Posted)})");
            return (list.All(x => x.Completed) ? "✔ " : "") + string.Join(" · ", parts);
        }

        /// <summary>Ghi chú chi tiết cho nhiều HBL (tooltip dòng MBL / Job).</summary>
        public static string AggregateNote(IEnumerable<HblFinanceStatus?> items, DateTime at) =>
            string.Join("\n\n", items.Where(x => x != null).Select(x => $"HBL {x!.HblNo}\n{x.Note(at)}"));

        /// <summary>Kết quả đã tính, dùng trực tiếp trong lưới (HBL / MBL / Job).</summary>
        public sealed class FinanceView
        {
            public bool Loading { get; set; } = true;
            public DateTime At { get; set; } = DateTime.Now;
            public Dictionary<Guid, HblFinanceStatus> Map { get; set; } = new();
            /// <summary>Danh sách HBL theo MBL (chỉ dùng cho lưới MBL).</summary>
            public Dictionary<Guid, List<Guid>> HblsByMbl { get; set; } = new();

            HblFinanceStatus? Get(Guid id) => Map.TryGetValue(id, out var s) ? s : null;
            public string Short(Guid hblId) => Loading ? "…" : Get(hblId)?.Short ?? "-";
            public string Note(Guid hblId) => Get(hblId)?.Note(At) ?? "";
            public string ShortMany(IEnumerable<Guid> hblIds) => Loading ? "…" : Aggregate(hblIds.Select(Get));
            public string NoteMany(IEnumerable<Guid> hblIds) => AggregateNote(hblIds.Select(Get), At);
            public IEnumerable<Guid> HblsOfMbl(Guid mblId) => HblsByMbl.TryGetValue(mblId, out var l) ? l : Enumerable.Empty<Guid>();
        }

        public async Task<FinanceView> LoadForHblsAsync(IEnumerable<Guid> hblIds)
        {
            var map = await ComputeAsync(hblIds);
            return new FinanceView { Loading = false, At = DateTime.Now, Map = map };
        }

        public async Task<FinanceView> LoadForMblsAsync(IEnumerable<Guid> mblIdsIn)
        {
            var mblIds = mblIdsIn.Where(x => x != Guid.Empty).Distinct().ToList();
            var byMbl = new Dictionary<Guid, List<Guid>>();
            await using (var db = await _dbFactory.CreateDbContextAsync())
            {
                foreach (var chunk in mblIds.Chunk(ChunkSize))
                {
                    var ids = chunk.ToList();
                    var rows = await db.HBL.AsNoTracking().Where(x => ids.Contains(x.mblid))
                        .Select(x => new { x.mblid, x.hblID }).ToListAsync();
                    foreach (var g in rows.GroupBy(r => r.mblid))
                        byMbl[g.Key] = g.Select(r => r.hblID).Distinct().ToList();
                }
            }
            var view = await LoadForHblsAsync(byMbl.Values.SelectMany(x => x));
            view.HblsByMbl = byMbl;
            return view;
        }

        static readonly char[] DocNoSeparators = { ',', ';', '/', '|', ' ', '\t', '\r', '\n' };

        static IEnumerable<string> SplitDocNos(string? text) =>
            string.IsNullOrWhiteSpace(text)
                ? Enumerable.Empty<string>()
                : text.Split(DocNoSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        static string Cur(string? c) => string.IsNullOrWhiteSpace(c) ? "VND" : c.Trim().ToUpperInvariant();

        /// <summary>
        /// Tính tình trạng cho các HBL; <paramref name="saveNote"/> = true thì ghi vào HBL.financeNote những lô có thay đổi.
        /// </summary>
        public async Task<Dictionary<Guid, HblFinanceStatus>> ComputeAsync(IEnumerable<Guid> hblIdsIn, bool saveNote = true)
        {
            var result = new Dictionary<Guid, HblFinanceStatus>();
            var hblIds = hblIdsIn.Where(x => x != Guid.Empty).Distinct().ToList();
            if (hblIds.Count == 0) return result;

            await using var db = await _dbFactory.CreateDbContextAsync();

            // HBL
            var hbls = new List<(Guid Id, string No, string? Note)>();
            foreach (var chunk in hblIds.Chunk(ChunkSize))
            {
                var ids = chunk.ToList();
                var rows = await db.HBL.AsNoTracking().Where(x => ids.Contains(x.hblID))
                    .Select(x => new { x.hblID, x.hbl, x.financeNote }).ToListAsync();
                hbls.AddRange(rows.Select(r => (r.hblID, (r.hbl ?? "").Trim(), r.financeNote)));
            }
            foreach (var h in hbls) result[h.Id] = new HblFinanceStatus { HblId = h.Id, HblNo = h.No };
            var hblIdByNo = hbls.Where(x => x.No != "")
                .GroupBy(x => x.No, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList(), StringComparer.OrdinalIgnoreCase);

            var receiptLinks = new List<(Guid Hbl, Guid Phieu)>();
            var paymentLinks = new List<(Guid Hbl, Guid Phieu)>();
            var invoiceLinks = new List<(Guid Hbl, string No, string Key, bool Paid)>();

            foreach (var chunk in hblIds.Chunk(ChunkSize))
            {
                var ids = chunk.ToList();

                // Debit
                var debits = await db.Debit.AsNoTracking().Where(x => ids.Contains(x.hblid))
                    .Select(x => new { x.hblid, x.dathanhtoan, x.tiente, x.thanhtiensauthue, x.thanhtien, x.PhieuthuID }).ToListAsync();
                foreach (var d in debits)
                {
                    if (!result.TryGetValue(d.hblid, out var s)) continue;
                    s.DebitLines++;
                    var paid = d.dathanhtoan == true;
                    if (paid) s.DebitPaid++;
                    var amt = d.thanhtiensauthue ?? d.thanhtien ?? 0;
                    var k = Cur(d.tiente);
                    var cur = s.DebitByCur.TryGetValue(k, out var v) ? v : (0, 0);
                    s.DebitByCur[k] = (cur.Total + amt, cur.Paid + (paid ? amt : 0));
                    if (d.PhieuthuID is Guid pid && pid != Guid.Empty) receiptLinks.Add((d.hblid, pid));
                }

                // Credit
                var credits = await db.Credit.AsNoTracking().Where(x => ids.Contains(x.hblid))
                    .Select(x => new { x.hblid, x.dathanhtoan, x.tiente, x.thanhtiensauthue, x.thanhtien }).ToListAsync();
                foreach (var c in credits)
                {
                    if (!result.TryGetValue(c.hblid, out var s)) continue;
                    s.CreditLines++;
                    var paid = c.dathanhtoan == true;
                    if (paid) s.CreditPaid++;
                    var amt = c.thanhtiensauthue ?? c.thanhtien ?? 0;
                    var k = Cur(c.tiente);
                    var cur = s.CreditByCur.TryGetValue(k, out var v) ? v : (0, 0);
                    s.CreditByCur[k] = (cur.Total + amt, cur.Paid + (paid ? amt : 0));
                }

                // Hoá đơn đầu ra đã phát hành
                var invs = await db.HoaDonDauRa.AsNoTracking()
                    .Where(x => x.continued == true && ids.Contains(x.hblid)
                                && ((x.sohoadonDientu != null && x.sohoadonDientu != "") || x.BkavInvoiceNo > 0))
                    .Select(x => new { x.hblid, x.sohoadonDientu, x.BkavInvoiceNo, x.sohoadonNoibo, x.customerid, x.dathanhtoan })
                    .ToListAsync();
                foreach (var i in invs)
                {
                    var no = !string.IsNullOrWhiteSpace(i.sohoadonDientu) ? i.sohoadonDientu!.Trim() : i.BkavInvoiceNo?.ToString() ?? "";
                    if (no == "") continue;
                    var key = $"{(i.sohoadonNoibo ?? "").Trim()}|{i.customerid:N}";
                    invoiceLinks.Add((i.hblid, no, key, i.dathanhtoan == true));
                }
            }

            // Phiếu thu / phiếu chi khớp theo số HBL ghi trên phiếu
            if (hblIdByNo.Count > 0)
            {
                var billRows = await db.Phieuthu.AsNoTracking()
                    .Where(x => x.BillNo != null && x.BillNo != "" && x.SoPhieuthu != null && x.SoPhieuthu != "")
                    .Select(x => new { x.PhieuthuID, x.BillNo }).ToListAsync();
                foreach (var r in billRows)
                    foreach (var no in SplitDocNos(r.BillNo))
                        if (hblIdByNo.TryGetValue(no, out var ids))
                            foreach (var id in ids) receiptLinks.Add((id, r.PhieuthuID));

                var payRows = await db.Phieuchi.AsNoTracking()
                    .Where(x => x.Hbl != null && x.Hbl != "" && x.Sophieuchi != null && x.Sophieuchi != "")
                    .Select(x => new { x.PhieuchiID, x.Hbl }).ToListAsync();
                foreach (var r in payRows)
                    foreach (var no in SplitDocNos(r.Hbl))
                        if (hblIdByNo.TryGetValue(no, out var ids))
                            foreach (var id in ids) paymentLinks.Add((id, r.PhieuchiID));
            }
            receiptLinks = receiptLinks.Distinct().ToList();
            paymentLinks = paymentLinks.Distinct().ToList();

            var receipts = new Dictionary<Guid, (string No, bool Approved, bool Paid)>();
            foreach (var chunk in receiptLinks.Select(x => x.Phieu).Distinct().Chunk(ChunkSize))
            {
                var ids = chunk.ToList();
                var rows = await db.Phieuthu.AsNoTracking().Where(x => ids.Contains(x.PhieuthuID))
                    .Select(x => new { x.PhieuthuID, x.SoPhieuthu, x.Approve, x.dathanhtoan }).ToListAsync();
                foreach (var r in rows)
                    if (!string.IsNullOrWhiteSpace(r.SoPhieuthu))
                        receipts[r.PhieuthuID] = (r.SoPhieuthu!.Trim(), r.Approve == true, r.dathanhtoan == true);
            }
            var payments = new Dictionary<Guid, (string No, bool Approved, bool Paid)>();
            foreach (var chunk in paymentLinks.Select(x => x.Phieu).Distinct().Chunk(ChunkSize))
            {
                var ids = chunk.ToList();
                var rows = await db.Phieuchi.AsNoTracking().Where(x => ids.Contains(x.PhieuchiID))
                    .Select(x => new { x.PhieuchiID, x.Sophieuchi, x.Approve, x.dathanhtoan }).ToListAsync();
                foreach (var r in rows)
                    if (!string.IsNullOrWhiteSpace(r.Sophieuchi))
                        payments[r.PhieuchiID] = (r.Sophieuchi!.Trim(), r.Approve == true, r.dathanhtoan == true);
            }

            // Chứng từ kế toán đã ghi sổ
            var sourceKeys = receipts.Keys.Concat(payments.Keys).Select(x => x.ToString())
                .Concat(invoiceLinks.Select(x => x.Key))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var posted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var chunk in sourceKeys.Chunk(ChunkSize))
            {
                var keys = chunk.ToList();
                var rows = await db.AccountingVouchers.AsNoTracking()
                    .Where(v => v.SourceId != null && keys.Contains(v.SourceId) && v.Status != VoucherCancelled
                                && (v.Status == 2 || v.Ghiso == true || (v.PostedBy != null && v.PostedBy != "")))
                    .Select(v => new { v.SourceId, v.SourceModule }).ToListAsync();
                foreach (var r in rows)
                {
                    // Khoá hoá đơn ("số|khách") chỉ tính chứng từ BILLING; khoá phiếu là Guid nên không trùng.
                    if (r.SourceId!.Contains('|') && !string.Equals(r.SourceModule, BillingModule, StringComparison.OrdinalIgnoreCase)) continue;
                    posted.Add(r.SourceId.Trim());
                }
            }

            static void AddDoc(List<DocState> list, DocState d)
            {
                if (!list.Any(x => string.Equals(x.No, d.No, StringComparison.OrdinalIgnoreCase))) list.Add(d);
            }
            foreach (var (hbl, no, key, paid) in invoiceLinks)
                if (result.TryGetValue(hbl, out var s))
                    AddDoc(s.Invoices, new DocState(no, true, paid, posted.Contains(key)));
            foreach (var (hbl, pid) in receiptLinks)
                if (result.TryGetValue(hbl, out var s) && receipts.TryGetValue(pid, out var p))
                    AddDoc(s.Receipts, new DocState(p.No, p.Approved, p.Paid, posted.Contains(pid.ToString())));
            foreach (var (hbl, pid) in paymentLinks)
                if (result.TryGetValue(hbl, out var s) && payments.TryGetValue(pid, out var p))
                    AddDoc(s.Payments, new DocState(p.No, p.Approved, p.Paid, posted.Contains(pid.ToString())));

            if (saveNote)
                await SaveNotesAsync(db, hbls, result);

            return result;
        }

        /// <summary>Ghi chú cũ bỏ dòng thời gian đầu để so sánh nội dung (không ghi lại khi chỉ khác giờ).</summary>
        static string Body(string? note)
        {
            if (string.IsNullOrEmpty(note)) return string.Empty;
            var i = note.IndexOf(']');
            return note.StartsWith('[') && i > 0 ? note[(i + 1)..] : note;
        }

        static async Task SaveNotesAsync(AppDbContext db, List<(Guid Id, string No, string? Note)> hbls, Dictionary<Guid, HblFinanceStatus> result)
        {
            var now = DateTime.Now;
            var changed = hbls
                .Select(h => (h.Id, Old: h.Note, New: result[h.Id].Note(now)))
                .Where(x => Body(x.Old) != Body(x.New))
                .ToList();
            if (changed.Count == 0) return;
            try
            {
                foreach (var part in changed.Chunk(300))
                {
                    db.ChangeTracker.Clear();
                    foreach (var c in part)
                    {
                        var stub = new M_HBL { hblID = c.Id };
                        db.HBL.Attach(stub);
                        stub.financeNote = c.New;
                        stub.financeNoteDate = now;
                        db.Entry(stub).Property(x => x.financeNote).IsModified = true;
                        db.Entry(stub).Property(x => x.financeNoteDate).IsModified = true;
                    }
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                // Không làm hỏng lưới nếu ghi chú không lưu được (vd. chưa chạy script thêm cột).
            }
        }
    }
}
