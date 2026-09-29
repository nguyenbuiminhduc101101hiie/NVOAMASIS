namespace NVOAMASIS
{
    /// <summary>
    /// Tên phần mềm — hiện trên thanh tiêu đề (MainLayout) và đầu tên tab trình duyệt
    /// ("<tên> | <màn hình>", qua thẻ meta app-brand trong App.razor).
    /// Lấy từ appsettings.json → "AppBrand" khi khởi động (Program.cs); để trống / không có thì dùng giá trị mặc định dưới đây.
    /// Mỗi server / mỗi công ty chỉ cần sửa appsettings.json rồi khởi động lại app, không phải build lại.
    /// </summary>
    public static class AppInfo
    {
        public const string DefaultBrand = "AMASIS - NVOCC";

        public static string Brand { get; set; } = DefaultBrand;
    }
}
