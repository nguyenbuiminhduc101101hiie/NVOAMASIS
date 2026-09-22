using Stimulsoft.Base;
using Stimulsoft.Report;
using Stimulsoft.Report.Components;

namespace NVOAMASIS.Services
{
    /// <summary>
    /// Gán license Stimulsoft trước mọi thao tác Load/Save/Render.
    /// IIS/publish đôi khi mất trạng thái static nếu chỉ set 1 lần ở Program.cs.
    /// </summary>
    public static class StimulsoftLicenseHelper
    {
        // Đổi key ở đây (Program.cs gọi EnsureApplied từ helper này).
        public const string LicenseKey =
            "6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkgpgFGkUl79uxVs8X+uspx6K+tqdtOB5G1S6PFPRrlVNvMUiSiNYl724EZbrUAWwAYHlGLRbvxMviMExTh2l9xZJ2xc4K1z3ZVudRpQpuDdFq+fe0wKXSKlB6okl0hUd2ikQHfyzsAN8fJltqvGRa5LI8BFkA/f7tffwK6jzW5xYYhHxQpU3hy4fmKo/BSg6yKAoUq3yMZTG6tWeKnWcI6ftCDxEHd30EjMISNn1LCdLN0/4YmedTjM7x+0dMiI2Qif/yI+y8gmdbostOE8S2ZjrpKsgxVv2AAZPdzHEkzYSzx81RHDzZBhKRZc5mwWAmXsWBFRQol9PdSQ8BZYLqvJ4Jzrcrext+t1ZD7HE1RZPLPAqErO9eo+7Zn9Cvu5O73+b9dxhE2sRyAv9Tl1lV2WqMezWRsO55Q3LntawkPq0HvBkd9f8uVuq9zk7VKegetCDLb0wszBAs1mjWzN+ACVHiPVKIk94/QlCkj31dWCg8YTrT5btsKcLibxog7pv1+2e4yocZKWsposmcJbgG0";

        private static readonly object Sync = new();
        private static bool _licenseApplied;

        // Stimulsoft dùng các dictionary tĩnh nội bộ (sinh key duy nhất cho style/resource...)
        // không thread-safe. Khi nhiều request export report đồng thời, có thể trùng key
        // và ném ArgumentException "Item has already been added". Dùng lock này để serialize
        // toàn bộ vòng đời report (CreateReport -> Load -> Render -> ExportDocument).
        private static readonly SemaphoreSlim ReportLock = new(1, 1);

        public static IDisposable AcquireReportLock()
        {
            ReportLock.Wait();
            return new ReportLockRelease();
        }

        private sealed class ReportLockRelease : IDisposable
        {
            private bool _released;

            public void Dispose()
            {
                if (_released)
                    return;
                _released = true;
                ReportLock.Release();
            }
        }

        public static void EnsureApplied()
        {
            // Gán StiLicense.Key nhiều lần trong cùng vòng đời process không idempotent —
            // Stimulsoft đăng ký lại các module nội bộ (export, chart...) vào dictionary tĩnh
            // mỗi lần set key, gây "Item has already been added" nếu set lần 2 trở đi.
            // Chỉ set 1 lần; nếu static state thực sự bị mất (VD IIS recycle AppDomain),
            // field _licenseApplied cũng bị reset theo nên sẽ tự set lại đúng lúc cần.
            if (_licenseApplied)
                return;

            lock (Sync)
            {
                if (_licenseApplied)
                    return;

                try
                {
                    StiLicense.Key = LicenseKey;
                    _licenseApplied = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Stimulsoft] License apply failed: {ex.Message}");
                }
            }
        }

        public static StiReport CreateReport()
        {
            EnsureApplied();
            return StiReport.CreateNewReport();
        }

        public static void PrepareAndRender(StiReport report)
        {
            EnsureApplied();
            ClearTrialWatermark(report);
            report.Render();
        }

        /// <summary>
        /// Form từng Save lúc chạy trial có thể giữ watermark "Trial" trong .mrt — xóa trước Render/Save.
        /// </summary>
        public static void ClearTrialWatermark(StiReport report)
        {
            if (report?.Pages == null)
                return;

            foreach (StiPage page in report.Pages)
            {
                if (page.Watermark == null)
                    continue;

                var text = page.Watermark.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                if (text.Contains("Trial", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("Demo Version", StringComparison.OrdinalIgnoreCase)
                    || text.Contains("CREATED WITH", StringComparison.OrdinalIgnoreCase))
                {
                    page.Watermark.Text = string.Empty;
                    page.Watermark.Enabled = false;
                }
            }
        }
    }
}
