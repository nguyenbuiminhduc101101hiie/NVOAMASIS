# Module 12 — Quản lý nhân sự (giai đoạn 1)

## Chức năng

| Menu | Mã quyền | Nội dung |
|---|---|---|
| 12.1 Tổng quan nhân sự | `HR_Dashboard` | Số liệu theo trạng thái / phòng ban / chi nhánh; cảnh báo HĐ sắp hết hạn, sắp hết thử việc, sinh nhật trong tháng (15/30/60/90 ngày) |
| 12.2 Hồ sơ nhân viên | `HR_Employee` | Thêm/sửa/xóa hồ sơ (3 tab), xem hồ sơ chi tiết (tổng quan, hợp đồng, giấy tờ, lịch sử), tạo nhanh từ tài khoản đăng nhập, xuất Excel |
| 12.3 Hợp đồng lao động | `HR_Contract` | Danh sách + lọc (loại, trạng thái, sắp hết hạn), thêm/sửa/xóa |
| 12.4 Phòng ban & Chức vụ | `HR_Org` | Phòng ban (bảng `Department` có sẵn) và chức vụ (`HrPosition`) |
| (không có menu) | `HR_Salary` | See = xem lương trong hợp đồng; Edit = nhập/sửa lương |

Quy tắc nghiệp vụ chính:
- Mã NV tự sinh `NV0001`, số HĐ tự sinh `HD2026-0001` (sửa được).
- Mỗi tài khoản đăng nhập chỉ gắn được với 1 hồ sơ; không cho chọn quản lý tạo vòng lặp.
- Lưu hồ sơ tự ghi lịch sử khi đổi phòng ban, chức vụ, chi nhánh, quản lý, trạng thái, tài khoản.
- Không xóa được nhân viên đã có hợp đồng hoặc đang quản lý người khác → chuyển trạng thái "Đã nghỉ việc".
- Thêm HĐ mới (hiệu lực): tùy chọn chấm dứt HĐ cũ; HĐ thử việc cập nhật ngày hết thử việc; HĐ chính thức cho NV đang thử việc → tùy chọn chuyển "Chính thức".
- Không đổi mã / xóa phòng ban khi đang có tài khoản dùng mã đó (`UserList.Department`).
- Giấy tờ lưu ngoài wwwroot: `App_Data/hr/{tenant}/{employeeId}/` (đổi bằng `Hr:StorageRoot` trong appsettings), tải qua `/api/hr/documents/{id}` có kiểm tra quyền. Tối đa 20 MB; pdf, ảnh, Word, Excel, zip, rar.

## Triển khai

1. Chép các file trong gói đè vào project (3 file sửa: `Program.cs`, `Data/AppDbContext.cs`,
   `Components/Layout/NavMenu.razor` — nếu repo đã thay đổi, dùng `hr_modified_files.patch`).
2. Chạy SQL theo thứ tự trên **từng DB tenant** VÀ **DB template** (`MultiTenant:TemplateDatabase`, mặc định `nvoamasis`):
   1. `Scripts/CreateHrTables.sql`
   2. `Scripts/Add_HR_Menu_Permissions.sql` (tự cấp toàn quyền HR cho user phòng `ADMIN`; bỏ đoạn đó nếu không muốn)
   3. `Scripts/Localization_HR.sql`
3. Build, publish, recycle AppPool (bộ nhớ đệm chuỗi đa ngôn ngữ chỉ nạp lại khi restart).
4. Đảm bảo AppPool có quyền ghi `App_Data` (giống chat nội bộ).
5. Cấp quyền cho các user khác ở 1.6 Phân quyền; user nhấn F5 để thấy nhóm menu 12.
6. Vào 12.2 → "Tạo từ tài khoản" để tạo hồ sơ cho nhân viên hiện có, rồi bổ sung thông tin.

## Kiểm tra đã thực hiện
- Build toàn bộ project (C# + Razor) với .NET 8 SDK: 0 lỗi mới.
- Đối chiếu mọi thuộc tính MudBlazor dùng trong trang HR với MudBlazor 6.12.0.
- Chạy toàn bộ 38 câu truy vấn EF của các service qua EF Core SqlServer provider: đều dịch sang SQL được.
- Chưa chạy trên SQL Server thật / trình duyệt thật — cần test tay trước khi đưa lên production.

---

# Giai đoạn 2 — Nghỉ phép, quỹ phép, bảng công, cài đặt

## Chức năng mới

| Menu | Mã quyền | Nội dung |
|---|---|---|
| 12.5 Nghỉ phép | (mọi user) + `LeaveRequests` | Tab **Đơn của tôi** (xem quỹ phép, tạo/sửa/xóa đơn chờ duyệt), **Chờ tôi duyệt** (quản lý trực tiếp hoặc có quyền Approve), **Tất cả đơn** (quyền See; người có Approve hủy được đơn đã duyệt) |
| 12.6 Quỹ phép năm | `HR_LeaveBalance` | Xem phép tiêu chuẩn / thâm niên / tồn / điều chỉnh / đã dùng / chờ duyệt / còn lại; tạo, tính lại, sửa tay |
| 12.7 Bảng công tháng | `HR_Timesheet` | Tổng hợp chấm công + đơn nghỉ + ngày lễ theo buổi; xuất Excel (kèm sheet chú thích) |
| 12.8 Cài đặt nhân sự | `HR_Settings` | Ngày làm việc, T7 nửa ngày, giờ/ngày, phép tiêu chuẩn, thâm niên, phép tồn, cho phép âm phép, nguồn IP chấm công; danh sách ngày lễ |

Nút trên thanh tiêu đề (đăng ký / duyệt nghỉ phép) nay mở trang 12.5. Nút duyệt hiện cho cả **quản lý trực tiếp**
(không chỉ ADMIN). Trang cũ `Components/LeaveRequests/*` vẫn giữ nguyên nhưng không còn được mở.

## Quy tắc nghiệp vụ
- **Số ngày nghỉ** tính theo lịch làm việc: bỏ ngày nghỉ tuần và ngày lễ; T7 nửa ngày = 0,5. Nghỉ theo giờ trừ giờ trưa 12–13h,
  quy đổi ngày = giờ / số giờ mỗi ngày. (Module cũ đếm cả T7/CN và chia giờ cho 24.)
- **Nửa ngày** lưu buổi sáng/chiều vào StartTime/EndTime để bảng công biết buổi nào.
- **Người duyệt**: quản lý trực tiếp (HrEmployee.ManagerEmployeeId) còn làm việc và có tài khoản. Không có → gửi cho người có quyền
  Approve trên `LeaveRequests`, nếu không có ai thì phòng ADMIN (như cũ). Không tự duyệt đơn của mình.
- **Kiểm tra quỹ phép**: đơn phép năm bị chặn nếu vượt số còn có thể dùng (đã trừ đơn đang chờ), trừ khi bật "cho phép âm phép".
  Chặn đơn trùng thời gian với đơn khác (chờ duyệt / đã duyệt).
- **Quỹ phép mặc định**: 12 ngày (cấu hình); vào làm trong năm → theo tỷ lệ tháng (tháng vào làm được tính nếu vào trước ngày 16),
  làm tròn 0,5; +1 ngày mỗi 5 năm thâm niên tính đến 1/1; phép tồn năm trước tối đa N ngày (mặc định 0).
  Số đã dùng tính trực tiếp từ đơn phép năm đã duyệt nên luôn khớp.
- **Bảng công**: mỗi buổi 0,5 công. Có chấm công → X (tại VP) / TX (từ xa); không chấm nhưng có đơn đã duyệt → P/Ô/R/KL;
  ngày lễ → L; còn lại → V (vắng). Nghỉ theo giờ chỉ tính cho buổi khi nghỉ từ 2 giờ trở lên trong buổi đó.
  Công hưởng lương = làm việc + phép năm + việc riêng + lễ (nghỉ ốm do BHXH chi trả).
- **Nguồn IP chấm công** (12.8): mặc định "client" như cũ. "server" lấy IP từ request (header proxy), khó giả mạo hơn — cần thử
  chấm công tại văn phòng sau khi đổi.

## Triển khai giai đoạn 2
1. Chạy trên **từng DB tenant + DB template**, theo thứ tự:
   1. `Scripts/CreateHrLeaveTimesheetTables.sql` — **bắt buộc chạy TRƯỚC khi deploy code**: thêm cột
      `LeaveRequests.AssignedApproverId`; thiếu cột thì mọi màn hình nghỉ phép (cũ và mới) đều lỗi.
   2. `Scripts/Add_HR_Menu_Permissions.sql` (bản mới, thêm 3 mã quyền; chạy lại an toàn).
   3. `Scripts/Localization_HR.sql` (bản mới, gồm cả chuỗi giai đoạn 1).
2. Deploy code, recycle AppPool.
3. Vào 12.8: kiểm tra ngày làm việc (mặc định T2–T6), thêm ngày lễ của năm (Tết, Giỗ Tổ, nghỉ bù nhập tay).
4. Gán **Quản lý trực tiếp** cho nhân viên ở 12.2 (quyết định ai duyệt đơn).
5. Vào 12.6 → "Tạo quỹ phép" cho năm hiện tại; nhập phép tồn / điều chỉnh nếu cần.

## Kiểm tra đã thực hiện (giai đoạn 2)
- Build toàn bộ project: 0 lỗi; thuộc tính MudBlazor đối chiếu với 6.12.0.
- 45 câu truy vấn EF mới (cài đặt, nghỉ phép, quỹ phép, bảng công) đều dịch sang SQL được.
- Kiểm thử logic bảng công với dữ liệu mẫu (24 trường hợp: nửa ngày, T7 nửa ngày, lễ, nghỉ theo giờ, ngày tương lai,
  vào làm giữa tháng, chưa có tài khoản): đạt.
- Kiểm thử tính số ngày nghỉ: T2 28/9 → CN 4/10 = 5 ngày; 10:00–15:00 = 4 giờ = 0,5 ngày.
- Chưa chạy trên SQL Server thật / trình duyệt — cần test tay trước khi đưa lên production.

---

# Giai đoạn 3 — Bảng lương

## Chức năng: 12.9 Bảng lương (mã quyền `HR_Payroll`)
- **Kỳ lương**: tạo kỳ theo tháng → hệ thống tính ngay cho mọi nhân viên làm việc trong tháng từ
  hợp đồng hiệu lực (lương cơ bản, lương đóng BH, phụ cấp), bảng công 12.7 (công hưởng lương, ngày không lương)
  và tham số pháp lý. Sửa từng phiếu: công nhập tay, làm thêm, thưởng, thu nhập khác, thu nhập không chịu thuế,
  tạm ứng, khấu trừ khác, cách tính thuế, ghi chú. "Tính lại" giữ nguyên các khoản nhập tay.
  Quy trình: Nháp → **Chốt** (khóa sửa, nhân viên xem được phiếu) → **Hạch toán** (tạo chứng từ kế toán NHÁP).
  Mở chốt / hủy hạch toán được khi chứng từ chưa ghi sổ. Xuất Excel bảng lương (có dòng tổng).
- **Phiếu lương của tôi**: mọi nhân viên (có tài khoản gắn hồ sơ) xem phiếu của mình ở các kỳ đã chốt.
- **Tham số & tài khoản**: tham số pháp lý theo ngày hiệu lực; vùng lương tối thiểu, KPCĐ, quy tắc 14 ngày;
  mã tài khoản hạch toán và loại nghiệp vụ mặc định.

## Công thức
1. Lương theo công = Lương cơ bản × Công hưởng lương / Công chuẩn tháng (tối đa 1). Phụ cấp tương tự.
   Nhân viên chưa có tài khoản chấm công: tạm tính đủ công (có cảnh báo).
2. Tổng thu nhập = lương theo công + phụ cấp + làm thêm + thưởng + thu nhập khác + thu nhập không chịu thuế.
3. Bảo hiểm — chỉ với HĐ xác định/không xác định thời hạn, và số ngày không hưởng lương (không lương + vắng + ốm) < 14:
   lương đóng = "Lương đóng BHXH" trên hợp đồng (trống thì lấy lương cơ bản);
   BHXH/BHYT trên min(lương đóng, 20 × mức tham chiếu); BHTN trên min(lương đóng, 20 × lương tối thiểu vùng).
   NLĐ 8% / 1,5% / 1%; DN 17,5% / 3% / 1% + KPCĐ 2%.
4. Thuế TNCN: HĐ xác định/không xác định thời hạn → lũy tiến 5 bậc trên
   (thu nhập chịu thuế − BH NLĐ − 15,5tr − 6,2tr × số người phụ thuộc);
   thử việc / HĐ dịch vụ / không HĐ → khấu trừ 10% khi thu nhập chịu thuế từ 2tr. Đổi được trên từng phiếu.
5. Thực lĩnh = tổng thu nhập − BH NLĐ − thuế − tạm ứng − khấu trừ khác.

## Bút toán (chứng từ nháp, chi phí tách theo phòng ban)
| Nợ | Có | Số tiền |
|---|---|---|
| 642 (theo phòng ban) | 334 | Tổng thu nhập |
| 642 (theo phòng ban) | 3383 / 3384 / 3386 / 3382 | BH + KPCĐ phần DN |
| 334 | 3383 / 3384 / 3386 | BH phần NLĐ |
| 334 | 3335 | Thuế TNCN |
| 334 | 141 / 1388 | Tạm ứng / khấu trừ khác |

Số dư Có 334 còn lại = tổng thực lĩnh, được tất toán khi chi lương (phiếu chi / ủy nhiệm chi).
Mã tài khoản sửa được; tài khoản phải có trong 1.12 Danh mục tài khoản (hệ thống báo thiếu trước khi hạch toán).

## Tham số mặc định 2026 (kiểm tra lại trước khi dùng)
- Lương tối thiểu vùng từ 01/01/2026 (NĐ 293/2025/NĐ-CP): I 5.310.000; II 4.730.000; III 4.140.000; IV 3.700.000.
- Mức tham chiếu: 2.340.000 (01/01–30/6/2026), 2.530.000 (từ 01/7/2026).
- Thuế TNCN kỳ 2026: 5 bậc 5/10/20/30/35% (ngưỡng 10/30/60/100 triệu/tháng); giảm trừ 15,5tr / 6,2tr.
Khi Nhà nước thay đổi, thêm dòng tham số mới với ngày hiệu lực tương ứng.

## Triển khai giai đoạn 3
1. Chạy trên từng DB tenant + DB template: `Scripts/CreateHrPayrollTables.sql`, rồi `Add_HR_Menu_Permissions.sql`
   (bản mới, thêm `HR_Payroll`; ADMIN được cấp cả Approve), rồi `Localization_HR.sql` (bản mới).
2. Deploy, recycle AppPool.
3. 12.9 → Tham số & tài khoản: chọn vùng, loại nghiệp vụ, kiểm tra mã tài khoản.
4. Đảm bảo hợp đồng có lương cơ bản / lương đóng BH / phụ cấp và hồ sơ có số người phụ thuộc.
5. Tạo kỳ lương sau khi bảng công tháng đã đủ (nghỉ phép đã duyệt), kiểm tra cảnh báo, chốt, hạch toán.

## Kiểm tra đã thực hiện (giai đoạn 3)
- Build toàn bộ project: 0 lỗi; thuộc tính MudBlazor đối chiếu 6.12.0.
- 38 kiểm thử công thức lương, đạt: lương 30tr/1 NPT (thuế 257.500, thực lĩnh 26.592.500); 80tr chạm trần BHXH
  50,6tr (thuế 8.278.600); thử việc 20/22 công khấu trừ 10%; dưới ngưỡng 2tr; quy tắc 14 ngày; lương đóng BH riêng +
  thưởng + thu nhập không chịu thuế + tạm ứng; cảnh báo thiếu HĐ / dưới lương tối thiểu; thuế ở cả 5 bậc khớp công thức
  rút gọn; bút toán cân và số dư 334 = tổng thực lĩnh.
- 28 truy vấn EF mới của bảng lương dịch sang SQL được; kiểm thử hồi quy giai đoạn 1–2 vẫn đạt.
- Chưa chạy trên SQL Server thật / trình duyệt; nên đối chiếu 1 tháng lương với cách tính hiện tại (Excel) trước khi dùng thật.
