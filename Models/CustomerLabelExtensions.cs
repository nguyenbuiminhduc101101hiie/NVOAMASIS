namespace NVOAMASIS.Models
{
    /// <summary>Hiển thị khách hàng thống nhất dạng "Code - Tên" và chuỗi tìm kiếm gồm code, tên, tên tắt, MST.</summary>
    public static class CustomerLabelExtensions
    {
        public static string CodeName(this M_Customer? c)
        {
            if (c == null) return string.Empty;
            var name = !string.IsNullOrWhiteSpace(c.COMPANY) ? c.COMPANY!.Trim()
                     : !string.IsNullOrWhiteSpace(c.shortname) ? c.shortname!.Trim()
                     : (c.EnglishName ?? string.Empty).Trim();
            var code = (c.Customer_Code ?? string.Empty).Trim();
            if (code.Length == 0) return name;
            if (name.Length == 0) return code;
            return $"{code} - {name}";
        }

        public static string SearchKey(this M_Customer? c)
        {
            if (c == null) return string.Empty;
            return string.Join(" | ", new[] { c.Customer_Code, c.COMPANY, c.shortname, c.EnglishName, c.TaxCode }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
