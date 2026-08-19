using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Accounting.B09.Domain;
using NVOAMASIS.Data;

namespace NVOAMASIS.Accounting.B09.Seed;

public sealed class B09TemplateSeeder
{
    private readonly AppDbContext _db;

    public B09TemplateSeeder(AppDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Set<FinancialStatementNoteTemplate>().AnyAsync(x => x.Code == "B09-DN" && x.Version == "2025.1", cancellationToken))
            return;

        var template = new FinancialStatementNoteTemplate
        {
            Code = "B09-DN",
            Name = "Bản thuyết minh Báo cáo tài chính",
            AccountingRegime = "TT99/2025",
            Version = "2025.1",
            EffectiveFrom = new DateTime(2026, 1, 1),
            IsActive = true
        };

        var s1 = Section(template, "I", "Đặc điểm hoạt động của doanh nghiệp", 10);
        AddManual(s1, "I.01", "Hình thức sở hữu vốn", 10, true);
        AddManual(s1, "I.02", "Lĩnh vực kinh doanh", 20, true);
        AddManual(s1, "I.03", "Ngành nghề kinh doanh", 30, true);
        AddManual(s1, "I.04", "Chu kỳ sản xuất, kinh doanh thông thường", 40, false);
        AddManual(s1, "I.05", "Đặc điểm hoạt động trong năm tài chính có ảnh hưởng đến BCTC", 50, false);
        AddManual(s1, "I.06", "Cấu trúc doanh nghiệp", 60, false);
        AddManual(s1, "I.07", "Số lượng người lao động", 70, false);
        AddManual(s1, "I.08", "Khả năng so sánh thông tin trên BCTC", 80, false);
        AddManual(s1, "I.09", "Thông tin khác theo quy định pháp luật có liên quan", 90, false);

        var s2 = Section(template, "II", "Kỳ kế toán, đơn vị tiền tệ sử dụng trong kế toán", 20);
        AddManual(s2, "II.01", "Kỳ kế toán năm", 10, true);
        AddManual(s2, "II.02", "Đơn vị tiền tệ sử dụng trong kế toán", 20, true);

        var s3 = Section(template, "III", "Chuẩn mực và Chế độ kế toán áp dụng", 30);
        AddManual(s3, "III.01", "Chế độ kế toán áp dụng", 10, true);
        AddManual(s3, "III.02", "Tuyên bố tuân thủ Chuẩn mực kế toán Việt Nam và Chế độ kế toán", 20, true);

        var s4 = Section(template, "IV", "Các chính sách kế toán, ước tính kế toán và quy định pháp luật có liên quan", 40);
        AddManual(s4, "IV.01", "Nguyên tắc chuyển đổi BCTC lập bằng ngoại tệ sang VND", 10, false);
        AddManual(s4, "IV.02", "Các loại tỷ giá hối đoái áp dụng trong kế toán", 20, false);
        AddManual(s4, "IV.04", "Nguyên tắc ghi nhận tiền và các khoản tương đương tiền", 40, false);
        AddManual(s4, "IV.06", "Nguyên tắc kế toán nợ phải thu", 60, false);
        AddManual(s4, "IV.07", "Nguyên tắc kế toán hàng tồn kho", 70, false);
        AddManual(s4, "IV.08", "Nguyên tắc kế toán và khấu hao TSCĐ", 80, false);
        AddManual(s4, "IV.22", "Nguyên tắc và phương pháp ghi nhận doanh thu, thu nhập khác", 220, false);
        AddManual(s4, "IV.28", "Nguyên tắc và phương pháp ghi nhận chi phí thuế TNDN", 280, false);

        var s5 = Section(template, "V", "Thông tin bổ sung cho các khoản mục trình bày trong Báo cáo tình hình tài chính", 50);
        var cash = AddAuto(s5, "V.01", "Tiền và các khoản tương đương tiền", 10, B09SourceType.GeneralLedger);
        Map(cash, B09MappingMode.AccountBalance, "111,112,113", +1m);

        AddManual(s5, "V.02", "Các khoản đầu tư tài chính", 20, false);
        var ar = AddAuto(s5, "V.03", "Phải thu của khách hàng", 30, B09SourceType.Hybrid, true);
        Map(ar, B09MappingMode.AccountBalance, "131", +1m, 10m);
        AddManual(s5, "V.04", "Phải thu khác", 40, false);
        AddManual(s5, "V.05", "Tài sản thiếu chờ xử lý", 50, false);
        AddManual(s5, "V.06", "Nợ xấu", 60, true);
        AddManual(s5, "V.07", "Hàng tồn kho", 70, false);
        AddManual(s5, "V.08", "Tài sản dở dang dài hạn", 80, false);
        AddManual(s5, "V.09", "Tăng, giảm tài sản cố định hữu hình", 90, false);
        AddManual(s5, "V.10", "Tăng, giảm tài sản cố định vô hình", 100, false);
        AddManual(s5, "V.11", "Tăng, giảm tài sản cố định thuê tài chính", 110, false);
        AddManual(s5, "V.12", "Tài sản sinh học", 120, false);
        AddManual(s5, "V.13", "Tăng, giảm bất động sản đầu tư", 130, false);
        AddManual(s5, "V.14", "Chi phí chờ phân bổ", 140, false);
        AddManual(s5, "V.15", "Tài sản khác", 150, false);
        var loans = AddAuto(s5, "V.16", "Vay và nợ thuê tài chính", 160, B09SourceType.Hybrid, false);
        Map(loans, B09MappingMode.AccountBalance, "341", -1m);
        var ap = AddAuto(s5, "V.17", "Phải trả người bán", 170, B09SourceType.Hybrid, false);
        Map(ap, B09MappingMode.AccountBalance, "331", -1m, 10m);
        AddManual(s5, "V.18", "Phải trả về cổ tức, lợi nhuận", 180, false);
        var tax = AddAuto(s5, "V.19", "Thuế và các khoản phải nộp nhà nước", 190, B09SourceType.Hybrid, false);
        Map(tax, B09MappingMode.AccountBalance, "333", -1m);
        AddManual(s5, "V.20", "Chi phí phải trả", 200, false);
        AddManual(s5, "V.21", "Phải trả khác", 210, false);
        AddManual(s5, "V.22", "Doanh thu chờ phân bổ", 220, false);
        AddManual(s5, "V.23", "Trái phiếu phát hành", 230, false);
        AddManual(s5, "V.24", "Cổ phiếu ưu đãi phân loại là nợ phải trả", 240, false);
        AddManual(s5, "V.25", "Dự phòng phải trả", 250, false);
        AddManual(s5, "V.26", "Tài sản thuế thu nhập hoãn lại và thuế thu nhập hoãn lại phải trả", 260, false);
        var equity = AddAuto(s5, "V.27", "Vốn chủ sở hữu", 270, B09SourceType.Hybrid, false);
        Map(equity, B09MappingMode.AccountBalance, "411,412,413,414,415,418,419,421", -1m);
        AddManual(s5, "V.28", "Chênh lệch đánh giá lại tài sản", 280, false);
        AddManual(s5, "V.29", "Chênh lệch tỷ giá", 290, false);
        AddManual(s5, "V.30", "Các khoản mục ngoài Báo cáo tình hình tài chính", 300, true);
        AddManual(s5, "V.31", "Tài sản của bên khác doanh nghiệp đang nắm giữ nhưng bị giới hạn sử dụng / nghĩa vụ liên quan", 310, false);
        AddManual(s5, "V.32", "Các thông tin khác doanh nghiệp thấy cần thuyết minh, giải trình thêm", 320, false);

        // The source form jumps from V to VII; do not invent a VI section.
        var s7 = Section(template, "VII", "Thông tin bổ sung cho các khoản mục trình bày trong Báo cáo kết quả hoạt động kinh doanh", 70);
        var revenue = AddAuto(s7, "VII.01", "Tổng doanh thu bán hàng và cung cấp dịch vụ", 10, B09SourceType.Hybrid, false);
        Map(revenue, B09MappingMode.PeriodActivity, "511", -1m);
        var deductions = AddAuto(s7, "VII.02", "Các khoản giảm trừ doanh thu", 20, B09SourceType.GeneralLedger, false);
        Map(deductions, B09MappingMode.PeriodActivity, "521", +1m);
        var cogs = AddAuto(s7, "VII.03", "Giá vốn hàng bán", 30, B09SourceType.Hybrid, false);
        Map(cogs, B09MappingMode.PeriodActivity, "632", +1m);
        AddManual(s7, "VII.04", "Lãi/lỗ của hoạt động bán, thanh lý BĐSĐT", 40, false);
        var finRevenue = AddAuto(s7, "VII.05", "Doanh thu hoạt động tài chính", 50, B09SourceType.GeneralLedger, false);
        Map(finRevenue, B09MappingMode.PeriodActivity, "515", -1m);
        var finExpense = AddAuto(s7, "VII.06", "Chi phí tài chính", 60, B09SourceType.Hybrid, false);
        Map(finExpense, B09MappingMode.PeriodActivity, "635", +1m);
        var otherIncome = AddAuto(s7, "VII.07", "Thu nhập khác", 70, B09SourceType.GeneralLedger, false);
        Map(otherIncome, B09MappingMode.PeriodActivity, "711", -1m);
        var otherExpense = AddAuto(s7, "VII.08", "Chi phí khác", 80, B09SourceType.GeneralLedger, false);
        Map(otherExpense, B09MappingMode.PeriodActivity, "811", +1m);
        var salesAdmin = AddAuto(s7, "VII.09", "Chi phí bán hàng và chi phí quản lý doanh nghiệp", 90, B09SourceType.Hybrid, false);
        Map(salesAdmin, B09MappingMode.PeriodActivity, "641,642", +1m, 10m);
        AddManual(s7, "VII.10", "Chi phí sản xuất, kinh doanh theo yếu tố", 100, false);
        var incomeTax = AddAuto(s7, "VII.11", "Chi phí thuế thu nhập doanh nghiệp", 110, B09SourceType.Hybrid, false);
        Map(incomeTax, B09MappingMode.PeriodActivity, "821", +1m);

        var s8 = Section(template, "VIII", "Thông tin bổ sung cho các khoản mục trình bày trong Báo cáo lưu chuyển tiền tệ", 80);
        AddManual(s8, "VIII.01", "Các khoản tiền doanh nghiệp nắm giữ nhưng không được sử dụng", 10, true);
        AddManual(s8, "VIII.02", "Các giao dịch không bằng tiền ảnh hưởng đến BCLCTT trong tương lai", 20, false);
        AddManual(s8, "VIII.03", "Số tiền đi vay thực thu trong kỳ", 30, false);
        AddManual(s8, "VIII.04", "Số tiền đã thực trả gốc vay trong kỳ", 40, false);
        AddManual(s8, "VIII.05", "Mua và thanh lý công ty con trong kỳ báo cáo", 50, false);

        var s9 = Section(template, "IX", "Những thông tin khác", 90);
        AddManual(s9, "IX.01", "Nợ tiềm tàng, cam kết và thông tin tài chính khác", 10, false);
        AddManual(s9, "IX.02", "Sự kiện phát sinh sau ngày kết thúc kỳ kế toán năm", 20, false);
        AddManual(s9, "IX.03", "Thông tin về các bên liên quan", 30, false);
        AddManual(s9, "IX.04", "Thông tin theo bộ phận", 40, false);
        AddManual(s9, "IX.05", "Thông tin so sánh", 50, false);
        AddManual(s9, "IX.06", "Khả năng hoạt động liên tục", 60, false);
        AddManual(s9, "IX.07", "Các giả định và ước tính quan trọng", 70, false);
        AddManual(s9, "IX.08", "Các biện pháp/giải pháp khác", 80, false);

        var s10 = Section(template, "X", "Những nội dung sửa đổi, bổ sung biểu mẫu, tên và nội dung các chỉ tiêu của BCTC", 100);
        AddManual(s10, "X.01", "Tên các chỉ tiêu có sửa đổi, bổ sung", 10, false);
        AddManual(s10, "X.02", "Nội dung các chỉ tiêu có sửa đổi, bổ sung", 20, false);
        AddManual(s10, "X.03", "Lý do thay đổi", 30, false);

        _db.Add(template);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static FinancialStatementNoteSection Section(FinancialStatementNoteTemplate template, string code, string name, int order)
    {
        var section = new FinancialStatementNoteSection { Code = code, Name = name, DisplayOrder = order };
        template.Sections.Add(section);
        return section;
    }

    private static FinancialStatementNoteLine AddManual(FinancialStatementNoteSection section, string code, string name, int order, bool required)
    {
        var line = new FinancialStatementNoteLine
        {
            Code = code,
            Name = name,
            DisplayOrder = order,
            ValueType = B09ValueType.Text,
            SourceType = B09SourceType.Manual,
            IsRequiredNarrative = required
        };
        section.Lines.Add(line);
        return line;
    }

    private static FinancialStatementNoteLine AddAuto(FinancialStatementNoteSection section, string code, string name, int order, B09SourceType source, bool requiredNarrative = false)
    {
        var line = new FinancialStatementNoteLine
        {
            Code = code,
            Name = name,
            DisplayOrder = order,
            ValueType = B09ValueType.Amount,
            SourceType = source,
            IsRequiredNarrative = requiredNarrative
        };
        section.Lines.Add(line);
        return line;
    }

    private static void Map(FinancialStatementNoteLine line, B09MappingMode mode, string accountPrefixes, decimal sign, decimal? thresholdPercent = null)
    {
        line.Mappings.Add(new FinancialStatementNoteMapping
        {
            Mode = mode,
            AccountPrefixesCsv = accountPrefixes,
            SignMultiplier = sign,
            DetailThresholdPercent = thresholdPercent,
            IsEnabled = true
        });
    }
}
