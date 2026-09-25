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

## Hóa đơn (5.14 BKAV Statistics)

Webhook cũng tự nhận tiền cho hóa đơn. Áp dụng khi nội dung chuyển khoản **không** có số phiếu thu:

1. Mọi dãy số trong nội dung (bỏ số 0 ở đầu, ví dụ `HD0000123` là `123`) được so với **E-Invoice No**.
2. Số tiền chuyển phải đúng bằng **Amount (VAT)** của hóa đơn (hóa đơn VND).

Đủ cả hai và chỉ có đúng 1 hóa đơn khớp thì hóa đơn được tick **Đã thanh toán** như khi bấm tay (kể cả công nợ tự động). Vì xét mọi con số nên số tiền là điều kiện bắt buộc:

| Tình huống | Kết quả |
|---|---|
| Đúng số hóa đơn và đúng Amount (VAT) | Tự tick |
| Số tiền lệch, không có số hóa đơn, hoặc nhiều hóa đơn cùng khớp | Giao dịch ở 10.1.1 (Chưa khớp phiếu), kế toán gắn tay |
| Hóa đơn đã thanh toán mà tiền về thêm | "Phiếu đã thanh toán" |

Ở 10.1.1, bấm **Gắn phiếu thu** rồi chọn **Phiếu thu** hoặc **Hóa đơn** để gắn tay. Gắn đúng Amount (VAT) thì hóa đơn tự tick.

Trên 5.14, cột Paid là một chip bấm được để tick hoặc bỏ tick tay, kèm cảnh báo khi tiền nhận khác Amount (VAT).

## Phiếu chi (10.2) — công ty chuyển khoản đi

Chiều ngược lại: khi công ty chuyển khoản cho khách hàng/nhà cung cấp, SePay báo giao dịch `transferType = "out"` qua cùng một webhook. Áp dụng đúng quy tắc như Phiếu thu, chỉ đổi chiều:

1. **Nội dung lệnh chuyển khoản** có chứa số phiếu chi (ví dụ `HCMPC202690001`; bỏ dấu `_` vẫn nhận).
2. **Số tiền** chuyển đúng bằng số tiền của phiếu chi (phiếu VND).

Đủ cả hai thì phiếu chi tự tick **Đã thanh toán**. Không khớp hoặc sai số tiền thì giao dịch nằm ở 10.1.1 để kế toán gắn tay (chọn **Phiếu chi** trong hộp thoại gắn). Trên lưới 10.2, cột Paid là chip bấm được giống hệt 10.1.

Kế toán khi lập lệnh chuyển khoản trên app ngân hàng nên ghi đúng số phiếu chi vào nội dung, để hệ thống tự nhận biết đã chuyển thành công mà không cần vào lại app ngân hàng kiểm tra.

## Cài đặt

1. Đăng ký SePay (sepay.vn) và liên kết tài khoản ngân hàng công ty.
2. Chạy 2 script SQL: `Scripts/AlterPhieuThu_AddBankPayment.sql` (đã gồm cột gắn hóa đơn và phiếu chi) và `Scripts/Localization_BankPayment.sql`. Cả hai chạy lại nhiều lần an toàn.
3. Trên server đặt biến môi trường (không ghi key thật vào `appsettings.json`):
   - `SePay__WebhookApiKey` = chuỗi bí mật tự đặt
   - `SePay__AccountNumber` = số tài khoản nhận tiền (tùy chọn, để chỉ nhận giao dịch của tài khoản này)
4. Trên SePay, vào **Webhooks** và thêm webhook:
   - URL: `https://<tên-miền>/api/webhook/bank/sepay`
   - Kiểu xác thực: **API Key**, dán đúng chuỗi ở bước 3
   - Sự kiện: **cả tiền vào lẫn tiền ra** (SePay thường gọi chung 1 webhook cho cả hai chiều)
5. Dặn khách ghi **số phiếu thu**, và dặn kế toán ghi **số phiếu chi**, vào nội dung chuyển khoản.

Nếu `SePay__WebhookApiKey` để trống, webhook luôn trả 401 (tính năng tắt).

## Test

Chạy local, SePay không gọi được `localhost`. Dùng `ngrok http <port>` hoặc gửi thử bằng curl:

```
curl -X POST http://localhost:<port>/api/webhook/bank/sepay \
  -H "Authorization: Apikey <key>" -H "Content-Type: application/json" \
  -d '{"id":1001,"gateway":"MBBank","transactionDate":"2026-09-24 10:00:00","accountNumber":"xxx","content":"HCMPT202690012 thanh toan","transferType":"in","transferAmount":5000000,"referenceCode":"FT1"}'
```

Gửi lại cùng `id` thì bị bỏ qua (chống xử lý trùng). Để test phiếu chi, đổi `"transferType":"out"` và ghi số phiếu chi vào `content`, ví dụ:

```
curl -X POST http://localhost:<port>/api/webhook/bank/sepay \
  -H "Authorization: Apikey <key>" -H "Content-Type: application/json" \
  -d '{"id":3001,"gateway":"MBBank","transactionDate":"2026-09-26 10:00:00","accountNumber":"xxx","content":"HCMPC202690001 chuyen tien NCC","transferType":"out","transferAmount":2000000,"referenceCode":"FT3"}'
```

## Giới hạn

- Hệ thống multi-tenant hiện tắt. Nếu bật, webhook ẩn danh sẽ ghi vào DB mặc định, cần map tenant riêng.
- Chưa có mã QR VietQR; sẽ làm ở giai đoạn sau.
