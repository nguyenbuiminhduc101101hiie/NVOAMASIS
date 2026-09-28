/*
  Module 12 - Quản lý nhân sự: mã quyền (MenuNames) + cấp quyền mặc định.
  Chạy trên từng tenant DB + DB template. Safe to re-run.

  Mã quyền (Permission code == MenuName):
    HR_Dashboard  12.1 Tổng quan nhân sự           (See)
    HR_Employee   12.2 Hồ sơ nhân viên + giấy tờ    (See / Add / Edit / Del)
    HR_Contract   12.3 Hợp đồng lao động            (See / Add / Edit / Del)
    HR_Org        12.4 Phòng ban & Chức vụ          (See / Add / Edit / Del)
    HR_Salary     Xem lương trong hợp đồng (See) - Nhập/sửa lương (Edit)
    -- Giai đoạn 2 --
    HR_LeaveBalance 12.6 Quỹ phép năm                (See / Edit: tạo, tính lại, sửa)
    HR_Timesheet    12.7 Bảng công tháng             (See)
    HR_Settings     12.8 Cài đặt nhân sự             (See / Edit)
    -- Giai đoạn 3 --
    HR_Payroll      12.9 Bảng lương: See = xem bảng lương; Add = tạo kỳ; Edit = tính lại, sửa phiếu;
                    Delete = xóa kỳ nháp; Approve = chốt/mở chốt, hạch toán/hủy, sửa tham số & tài khoản.
                    Mọi user thấy tab "Phiếu lương của tôi" (kỳ đã chốt).
    12.5 Nghỉ phép: mọi user dùng được (đơn của mình). Quyền cũ "LeaveRequests":
      See = xem tab "Tất cả đơn", Approve = duyệt/hủy mọi đơn (kể cả không phải cấp dưới).
      Quản lý trực tiếp (HrEmployee.ManagerEmployeeId) tự động duyệt được đơn của cấp dưới.

  Sau khi chạy: cấp quyền cho user khác tại màn hình 1.6 Phân quyền.
  Menu trái đọc quyền khi tải trang → user nhấn F5 (hoặc đăng nhập lại) để thấy nhóm menu 12.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'HR_Dashboard' AS MenuName, N'12.1 Tong quan nhan su' AS Title UNION ALL
    SELECT N'HR_Employee',  N'12.2 Ho so nhan vien' UNION ALL
    SELECT N'HR_Contract',  N'12.3 Hop dong lao dong' UNION ALL
    SELECT N'HR_Org',       N'12.4 Phong ban & Chuc vu' UNION ALL
    SELECT N'HR_Salary',    N'12.x Xem/sua luong trong hop dong' UNION ALL
    SELECT N'HR_LeaveBalance', N'12.6 Quy phep nam' UNION ALL
    SELECT N'HR_Timesheet', N'12.7 Bang cong thang' UNION ALL
    SELECT N'HR_Settings',  N'12.8 Cai dat nhan su' UNION ALL
    SELECT N'HR_Payroll',   N'12.9 Bang luong'
)
INSERT INTO [MenuNames] (MenuID, MenuName, Title)
SELECT NEWID(), src.MenuName, src.Title
FROM src
WHERE NOT EXISTS (SELECT 1 FROM [MenuNames] m WHERE m.MenuName = src.MenuName);

/*
  Cấp toàn quyền HR cho user thuộc phòng ban ADMIN (UserList.Department = 'ADMIN')
  nếu user đó chưa có dòng quyền cho menu tương ứng.
  Bỏ đoạn này nếu công ty không muốn ADMIN mặc định thấy dữ liệu nhân sự / lương.
*/
INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, [Add])
SELECT NEWID(), m.MenuID, m.MenuName, u.Usr, 1, 1, 1,
       CASE WHEN m.MenuName = N'HR_Payroll' THEN 1 ELSE 0 END, 1
FROM [UserList] u
CROSS JOIN [MenuNames] m
WHERE u.Department = N'ADMIN'
  AND u.Usr IS NOT NULL
  AND m.MenuName IN (N'HR_Dashboard', N'HR_Employee', N'HR_Contract', N'HR_Org', N'HR_Salary',
                     N'HR_LeaveBalance', N'HR_Timesheet', N'HR_Settings', N'HR_Payroll')
  AND NOT EXISTS (
      SELECT 1 FROM [Permissions] p
      WHERE p.UserName = u.Usr AND p.MenuName = m.MenuName
  );

COMMIT TRANSACTION;

PRINT N'HR menu permissions ready.';

/*
  Ví dụ cấp quyền cho 1 user cụ thể (chỉ xem hồ sơ + hợp đồng, không xem lương):

DECLARE @UserName NVARCHAR(256) = N'YOUR_USER_NAME';
INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, [Add])
SELECT NEWID(), m.MenuID, m.MenuName, @UserName, 1, 0, 0, 0, 0
FROM [MenuNames] m
WHERE m.MenuName IN (N'HR_Dashboard', N'HR_Employee', N'HR_Contract')
  AND NOT EXISTS (SELECT 1 FROM [Permissions] p WHERE p.UserName = @UserName AND p.MenuName = m.MenuName);
*/
