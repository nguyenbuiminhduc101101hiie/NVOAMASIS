# Hướng dẫn sử dụng — Duyệt phiếu thu / phiếu chi

## 1. Chuẩn bị (Admin / IT)

### 1.1. Database (mỗi DB tenant)
Chạy các script:

- `Scripts/Alter_PhieuThuChi_ApproveEmailFlow.sql`
- `Scripts/Create_DevicePushToken.sql` (nếu dùng app Flutter / FCM)

### 1.2. Cấu hình gửi email (Company Information)
Vào **Company Information**, điền:

- Email gửi thông báo  
- Mật khẩu email (Gmail nên dùng App Password)  
- SMTP Server (vd. `smtp.gmail.com`)  
- SMTP Port (vd. `587` hoặc `465`)  
- **Danh sách email nhận thông báo duyệt** (`ListEmail_nhanTB_Approve_Thu_Chi`): các email cách nhau dấu `,`

> Thiếu SMTP vẫn gửi được **Message trong web**. Email chỉ gửi khi SMTP đủ cấu hình.

### 1.3. Gán quyền duyệt cho user
Vào **User List** → sửa user → tick:

- **Duyệt phiếu thu** → nhận yêu cầu duyệt phiếu thu  
- **Duyệt phiếu chi** → nhận yêu cầu duyệt phiếu chi  

User nên có **Email** nếu muốn nhận mail duyệt.

---

## 2. Gửi yêu cầu duyệt (kế toán / user tạo phiếu)

1. Mở **Phiếu thu** hoặc **Phiếu chi**.
2. Click chọn 1 dòng phiếu (chưa Approve).
3. Bấm **Gửi Yêu cầu Duyệt**.

Hệ thống sẽ:

- Đặt phiếu sang trạng thái **Chờ duyệt**
- Gửi **Message** (chuông/email icon trên thanh menu) tới các user được gán duyệt
- Gửi **email** (nếu đã cấu hình SMTP) kèm link Approve / Deny

Cột Approve trên lưới:

| Hiển thị | Ý nghĩa |
|---|---|
| Chưa gửi | Chưa gửi yêu cầu |
| Chờ duyệt | Đã gửi, đang chờ |
| Approved | Đã duyệt (kèm tên người duyệt) |
| Denied | Đã từ chối (kèm tên người từ chối) |

### Lưu ý khi gửi lại
- Phiếu **đã Approve** → không gửi lại được; cần **Bỏ Approve** trước (chỉ người đã duyệt mới bỏ được).
- Phiếu **bị Deny** → được **sửa** và **Gửi Yêu cầu Duyệt** lại.

---

## 3. Duyệt trên Web (người được gán duyệt)

### Cách 1 — Message trong hệ thống (khuyến nghị)
1. Đăng nhập bằng user có quyền Duyệt phiếu thu/chi.
2. Click icon **Message** trên thanh trên (AppBar).
3. Click thông báo dạng:  
   `[PHIEU_APPROVE|Thu|...] Yêu cầu duyệt phiếu thu ...`
4. Dialog hiện thông tin phiếu:
   - **Approve** → duyệt  
   - **Deny** → bắt buộc nhập **lý do** rồi xác nhận  

Hoặc vào trang **Message** (View all messages) → click dòng nhận tương tự.

### Cách 2 — Email
1. Mở email yêu cầu duyệt.
2. Bấm **Mở trang Approve / Deny**.
3. Trên trang web:
   - **Approve**  
   - **Deny** (+ nhập lý do)

### Đồng bộ (rất quan trọng)
- Chỉ **1 người** được Approve hoặc Deny trước.
- Người còn lại:
  - Mở link/dialog sẽ thấy: *đã được xử lý bởi …*
  - Nhận thêm Message/email: không cần duyệt nữa.

---

## 4. Sau khi Approve / Deny

### Thông báo kết quả
Gửi tới:

1. Các email trong **Danh sách email nhận thông báo duyệt** (Company Information)  
2. Các user duyệt còn lại (đồng bộ)

Nội dung gồm: thông tin phiếu + trạng thái Approve/Deny + người xử lý (+ lý do nếu Deny).

### Tạo voucher
Chỉ phiếu **đã Approve** mới tạo được voucher kế toán (như rule cũ trên màn phiếu).

### Sửa / xóa phiếu
- Đã Approve → không sửa/xóa được; cần **Bỏ Approve** trước.
- Denied / Chờ duyệt / Chưa gửi → vẫn sửa được theo quyền Edit.

---

## 5. Bỏ Approve

1. Chọn phiếu đang **Approved**.
2. Bấm **Bỏ Approve**.
3. Chỉ **đúng user đã duyệt** (`ApproveByUserId`) mới bỏ được.

Sau khi bỏ:

- Phiếu về trạng thái chưa duyệt  
- Có thể sửa và gửi yêu cầu duyệt lại  

---

## 6. App Flutter (nếu đã deploy Mobile API)

1. Login API multi-DB (`/api/mobile/auth/login`).
2. Xem danh sách notifications.
3. Tap thông báo phiếu → màn Approve/Deny.
4. Đồng bộ với Web/Email (cùng token / cùng DB tenant).

> Chỉ dùng được khi server đã publish Mobile API (kiểm tra `GET /api/mobile/ping` trả JSON `mobile api ok`).

Chi tiết kỹ thuật cho team Flutter: `docs/Flutter_PhieuApprove_Mobile_Prompt.md`.

---

## 7. Checklist nhanh khi “không nhận được thông báo”

1. User đã tick **Duyệt phiếu thu/chi** chưa?  
2. User có **Email** chưa (nếu cần mail)?  
3. Company Info đã có SMTP chưa?  
4. Script DB đã chạy trên đúng tenant chưa?  
5. Đã bấm **Gửi Yêu cầu Duyệt** (không còn duyệt bằng checkbox trên lưới)?  
6. Người nhận đã login đúng DB/tenant chưa?

---

## 8. Tóm tắt luồng

```text
Tạo/sửa phiếu
    → Gửi Yêu cầu Duyệt
        → Message (web) + Email (nếu có SMTP)
            → 1 người Approve hoặc Deny
                → Thông báo kết quả cho list email + người duyệt còn lại
                → (Approve) được tạo voucher / (Deny) sửa rồi gửi lại
```
