using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using MudExtensions.Services;
using Stimulsoft.Base;
using System.Text;
using NVOAMASIS.Components;
using NVOAMASIS.Data;
using NVOAMASIS.Hubs;
using NVOAMASIS.Interface;
using NVOAMASIS.Models;
using NVOAMASIS.Services;
using NVOAMASIS.Services.Localization;
using NVOAMASIS.Services.MultiTenant;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();

// Register custom database-based string localizer factory
// This replaces the default file-based localization with database-driven localization
// Resources are loaded from LocalizationResources table and cached per culture
builder.Services.AddSingleton<IStringLocalizerFactory, DbStringLocalizerFactory>();

// Set Stimulsoft license key
try
{
    StiLicense.Key = "6vJhGtLLLz2GNviWmUTrhSqnOItdDwjBylQzQcAOiHkgpgFGkUl79uxVs8X+uspx6K+tqdtOB5G1S6PFPRrlVNvMUiSiNYl724EZbrUAWwAYHlGLRbvxMviMExTh2l9xZJ2xc4K1z3ZVudRpQpuDdFq+fe0wKXSKlB6okl0hUd2ikQHfyzsAN8fJltqvGRa5LI8BFkA/f7tffwK6jzW5xYYhHxQpU3hy4fmKo/BSg6yKAoUq3yMZTG6tWeKnWcI6ftCDxEHd30EjMISNn1LCdLN0/4YmedTjM7x+0dMiI2Qif/yI+y8gmdbostOE8S2ZjrpKsgxVv2AAZPdzHEkzYSzx81RHDzZBhKRZc5mwWAmXsWBFRQol9PdSQ8BZYLqvJ4Jzrcrext+t1ZD7HE1RZPLPAqErO9eo+7Zn9Cvu5O73+b9dxhE2sRyAv9Tl1lV2WqMezWRsO55Q3LntawkPq0HvBkd9f8uVuq9zk7VKegetCDLb0wszBAs1mjWzN+ACVHiPVKIk94/QlCkj31dWCg8YTrT5btsKcLibxog7pv1+2e4yocZKWsposmcJbgG0";
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Stimulsoft license key is invalid or expired. {ex.Message}");
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        // Enable to see Blazor circuit exception details when debugging (disable in production for security)
        options.DetailedErrors = builder.Configuration.GetValue<bool>("DetailedErrors");
    });

builder.Services.AddMudServices();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Distributed cache (SQL Server) - token D/O QR và dữ liệu cache khác sống qua restart
builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    options.TableName = "DistributedCache";
});

//# for BLAZOR COOKIE Auth
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMudExtensions();
builder.Services.AddBlazorBootstrap();

//## for BLAZOR COOKIE Auth
/// ref �� https://blazorhelpwebsite.com/ViewBlogPost/36
//builder.Services.Configure<CookiePolicyOptions>(options =>
//{
//  options.CheckConsentNeeded = context => true;
//  options.MinimumSameSitePolicy = SameSiteMode.Strict;
//});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(cfg =>
    {
        cfg.LoginPath = "/Account/Login"; // default: /Accout/Login
        cfg.Cookie.Name = ".NVOAMASIS.Cookies"; //default:.AspNetCore.Cookies
        cfg.Cookie.SameSite = SameSiteMode.Lax;
        cfg.Cookie.HttpOnly = true;
        cfg.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

//Setting
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddDbContext<RegistryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RegistryConnection") ??
        throw new InvalidOperationException("RegistryConnection is not configured")));
builder.Services.AddScoped<TenantDatabaseProvisioningService>();
builder.Services.AddScoped<TenantAuthService>();

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();
    tenantContext.EnsureInitializedFromHttpContext();
    var connectionString = tenantContext.ConnectionString
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Sorry, your connection is not found");
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<IDbContextFactory<AppDbContext>, TenantAwareDbContextFactory>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ReportServices>();
builder.Services.AddScoped<TrainScheduleServices>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IUserService, UserServicecs>();
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.BufferBodyLengthLimit = int.MaxValue;
});
builder.Services.AddScoped<SharedServices>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<SupportServices>();
builder.Services.AddScoped<ShipmentService>();
builder.Services.AddScoped<InvoicePdfImportService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<ExcelImportCrudService>();
builder.Services.AddScoped<DeliveryOrderQrService>();
builder.Services.AddScoped<ArrivalNoticeQrService>();
builder.Services.AddScoped<QuotationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<FontService>();
builder.Services.AddScoped<GlobalServices>();
builder.Services.AddScoped<BillSeaReportTemplateService>();
builder.Services.AddScoped<ProductPriceServices>();
builder.Services.AddScoped<LocalChargesServices>();
builder.Services.AddScoped<LenhDieuXeServices>();
builder.Services.AddScoped<YeuCauTruckingServices>();
builder.Services.AddScoped<TKHQ_Services>();
builder.Services.AddScoped<ThuTucTuVanHQ_Services>();
builder.Services.AddScoped<Bieugianangha_Services>();
builder.Services.AddScoped<DNTUServices>();
builder.Services.AddScoped<PhieuThu_Chi_Services>();
builder.Services.AddScoped<Danhmuctaikhoan_services>();
builder.Services.AddScoped<TaxServices>();
builder.Services.Configure<BkavInvoiceSettings>(builder.Configuration.GetSection("BkavInvoice"));
builder.Services.AddHttpClient<BkavInvoiceService>();
builder.Services.AddScoped<ExportCostPriceServices>();
builder.Services.AddScoped<IssueReportServices>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<tigiaVCBservices>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<InventoryExportService>();
builder.Services.AddScoped<IClientIpService, ClientIpService>();
builder.Services.AddScoped<IWordDocumentService, WordDocumentService>();
builder.Services.AddScoped<LeaveRequestService>();
builder.Services.AddScoped<ResxImportService>();
builder.Services.AddScoped<ThemeService>();

// Forwarded headers (X-Forwarded-For / X-Forwarded-Proto) support
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Optionally add known networks/proxies if you want to restrict trust:
    // options.KnownProxies.Add(System.Net.IPAddress.Parse("127.0.0.1"));
});

builder.Services.AddScoped<IForgotPasswordEmailService, ForgotPasswordEmailService>();

// AI Assistant - ChatGPT integration
builder.Services.Configure<NVOAMASIS.Models.ChatGPTSettings>(builder.Configuration.GetSection("ChatGPT"));
builder.Services.AddHttpClient<NVOAMASIS.Services.ChatGPTService>();
builder.Services.AddScoped<NVOAMASIS.Services.ChatGPTService>();
builder.Services.AddHttpClient<NVOAMASIS.Services.WebSearchService>();
builder.Services.AddScoped<NVOAMASIS.Services.WebSearchService>();



builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<HistoryLogService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();

    string baseUrl;

    if (env.IsDevelopment())
    {
        // D�ng URL local khi ?ang ch?y local
        baseUrl = "http://localhost:5070/";
    }
    else
    {
        // D�ng URL th?t khi publish l�n server
        baseUrl = config["ApiSettings:HistoryLogBaseUrl"]; // may be null
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            // Fallback: keep existing or throw meaningful error
            baseUrl = "https://localhost/"; // safe fallback placeholder
        }
        //baseUrl = builder.Configuration["ApiSettings:HistoryLogBaseUrl"];
    }

    client.BaseAddress = new Uri(baseUrl!);
});
// Change SingletonSerivce to scoped to align with per-request auth/user context and avoid holding onto stale user/context state.
builder.Services.AddScoped<SingletonSerivce>();
// Remove duplicate singleton registration of CustomAuthenticationStateProvider (already added as scoped above)

var app = builder.Build(); //--------------------------------------------------

var supportedCultures = new[] { "en-US", "vi-VN", "zh-CN" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// MUST appear early, before auth and other middleware that reads scheme or RemoteIpAddress
//app.UseForwardedHeaders();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseAntiforgery();
app.UseCors();
//# for BLAZOR COOKIE Auth
app.UseCookiePolicy();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationhub");

/// ref��https://learn.microsoft.com/zh-tw/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#usestatuscodepages
// Map API controllers BEFORE status code pages to avoid redirects
app.MapControllers();
app.UseStatusCodePagesWithRedirects("/ErrorStatus/{0}");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
