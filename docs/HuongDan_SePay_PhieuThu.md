# Tự nhận tiền chuyển khoản cho Phiếu thu (SePay)

Khi khách chuyển khoản vào tài khoản công ty, SePay gửi webhook về phần mềm. Phần mềm kiểm tra:

1. **Nội dung chuyển khoản** có chứa số phiếu thu (ví dụ `HCMPT202690012`; ngân hàng bỏ dấu `_` cũng vẫn nhận).
2. **Số tiền** chuyển đúng bằng số tiền của phiếu thu (phiếu VND).

Đủ cả hai thì phiếu thu được tự tick **Đã thanh toán**. Trường hợp còn lại:

| Tình huống | Kết quả |
|---|---|
| Không có số phiếu trong nội dung | Giao dịch lưu ở màn hình **10.1.1 Giao dịch ngân hàng** (Chưa khớp phiếu), kế toán gắn tay |
| Có số phiếu nhưng thiếu hoặc thừa tiền | Phiếu **không** tick, lưới phiếu thu hiện chip cam "Sai số tiền: nhận X / cần Y" |
| Phiếu đã thanh toán mà tiền về thêm | Giao dịch đánh dấu "Phiếu đã thanh toán" để kế toán xem (khách chuyển trùng) |
| Phiếu USD | Không tự tick, kế toán kiểm tra tay |

Kế toán vẫn tick / bỏ tick tay được trên lưới phiếu thu (cần quyền Edit).

## Cài đặt

1. Đăng ký SePay (sepay.vn) và liên kết tài khoản ngân hàng công ty.
2. Chạy 2 script SQL: `Scripts/AlterPhieuThu_AddBankPayment.sql` và `Scripts/Localization_BankPayment.sql`.
3. Trên server đặt biến môi trường (không ghi key thật vào `appsettings.json`):
   - `SePay__WebhookApiKey` = chuỗi bí mật tự đặt
   - `SePay__AccountNumber` = số tài khoản nhận tiền (tùy chọn, để chỉ nhận giao dịch của tài khoản này)
4. Trên SePay, vào **Webhooks** và thêm webhook:
   - URL: `https://<tên-miền>/api/webhook/bank/sepay`
   - Kiểu xác thực: **API Key**, dán đúng chuỗi ở bước 3
   - Sự kiện: tiền vào
5. Dặn khách ghi **số phiếu thu** vào nội dung chuyển khoản.

Nếu `SePay__WebhookApiKey` để trống, webhook luôn trả 401 (tính năng tắt).

## Test

Chạy local, SePay không gọi được `localhost`. Dùng `ngrok http <port>` hoặc gửi thử bằng curl:

```
curl -X POST http://localhost:<port>/api/webhook/bank/sepay \
  -H "Authorization: Apikey <key>" -H "Content-Type: application/json" \
  -d '{"id":1001,"gateway":"MBBank","transactionDate":"2026-09-24 10:00:00","accountNumber":"xxx","content":"HCMPT202690012 thanh toan","transferType":"in","transferAmount":5000000,"referenceCode":"FT1"}'
```

Gửi lại cùng `id` thì bị bỏ qua (chống xử lý trùng).

## Giới hạn

- Hệ thống multi-tenant hiện tắt. Nếu bật, webhook ẩn danh sẽ ghi vào DB mặc định, cần map tenant riêng.
- Chưa có mã QR VietQR; sẽ làm ở giai đoạn sau.
