# NVOAMASIS — Hệ thống quản lý Logistics / Freight Forwarding (NVOCC)

> Phần mềm web quản lý toàn bộ nghiệp vụ của một công ty giao nhận vận tải & NVOCC: từ báo giá, booking, chứng từ lô hàng (HBL/MBL), container, công nợ, hóa đơn điện tử cho đến kế toán, báo cáo tài chính và nhân sự — trên một nền tảng duy nhất.

`NVOAMASIS` là bản triển khai dành riêng cho một đơn vị khách hàng (database `nvoamasis`). Mã nguồn dùng chung một khung với các repo anh em **NVOAMASIS**, **NVOPASL**, **NVOVSSA**, **NVOVSSL**, **DNX** — tính năng giống nhau, chỉ khác tên thương hiệu, database và cấu hình triển khai.

---

## 1. Hệ thống dùng để làm gì?

Phục vụ các phòng ban của một công ty forwarding/NVOCC làm việc trên cùng một dữ liệu:

| Phòng ban | Công việc trên hệ thống |
|---|---|
| **Sales / Pricing** | Tra & import bảng giá cước (Sea FCL/LCL, Air, Trucking, thủ tục hải quan, nâng hạ, lưu kho…), nhận yêu cầu báo giá (RFQ), gửi báo giá cho khách |
| **Chứng từ / Operation** | Booking, Shipping Instruction (SI), xuất HBL, Arrival Notice, lệnh giao hàng (D/O, eDO), lệnh cấp container rỗng, lệnh điều xe, yêu cầu trucking / thủ tục hải quan / C/O |
| **Quản lý container (EQ)** | Theo dõi container, tồn bãi, import dữ liệu ra/vào bãi từ các cảng/depot (Cát Lái, VICT, Macstar Thủ Đức, SP-ITC, Phương Đông, Hải Phòng…), quản lý tàu |
| **Kế toán công nợ** | Debit/Credit note, đề nghị thanh toán / tạm ứng / hoàn ứng và luồng duyệt, công nợ khách hàng – nhà cung cấp, SOA, tuổi nợ, nhắc nợ đến hạn |
| **Hóa đơn** | Xuất hóa đơn điện tử (BKAV, Viettel), import hóa đơn đầu vào (XML/PDF), bảng kê VAT |
| **Kế toán tổng hợp** | Phiếu thu/chi, phiếu kế toán, sổ nhật ký chung, sổ cái, chữ T, kỳ kế toán, số dư, tài sản cố định & khấu hao, báo cáo tài chính theo **TT99** (B01–B09), báo cáo thuế |
| **Ban giám đốc** | Dashboard điều hành, lợi nhuận theo lô/khách hàng, báo cáo sản lượng, báo cáo quản trị |
| **Nhân sự (HR)** | Hồ sơ nhân viên, hợp đồng, sơ đồ tổ chức, nghỉ phép, chấm công, bảng lương |
| **Toàn công ty** | Chat nội bộ, thông báo realtime, trợ lý AI hỏi đáp dữ liệu, lịch sử thao tác, phân quyền theo menu |

## 2. Các phân hệ chính

Menu được đánh số và chia theo nhóm (mỗi màn hình mở thành **tab** trong ứng dụng):

1. **System** – Người dùng, phân quyền, danh mục (cảng, phí, terminal, trạng thái, số Ref), thông tin công ty & mẫu in Bill, danh mục tài khoản, nhật ký thao tác, đăng nhập bằng QR trên điện thoại.
2. **Business management** – Shipment, khách hàng / nhà cung cấp / đối tác, lợi nhuận.
3. **Report** – Báo cáo RFQ, báo giá, tạm ứng/hoàn ứng, shipment, khách hàng, pricing, phí.
4. **Pricing & Price schedule** – Lịch tàu, giá xuất/nhập, trucking, KTCL, thủ tục hải quan, Ocean/Air freight rate, LCC, biểu giá nâng hạ / rút ruột / lưu kho / KDTV.
5. **Payment & Payment request** – Đề nghị thanh toán (DNTT), tạm ứng (DNTU), duyệt DNTT, tổng hợp công nợ, xuất Debit/Credit note, phát hành hóa đơn, hóa đơn điện tử.
6. **Task** – Booking, HBL, A/N, eDO, lệnh điều xe, lệnh cấp rỗng, trucking, container tracking, quản lý cont/tàu, các báo cáo thống kê lô hàng.
7. **EQ / Yard** – Terminals, tồn kho container, import dữ liệu ra vào bãi theo từng cảng/depot, Tariff header/tier, Shipment charge context.
8. **Quỹ & Kế toán** – Phiếu thu/chi (kể cả giao dịch ngân hàng), báo cáo quỹ tiền mặt, sổ sách kế toán, TSCĐ, báo cáo tài chính TT99, báo cáo thuế VAT, báo cáo quản trị.
9. **Courier / Rate Hub** – Liên kết sang hệ thống chuyển phát nhanh (LMS Courier).
10. **HR** – Dashboard nhân sự, nhân viên, hợp đồng, tổ chức, nghỉ phép, chấm công, lương.

## 3. Công nghệ sử dụng

| Hạng mục | Công nghệ |
|---|---|
| Nền tảng | **.NET 8**, **Blazor Server** (Interactive Server, render mode global) |
| Giao diện | MudBlazor 6, CodeBeam.MudBlazor.Extensions, Blazor.Bootstrap, Chart.js |
| Dữ liệu | **SQL Server**, Entity Framework Core 8 (`AppDbContext`), SQL Server distributed cache |
| Xác thực | Cookie Authentication (web), JWT Bearer (API cho mobile), Google Sign-In, đăng nhập QR, BCrypt |
| Realtime | SignalR (`/notificationhub`) — thông báo, chat nội bộ |
| Báo cáo / in ấn | Stimulsoft Reports (Blazor), ClosedXML, EPPlus, OpenXML, ExcelDataReader, QRCoder, PdfPig, RazorLight |
| Tích hợp ngoài | Hóa đơn điện tử **BKAV**, **Viettel**; **SePay** (webhook giao dịch ngân hàng → phiếu thu); **Firebase Cloud Messaging** (push mobile); SMTP (MailKit); **OpenAI** (trợ lý AI); tỷ giá Vietcombank |
| Đa ngôn ngữ | Bản dịch lưu trong DB (`LocalizationResources`): `vi-VN`, `en-US`, `zh-CN` |
| Đa công ty | Tùy chọn **Multi-tenant**: mỗi tenant một database, quản lý qua database registry |
| Triển khai | IIS (hosting OutOfProcess, `web.config` tùy chỉnh) |

## 4. Kiến trúc tổng quan

```text
Trình duyệt ──(SignalR / Blazor Server)──► NVOAMASIS (ASP.NET Core 8)
App mobile ──(REST + JWT)──────────────►   ├─ Components/   Giao diện Blazor theo từng phân hệ
Ngân hàng (SePay) ──(webhook)──────────►   ├─ Controllers/  API: mobile, webhook, upload, QR, file đính kèm
                                           ├─ Services/     Nghiệp vụ (booking, kế toán, hóa đơn, HR, chat, AI…)
                                           ├─ Hubs/         SignalR NotificationHub
                                           └─ Data/         AppDbContext (nghiệp vụ) + RegistryDbContext (tenant)
                                                   │
                                                   ▼
                                       SQL Server: nvoamasis (+ Acc_Multi_DB khi bật multi-tenant)
                                                   │
                     BKAV / Viettel e-Invoice ◄────┼────► FCM, SMTP, OpenAI
```

## 5. Cấu trúc thư mục

```text
Components/      Trang Blazor, mỗi phân hệ một thư mục (Booking, SI, ExportHBL, PhieuThu, Accounting, Hr…)
  Layout/        MainLayout, NavMenu (menu + phân quyền), DynamicTabs
Controllers/     API controller (MobileAuth, PhieuApprove, BankWebhook, Upload, QR login, attachment…)
Services/        Service nghiệp vụ, đăng ký DI trong Program.cs
Accounting/B09/  Thuyết minh báo cáo tài chính B09
Data/            AppDbContext, RegistryDbContext, migrations
Models/          Entity & view model
Hubs/            SignalR hub
Resources/       Resource đa ngôn ngữ
Scripts/         Script SQL: tạo/sửa bảng, thêm quyền menu, import bản dịch
docs/            Tài liệu hướng dẫn (duyệt phiếu thu chi, SePay, FCM, HR…)
wwwroot/         Static file, mẫu Excel/báo cáo (wwwroot/Reports)
```

## 6. Chạy dự án

**Yêu cầu:** .NET 8 SDK, SQL Server, Visual Studio 2022 (khuyến nghị).

1. Cấu hình `appsettings.json` (hoặc `appsettings.Development.json`):

   | Section | Ý nghĩa |
   |---|---|
   | `ConnectionStrings:DefaultConnection` | Database nghiệp vụ |
   | `ConnectionStrings:RegistryConnection`, `MultiTenant` | Registry tenant (chỉ cần khi `MultiTenant:Enabled = true`) |
   | `AppBrand` | Tên phần mềm hiển thị trên tiêu đề / tab trình duyệt |
   | `JwtSettings` | Khóa ký token cho API mobile |
   | `Smtp` | Gửi email (quên mật khẩu, duyệt phiếu…) |
   | `BkavInvoice`, `SePay`, `Fcm`, `ChatGPT` | Thông tin tích hợp bên ngoài |

   > ⚠️ Không commit mật khẩu, API key thật lên git — dùng User Secrets hoặc biến môi trường.

2. Chuẩn bị database: chạy các script cần thiết trong `Scripts/` (tạo bảng, thêm quyền menu, bản dịch).

3. Chạy:

   ```bash
   dotnet run --project NVOAMASIS.csproj --launch-profile http
   ```

   Mặc định: <http://localhost:5070> (profile `https`: <https://localhost:7248>).

## 7. Publish & triển khai

```bash
dotnet publish NVOAMASIS.csproj -c Release -o ./publish
```

Copy thư mục `publish` lên IIS. Dự án giữ nguyên `web.config` tùy chỉnh (hosting OutOfProcess) và tạo sẵn thư mục `logs/` cho stdout log. Khi deploy bản mới có thay đổi DB, chạy script SQL tương ứng trong `Scripts/` **trước** khi chép code.

Sau khi cập nhật bản dịch trong DB: gọi `DbStringLocalizerFactory.ClearCache()` hoặc khởi động lại app.
