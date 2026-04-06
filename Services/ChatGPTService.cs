using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using System.Text;
using System.Text.Json;

namespace NVOAMASIS.Services
{
    public class ChatGPTService
    {
        private readonly HttpClient _httpClient;
        private readonly ChatGPTSettings _settings;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<ChatGPTService> _logger;
        private readonly WebSearchService _webSearchService;

        public ChatGPTService(
            HttpClient httpClient,
            IOptions<ChatGPTSettings> settings,
            IDbContextFactory<AppDbContext> dbFactory,
            ILogger<ChatGPTService> logger,
            WebSearchService webSearchService)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _dbFactory = dbFactory;
            _logger = logger;
            _webSearchService = webSearchService;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }

        #region Send Message to OpenAI

        public async Task<string> SendMessageAsync(List<ChatMessage> history, string userMessage)
        {
            try
            {
                // 1. Try deep query first
                var dataContext = await QueryDataAsync(userMessage);

                // 2. Build system prompt
                var systemPrompt = BuildSystemPrompt(dataContext);

                // 3. Build messages
                var messages = new List<OpenAIMessage>
                {
                    new() { role = "system", content = systemPrompt }
                };

                // Add last 20 messages from history
                foreach (var msg in history.TakeLast(20))
                {
                    messages.Add(new OpenAIMessage { role = msg.Role, content = msg.Content });
                }

                // Add current message
                messages.Add(new OpenAIMessage { role = "user", content = userMessage });

                var request = new OpenAIChatRequest
                {
                    model = _settings.Model,
                    messages = messages,
                    temperature = _settings.Temperature,
                    max_tokens = _settings.MaxTokens
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/chat/completions", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {Status} - {Body}", response.StatusCode, responseBody);
                    return $"⚠️ Lỗi API: {response.StatusCode}. Vui lòng thử lại.";
                }

                var chatResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(responseBody);
                return chatResponse?.choices?.FirstOrDefault()?.message?.content ?? "Không có phản hồi.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMessageAsync failed");
                return $"⚠️ Lỗi: {ex.Message}";
            }
        }

        #endregion

        #region System Prompt

        private string BuildSystemPrompt(string dataContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bạn là AI Assistant thông minh của hệ thống LMS-NVOAMASIS (Logistics Management System - Non-Vessel Operating Amasis).");
            sb.AppendLine("Hệ thống quản lý toàn diện hoạt động logistics, vận tải biển, hàng không, trucking, kho bãi.");
            sb.AppendLine();
            sb.AppendLine("## CẤU TRÚC DATABASE CHÍNH:");
            sb.AppendLine("- **Job** (M_Job): Quản lý Job vận chuyển - JobNo, Loai (SEA/AIR/TRUCK), statusJob, Branch, Salecode");
            sb.AppendLine("- **MBL** (M_MBL): Master Bill of Lading - Mbl, Vessel, Voy, Polname, Podname, Shipper, Consignee, CustomerID");
            sb.AppendLine("- **HBL** (M_HBL): House Bill of Lading - hbl, bkno, shipper, consignee, vessel, polname, podname, CustomerID");
            sb.AppendLine("- **Container** (M_Container): Container - CONTAINER_NO, CTN_SIZE_TYPE, Seal, pkgs, GrossWeight, cbm");
            sb.AppendLine("- **Customer** (M_Customer): Khách hàng - Customer_Code, COMPANY, TaxCode, Address, SaleName, Type");
            sb.AppendLine("- **Booking** (M_Booking): Booking hãng tàu - POD_ID, POL_ID, SL20GP/40GP/40HC, Currency, SCNO");
            sb.AppendLine("- **Quotation** (M_Quotation): Báo giá - quotationNo, Subject, pol, pod, shippingline, SaleName, validDate");
            sb.AppendLine("- **Debit** (M_Debit): Công nợ phải thu - debitno, dongia, soluong, thanhtien, tiente, thue");
            sb.AppendLine("- **Credit** (M_Credit): Công nợ phải trả - dongia, soluong, thanhtien, tiente, thue");
            sb.AppendLine("- **PhieuThu** (M_PhieuThu): Phiếu thu tiền - SoPhieuthu, Sotien, Currency, Ngay, PTTT");
            sb.AppendLine("- **PhieuChi** (M_PhieuChi): Phiếu chi tiền - Sophieuchi, Sotien, Currency, Ngay, PTTT");
            sb.AppendLine("- **HoaDonDauRa** (M_HoaDonDauRa): Hóa đơn bán ra - sohoadonNoibo, sohoadonDientu, thanhtien");
            sb.AppendLine("- **HoaDonDauVao** (M_HoaDonDauVao): Hóa đơn mua vào - sohoadonNoibo, sohoadonDientu, thanhtien");
            sb.AppendLine("- **YeuCauTrucking** (M_YeuCauTrucking): Yêu cầu trucking - DiaDiemNhanHang, DiaDiemTraHang, LoaiCont");
            sb.AppendLine("- **LenhDieuXe** (M_LenhDieuXe): Lệnh điều xe - Taixe, Nhaxe, Soxe, Pol, Pod, GiaCost, Tamung");
            sb.AppendLine("- **RFQ** (M_RFQ): Request for Quotation - ServiceType, POL, POD, CargoDescription");
            sb.AppendLine("- **Product_Price** (M_Product_Price): Giá sản phẩm - agent, line, Trangthai, loai");
            sb.AppendLine("- **IssueReports** (M_IssueReports): Báo cáo lỗi - Title, Severity, Status, Reporter");
            sb.AppendLine("- **Notification**: Thông báo - Message, CreatedAt, IsRead");
            sb.AppendLine("- **UserList** (AuthUser): Người dùng - Usr, Name, Email, Department, Branch");
            sb.AppendLine("- **LeaveRequest**: Nghỉ phép - LeaveType, StartDate, EndDate, Status");
            sb.AppendLine("- **AttendanceLog**: Chấm công - CheckInTime, Session, IpAddress");
            sb.AppendLine("- **TaxInvoice** (M_TaxInvoice): Hóa đơn thuế");
            sb.AppendLine("- **DNTU** (M_DNTU): Đề nghị tạm ứng");
            sb.AppendLine("- **DNTT** (M_DNTT_Logistics): Đề nghị thanh toán");
            sb.AppendLine("- **TKHQ**: Tờ khai hải quan");
            sb.AppendLine("- **Charge** (ChargeModel): Danh mục phí");
            sb.AppendLine("- **Port** (PortModel): Danh mục cảng");
            sb.AppendLine("- **VesselSpace**: Quản lý space tàu");
            sb.AppendLine("- **Stock** (M_Stock): Tồn kho container");
            sb.AppendLine();
            sb.AppendLine("## QUAN HỆ CHÍNH:");
            sb.AppendLine("- Job → MBL (1-n) → HBL (1-n) → Container (1-n)");
            sb.AppendLine("- HBL → Debit (1-n), Credit (1-n)");
            sb.AppendLine("- Customer → Booking, Quotation, Debit, Credit, PhieuThu, PhieuChi");
            sb.AppendLine("- YeuCauTrucking → LenhDieuXe (1-n)");
            sb.AppendLine("- RFQ → Product_Price (1-n) → Product_Price_Detail_ALL (1-n)");
            sb.AppendLine();
            sb.AppendLine("## QUY TẮC TRẢ LỜI:");
            sb.AppendLine("1. Trả lời bằng tiếng Việt, chuyên nghiệp, rõ ràng");
            sb.AppendLine("2. Dùng markdown: bảng, bold, list để dễ đọc");
            sb.AppendLine("3. Với dữ liệu số: format đúng định dạng tiền tệ VND/USD");
            sb.AppendLine("4. Nếu có dữ liệu hệ thống kèm theo → phân tích chi tiết");
            sb.AppendLine("5. Nếu không có dữ liệu → trả lời dựa trên kiến thức chung về logistics");
            sb.AppendLine("6. Luôn thân thiện và hỗ trợ tối đa");

            if (!string.IsNullOrWhiteSpace(dataContext))
            {
                sb.AppendLine();
                sb.AppendLine("## DỮ LIỆU HỆ THỐNG LIÊN QUAN:");
                sb.AppendLine(dataContext);
            }

            return sb.ToString();
        }

        #endregion

        #region Deep Query

        public async Task<string> QueryDataAsync(string userQuestion)
        {
            try
            {
                var q = userQuestion.ToLower().Trim();

                // Check if external query
                if (IsExternalQuery(q))
                {
                    var webResult = await _webSearchService.SearchAsync(userQuestion);
                    return $"[KẾT QUẢ TÌM KIẾM WEB]\n{webResult}";
                }

                var sb = new StringBuilder();

                // Detect intent and query relevant data
                if (ContainsAny(q, "job", "lô hàng", "shipment", "lo hang", "công việc", "vận chuyển"))
                    sb.AppendLine(await GetJobDetailedAsync(q));

                if (ContainsAny(q, "customer", "khách hàng", "khach hang", "đối tác", "doi tac", "kh"))
                    sb.AppendLine(await GetCustomerDetailedAsync(q));

                if (ContainsAny(q, "booking", "đặt chỗ", "dat cho", "book"))
                    sb.AppendLine(await GetBookingDetailedAsync(q));

                if (ContainsAny(q, "quotation", "báo giá", "bao gia", "quote", "quo"))
                    sb.AppendLine(await GetQuotationDetailedAsync(q));

                if (ContainsAny(q, "debit", "credit", "công nợ", "cong no", "doanh thu", "chi phí", "chi phi", "phải thu", "phải trả"))
                    sb.AppendLine(await GetDebitCreditDetailedAsync(q));

                if (ContainsAny(q, "container", "cont", "teu", "20gp", "40gp", "40hc"))
                    sb.AppendLine(await GetContainerDetailedAsync(q));

                if (ContainsAny(q, "hbl", "house bill", "vận đơn", "van don"))
                    sb.AppendLine(await GetHBLDetailedAsync(q));

                if (ContainsAny(q, "mbl", "master bill"))
                    sb.AppendLine(await GetMBLDetailedAsync(q));

                if (ContainsAny(q, "trucking", "xe tải", "xe tai", "điều xe", "dieu xe", "lệnh điều", "lenh dieu", "vận tải", "van tai"))
                    sb.AppendLine(await GetTruckingDetailedAsync(q));

                if (ContainsAny(q, "phiếu thu", "phieu thu", "thu tiền", "thu tien"))
                    sb.AppendLine(await GetPhieuThuDetailedAsync(q));

                if (ContainsAny(q, "phiếu chi", "phieu chi", "chi tiền", "chi tien"))
                    sb.AppendLine(await GetPhieuChiDetailedAsync(q));

                if (ContainsAny(q, "rfq", "request for quotation", "yêu cầu báo giá", "yeu cau bao gia"))
                    sb.AppendLine(await GetRFQDetailedAsync(q));

                if (ContainsAny(q, "issue", "bug", "lỗi", "báo lỗi", "bao loi", "sự cố", "su co"))
                    sb.AppendLine(await GetIssueDetailedAsync(q));

                if (ContainsAny(q, "user", "người dùng", "nguoi dung", "nhân viên", "nhan vien", "tài khoản", "tai khoan"))
                    sb.AppendLine(await GetUserDetailedAsync(q));

                if (ContainsAny(q, "hóa đơn", "hoa don", "invoice", "thuế", "thue", "vat"))
                    sb.AppendLine(await GetInvoiceDetailedAsync(q));

                if (ContainsAny(q, "tổng quan", "tong quan", "overview", "thống kê", "thong ke", "dashboard", "bao nhieu", "bao nhiêu", "tổng", "tong"))
                    sb.AppendLine(await GetFullOverviewAsync());

                // If no specific intent matched, get overview
                if (sb.Length == 0)
                {
                    sb.AppendLine(await GetFullOverviewAsync());
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryDataAsync failed");
                return "";
            }
        }

        #endregion

        #region External Query Detection

        private bool IsExternalQuery(string q)
        {
            // Internal keywords - NOT external
            var internalKeywords = new[]
            {
                "job", "customer", "khách hàng", "booking", "quotation", "báo giá", "debit", "credit",
                "container", "hbl", "mbl", "trucking", "phiếu thu", "phiếu chi", "rfq", "issue",
                "user", "hóa đơn", "invoice", "tổng quan", "thống kê", "dashboard", "lô hàng",
                "shipment", "công nợ", "doanh thu", "chi phí", "vận đơn", "lệnh điều xe",
                "điều xe", "nhân viên", "tài khoản", "overview", "hệ thống", "he thong",
                "dữ liệu", "du lieu", "report", "báo cáo", "bao cao", "profit", "lợi nhuận"
            };

            // If contains internal keyword → not external
            if (internalKeywords.Any(kw => q.Contains(kw)))
                return false;

            // External indicators
            var externalIndicators = new[]
            {
                "tìm kiếm", "tim kiem", "search", "google", "internet", "web",
                "tin tức", "tin tuc", "news", "thời sự", "thoi su",
                "thời tiết", "thoi tiet", "weather",
                "giá vàng", "gia vang", "tỷ giá", "ty gia", "exchange rate",
                "wikipedia", "wiki",
                "ai là", "ai la", "who is", "what is",
                "lịch sử", "lich su", "history of",
                "cước tàu", "cuoc tau", "freight rate", "giá cước", "gia cuoc",
                "imf", "world bank", "un", "wto",
                "luật", "luat", "law", "regulation", "quy định", "quy dinh"
            };

            return externalIndicators.Any(kw => q.Contains(kw));
        }

        #endregion

        #region Detailed Query Methods

        private async Task<string> GetJobDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var jobs = await db.Job.AsNoTracking()
                .OrderByDescending(x => x.Datecreate)
                .Take(20)
                .ToListAsync();

            var total = await db.Job.AsNoTracking().CountAsync();
            var approved = await db.Job.AsNoTracking().CountAsync(x => x.Approve == true);

            var sb = new StringBuilder();
            sb.AppendLine($"### 📦 THỐNG KÊ JOB (Tổng: {total}, Đã duyệt: {approved})");
            sb.AppendLine("| # | Job No | Loại | Trạng thái | Chi nhánh | Ngày tạo |");
            sb.AppendLine("|---|--------|------|------------|-----------|----------|");

            foreach (var j in jobs.Take(15))
            {
                sb.AppendLine($"| | {j.JobNo} | {j.Loai} | {j.statusJob} | {j.Branch} | {j.Datecreate?.ToString("dd/MM/yyyy")} |");
            }

            // Group by type
            var byType = jobs.GroupBy(x => x.Loai).Select(g => $"{g.Key}: {g.Count()}");
            sb.AppendLine($"\n**Phân loại:** {string.Join(", ", byType)}");

            // Group by branch
            var byBranch = jobs.GroupBy(x => x.Branch).Select(g => $"{g.Key}: {g.Count()}");
            sb.AppendLine($"**Theo chi nhánh:** {string.Join(", ", byBranch)}");

            return sb.ToString();
        }

        private async Task<string> GetCustomerDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.Customer.AsNoTracking().CountAsync();
            var approved = await db.Customer.AsNoTracking().CountAsync(x => x.Approve == true);
            var customers = await db.Customer.AsNoTracking()
                .OrderByDescending(x => x.Updatetime)
                .Take(15)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 👥 KHÁCH HÀNG (Tổng: {total}, Đã duyệt: {approved})");
            sb.AppendLine("| Mã KH | Tên công ty | Mã số thuế | Sale | Loại |");
            sb.AppendLine("|-------|------------|------------|------|------|");

            foreach (var c in customers)
            {
                sb.AppendLine($"| {c.Customer_Code} | {c.COMPANY?.Truncate(30)} | {c.TaxCode} | {c.SaleName} | {c.Type} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetBookingDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.Booking.AsNoTracking().CountAsync();
            var bookings = await db.Booking.AsNoTracking()
                .OrderByDescending(x => x.updatetime)
                .Take(15)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 📋 BOOKING (Tổng: {total})");
            sb.AppendLine("| SCNO | 20GP | 40GP | 40HC | Currency | Validate | Status |");
            sb.AppendLine("|------|------|------|------|----------|----------|--------|");

            foreach (var b in bookings)
            {
                sb.AppendLine($"| {b.SCNO} | {b.SL20GPOwner} | {b.SL40GPOwner} | {b.SL40HCOwner} | {b.Currency} | {b.Validate?.ToString("dd/MM/yyyy")} | {(b.approve == true ? "✅" : "⏳")} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetQuotationDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.Quotation.AsNoTracking().CountAsync();
            var quotations = await db.Quotation.AsNoTracking()
                .OrderByDescending(x => x.dated)
                .Take(15)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 📝 BÁO GIÁ (Tổng: {total})");
            sb.AppendLine("| Quotation No | Subject | POL | POD | Shipping Line | Sale | Valid Date |");
            sb.AppendLine("|-------------|---------|-----|-----|---------------|------|------------|");

            foreach (var qt in quotations)
            {
                sb.AppendLine($"| {qt.quotationNo} | {qt.Subject?.Truncate(25)} | {qt.pol} | {qt.pod} | {qt.shippingline} | {qt.SaleName} | {qt.validDate?.ToString("dd/MM/yyyy")} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetDebitCreditDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var totalDebit = await db.Debit.AsNoTracking().CountAsync();
            var totalCredit = await db.Credit.AsNoTracking().CountAsync();

            var debits = await db.Debit.AsNoTracking()
                .OrderByDescending(x => x.dateupdate)
                .Take(10)
                .ToListAsync();

            var sumDebitVND = debits.Where(x => x.tiente == "VND").Sum(x => x.thanhtiensauthue ?? 0);
            var sumDebitUSD = debits.Where(x => x.tiente == "USD").Sum(x => x.thanhtiensauthue ?? 0);

            var credits = await db.Credit.AsNoTracking()
                .OrderByDescending(x => x.dateupdate)
                .Take(10)
                .ToListAsync();

            var sumCreditVND = credits.Where(x => x.tiente == "VND").Sum(x => x.thanhtiensauthue ?? 0);
            var sumCreditUSD = credits.Where(x => x.tiente == "USD").Sum(x => x.thanhtiensauthue ?? 0);

            var sb = new StringBuilder();
            sb.AppendLine($"### 💰 CÔNG NỢ");
            sb.AppendLine($"- **Debit (Phải thu):** {totalDebit} records | VND: {sumDebitVND:N0} | USD: {sumDebitUSD:N2}");
            sb.AppendLine($"- **Credit (Phải trả):** {totalCredit} records | VND: {sumCreditVND:N0} | USD: {sumCreditUSD:N2}");
            sb.AppendLine($"- **Lợi nhuận ước tính (VND):** {(sumDebitVND - sumCreditVND):N0}");
            sb.AppendLine($"- **Lợi nhuận ước tính (USD):** {(sumDebitUSD - sumCreditUSD):N2}");

            return sb.ToString();
        }

        private async Task<string> GetContainerDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.Container.AsNoTracking().CountAsync();
            var containers = await db.Container.AsNoTracking()
                .OrderByDescending(x => x.UPDATETIME)
                .Take(15)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 📦 CONTAINER (Tổng: {total})");

            var byType = containers.GroupBy(x => x.CTN_SIZE_TYPE).Select(g => $"{g.Key}: {g.Count()}");
            sb.AppendLine($"**Phân loại:** {string.Join(", ", byType)}");

            sb.AppendLine("| Container No | Size/Type | Seal | Packages | GW | CBM |");
            sb.AppendLine("|-------------|-----------|------|----------|----|----|");

            foreach (var c in containers.Take(10))
            {
                sb.AppendLine($"| {c.CONTAINER_NO} | {c.CTN_SIZE_TYPE} | {c.Seal} | {c.pkgs} | {c.GrossWeight} | {c.cbm} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetHBLDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.HBL.AsNoTracking().CountAsync();
            var hbls = await db.HBL.AsNoTracking()
                .OrderByDescending(x => x.dateupdate)
                .Take(15)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 📄 HBL - House Bill of Lading (Tổng: {total})");
            sb.AppendLine("| HBL No | BK No | POL | POD | Vessel | Status |");
            sb.AppendLine("|--------|-------|-----|-----|--------|--------|");

            foreach (var h in hbls)
            {
                sb.AppendLine($"| {h.hbl} | {h.bkno} | {h.polname?.Truncate(15)} | {h.podname?.Truncate(15)} | {h.vessel} | {h.statusHBL} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetMBLDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.MBL.AsNoTracking().CountAsync();
            var mbls = await db.MBL.AsNoTracking()
                .OrderByDescending(x => x.DateUpdate)
                .Take(15)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 📄 MBL - Master Bill of Lading (Tổng: {total})");
            sb.AppendLine("| MBL No | BK No | POL | POD | Vessel/Voy | Status |");
            sb.AppendLine("|--------|-------|-----|-----|------------|--------|");

            foreach (var m in mbls)
            {
                sb.AppendLine($"| {m.Mbl} | {m.Bkno} | {m.Polname?.Truncate(15)} | {m.Podname?.Truncate(15)} | {m.Vessel}/{m.Voy} | {m.statusMBL} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetTruckingDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var totalYC = await db.YeuCauTrucking.AsNoTracking().CountAsync();
            var totalLDX = await db.LenhDieuXe.AsNoTracking().CountAsync();

            var ycs = await db.YeuCauTrucking.AsNoTracking()
                .OrderByDescending(x => x.YeuCauTruckingNo)
                .Take(10)
                .ToListAsync();

            var ldxs = await db.LenhDieuXe.AsNoTracking()
                .OrderByDescending(x => x.Lenhdieuxeno)
                .Take(10)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 🚛 TRUCKING");
            sb.AppendLine($"- **Yêu cầu Trucking:** {totalYC}");
            sb.AppendLine($"- **Lệnh Điều Xe:** {totalLDX}");
            sb.AppendLine();

            sb.AppendLine("**Yêu cầu Trucking gần nhất:**");
            sb.AppendLine("| Số YC | Người YC | Nhận hàng | Trả hàng | Trạng thái |");
            sb.AppendLine("|-------|---------|-----------|----------|------------|");
            foreach (var yc in ycs)
            {
                sb.AppendLine($"| {yc.YeuCauTruckingNo} | {yc.Nguoiyeucau} | {yc.DiaDiemNhanHang?.Truncate(20)} | {yc.DiaDiemTraHang?.Truncate(20)} | {yc.Trangthai} |");
            }

            sb.AppendLine();
            sb.AppendLine("**Lệnh Điều Xe gần nhất:**");
            sb.AppendLine("| Số LĐX | Nhà xe | Tài xế | Số xe | Giá Cost | Tạm ứng |");
            sb.AppendLine("|--------|--------|--------|-------|----------|---------|");
            foreach (var ldx in ldxs)
            {
                sb.AppendLine($"| {ldx.Lenhdieuxeno} | {ldx.Nhaxe?.Truncate(15)} | {ldx.Taixe?.Truncate(15)} | {ldx.Soxe} | {ldx.GiaCost:N0} | {ldx.Tamung:N0} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetPhieuThuDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.Phieuthu.AsNoTracking().CountAsync();
            var items = await db.Phieuthu.AsNoTracking()
                .OrderByDescending(x => x.Ngay)
                .Take(10)
                .ToListAsync();

            var sumVND = items.Where(x => x.Currency == "VND").Sum(x => x.Sotien ?? 0);
            var sumUSD = items.Where(x => x.Currency == "USD").Sum(x => x.Sotien ?? 0);

            var sb = new StringBuilder();
            sb.AppendLine($"### 📥 PHIẾU THU (Tổng: {total})");
            sb.AppendLine($"**Tổng thu gần đây:** VND: {sumVND:N0} | USD: {sumUSD:N2}");
            sb.AppendLine("| Số PT | Ngày | Số tiền | Tiền tệ | Nội dung | PTTT |");
            sb.AppendLine("|-------|------|--------|---------|---------|------|");

            foreach (var pt in items)
            {
                sb.AppendLine($"| {pt.SoPhieuthu} | {pt.Ngay?.ToString("dd/MM/yyyy")} | {pt.Sotien:N0} | {pt.Currency} | {pt.Noidung?.Truncate(25)} | {pt.PTTT} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetPhieuChiDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.Phieuchi.AsNoTracking().CountAsync();
            var items = await db.Phieuchi.AsNoTracking()
                .OrderByDescending(x => x.Ngay)
                .Take(10)
                .ToListAsync();

            var sumVND = items.Where(x => x.Currency == "VND").Sum(x => x.Sotien ?? 0);
            var sumUSD = items.Where(x => x.Currency == "USD").Sum(x => x.Sotien ?? 0);

            var sb = new StringBuilder();
            sb.AppendLine($"### 📤 PHIẾU CHI (Tổng: {total})");
            sb.AppendLine($"**Tổng chi gần đây:** VND: {sumVND:N0} | USD: {sumUSD:N2}");
            sb.AppendLine("| Số PC | Ngày | Số tiền | Tiền tệ | Nội dung | PTTT |");
            sb.AppendLine("|-------|------|--------|---------|---------|------|");

            foreach (var pc in items)
            {
                sb.AppendLine($"| {pc.Sophieuchi} | {pc.Ngay?.ToString("dd/MM/yyyy")} | {pc.Sotien:N0} | {pc.Currency} | {pc.Noidung?.Truncate(25)} | {pc.PTTT} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetRFQDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.RFQ.AsNoTracking().CountAsync();
            var items = await db.RFQ.AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 📨 RFQ - Request For Quotation (Tổng: {total})");
            sb.AppendLine("| RFQ No | Service | POL | POD | Status | Sales | Deadline |");
            sb.AppendLine("|--------|---------|-----|-----|--------|-------|----------|");

            foreach (var r in items)
            {
                sb.AppendLine($"| {r.RFQNo} | {r.ServiceType} | {r.POL?.Truncate(12)} | {r.POD?.Truncate(12)} | {r.RFQStatus} | {r.SalesPerson} | {r.QuoteDeadline?.ToString("dd/MM/yyyy")} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetIssueDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.IssueReports.AsNoTracking().CountAsync();
            var items = await db.IssueReports.AsNoTracking()
                .OrderByDescending(x => x.ReportDate)
                .Take(10)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 🐛 ISSUE REPORTS (Tổng: {total})");
            sb.AppendLine("| No | Title | Severity | Status | Reporter | Assigned |");
            sb.AppendLine("|----|-------|----------|--------|----------|----------|");

            foreach (var i in items)
            {
                sb.AppendLine($"| {i.No} | {i.Title?.Truncate(25)} | {i.Severity} | {i.Status} | {i.Reporter} | {i.AssignedTo} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetUserDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var total = await db.UserList.AsNoTracking().CountAsync();
            var users = await db.UserList.AsNoTracking().ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 👤 NGƯỜI DÙNG (Tổng: {total})");

            var byDept = users.GroupBy(x => x.Department).Select(g => $"{g.Key}: {g.Count()}");
            sb.AppendLine($"**Theo phòng ban:** {string.Join(", ", byDept)}");

            var byBranch = users.GroupBy(x => x.Branch).Select(g => $"{g.Key}: {g.Count()}");
            sb.AppendLine($"**Theo chi nhánh:** {string.Join(", ", byBranch)}");

            sb.AppendLine("| Username | Tên | Email | Phòng ban | Chi nhánh |");
            sb.AppendLine("|----------|-----|-------|-----------|-----------|");
            foreach (var u in users.Take(15))
            {
                sb.AppendLine($"| {u.Usr} | {u.Name} | {u.Email} | {u.Department} | {u.Branch} |");
            }

            return sb.ToString();
        }

        private async Task<string> GetInvoiceDetailedAsync(string q)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var totalOut = await db.HoaDonDauRa.AsNoTracking().CountAsync();
            var totalIn = await db.HoaDonDauVao.AsNoTracking().CountAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"### 🧾 HÓA ĐƠN");
            sb.AppendLine($"- **Hóa đơn đầu ra (bán):** {totalOut}");
            sb.AppendLine($"- **Hóa đơn đầu vào (mua):** {totalIn}");

            var outInvoices = await db.HoaDonDauRa.AsNoTracking()
                .OrderByDescending(x => x.ngayphathanhhoadonDientu)
                .Take(10)
                .ToListAsync();

            var sumOutVND = outInvoices.Where(x => x.tiente == "VND").Sum(x => x.thanhtiensauthue ?? 0);
            sb.AppendLine($"**Tổng HĐ đầu ra gần nhất (VND):** {sumOutVND:N0}");

            return sb.ToString();
        }

        private async Task<string> GetFullOverviewAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            db.ChangeTracker.Clear();

            var sb = new StringBuilder();
            sb.AppendLine("### 📊 TỔNG QUAN HỆ THỐNG LMS-NVOAMASIS");
            sb.AppendLine();

            try { sb.AppendLine($"- 📦 **Job:** {await db.Job.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📄 **MBL:** {await db.MBL.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📄 **HBL:** {await db.HBL.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📦 **Container:** {await db.Container.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 👥 **Khách hàng:** {await db.Customer.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📋 **Booking:** {await db.Booking.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📝 **Báo giá:** {await db.Quotation.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 💳 **Debit:** {await db.Debit.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 💳 **Credit:** {await db.Credit.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📥 **Phiếu thu:** {await db.Phieuthu.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📤 **Phiếu chi:** {await db.Phieuchi.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 🚛 **Yêu cầu Trucking:** {await db.YeuCauTrucking.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 🚛 **Lệnh Điều Xe:** {await db.LenhDieuXe.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 📨 **RFQ:** {await db.RFQ.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 🧾 **Hóa đơn đầu ra:** {await db.HoaDonDauRa.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 🧾 **Hóa đơn đầu vào:** {await db.HoaDonDauVao.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 🐛 **Issue Reports:** {await db.IssueReports.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 👤 **Người dùng:** {await db.UserList.AsNoTracking().CountAsync()}"); } catch { }
            try { sb.AppendLine($"- 🔔 **Thông báo:** {await db.Notifications.AsNoTracking().CountAsync()}"); } catch { }

            return sb.ToString();
        }

        #endregion

        #region Helpers

        private bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }

    /// <summary>
    /// String extension for truncating
    /// </summary>
    public static class StringTruncateExtension
    {
        public static string? Truncate(this string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value[..maxLength] + "...";
        }
    }
}
