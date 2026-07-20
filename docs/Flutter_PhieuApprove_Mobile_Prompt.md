# Flutter Prompt — Đồng bộ duyệt Phiếu thu/chi với NVOAMASIS Web

Copy toàn bộ nội dung bên dưới gửi cho team Flutter / AI coding agent.

---

## Trạng thái backend (cập nhật cho team Web/Flutter)

| Mục | Trạng thái |
|---|---|
| Code trong solution NVOAMASIS | **ĐÃ CÓ** |
| Controllers | `Controllers/MobileAuthController.cs`, `MobileNotificationController.cs`, `MobilePhieuApproveController.cs`, `MobileDeviceController.cs` |
| Bug ISnackbar / 500 HTML | **ĐÃ FIX** — `NotificationService` không còn inject MudBlazor `ISnackbar` |
| FCM push khi gửi duyệt | **ĐÃ CÓ** FCM HTTP v1 (`FcmPushService` + `Google.Apis.Auth`). Legacy Server Key **không dùng**. |
| Prod `https://amasis.nvocc.vn` | **ĐÃ CÓ** Mobile API. Push prod cần đặt service account trên server + `Fcm.Enabled=true`. |
| Local test Flutter | `https://localhost:7248` |

Health check:

```bash
curl.exe -i "https://localhost:7248/api/mobile/ping"
```

### Bật FCM HTTP v1 (push điện thoại)

1. Firebase Console → project **lms-qrscan** → Service accounts → Generate new private key  
2. Đặt file: `Secrets/firebase-service-account.json` (đã trong `.gitignore`)  
3. Config:

```json
"Fcm": {
  "Enabled": true,
  "ProjectId": "lms-qrscan",
  "ServiceAccountPath": "Secrets/firebase-service-account.json"
}
```

- Local: `appsettings.Development.json` đã `Enabled: true` — **chỉ cần thả file service account rồi restart**.  
- Prod (`appsettings.json`): mặc định `Enabled: false` đến khi ops đặt SA trên server rồi bật.  
- Thiếu file / sai ProjectId → log warning/error, **không** chặn email/in-app.

---

## Mục tiêu

Xây dựng trên Flutter (hoặc mở rộng app Flutter hiện có) tính năng:

1. Đăng nhập multi-tenant (cùng kiểu login web).
2. Nhận danh sách **Message/Notification** từ backend.
3. Khi notification là yêu cầu duyệt phiếu thu/chi → mở màn **Approve / Deny**.
4. Đồng bộ 100% với Web + Email: ai duyệt trước thì người còn lại không duyệt được.

Backend NVOAMASIS đã có sẵn REST API JSON trong source. **Không tự tạo logic duyệt riêng trên Flutter** — chỉ gọi API.

---

## Base URL

Dùng base URL môi trường thực tế của server NVOAMASIS (nơi host API `/api/mobile/...`), ví dụ:

- Dev: `https://<host-dev>` hoặc `http://<ip>:<port>`
- Prod: `https://<host-prod>`

Không hardcode domain cụ thể trong app — cấu hình theo môi trường (flavor / `.env` / `--dart-define`).

Mọi endpoint mobile prefix: `/api/mobile/...`

Header bắt buộc sau login:

```http
Authorization: Bearer <accessToken>
Content-Type: application/json
```

---

## 1) Login

`POST /api/mobile/auth/login`

Body:

```json
{
  "databaseName": "nvoamasis",
  "sqlUserId": "sgn-fc-admin",
  "sqlPassword": "***",
  "appUserName": "username",
  "appPassword": "***"
}
```

Response success:

```json
{
  "flag": true,
  "message": "Đăng nhập thành công.",
  "accessToken": "eyJhbGciOi...",
  "expiresAtUtc": "2026-07-27T00:00:00Z",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "databaseName": "nvoamasis",
  "usrId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "usr": "username",
  "name": "Display Name",
  "email": "a@b.com",
  "department": "ADMIN"
}
```

Lưu `accessToken`, `usrId`, `tenantId` (secure storage).

Check session: `GET /api/mobile/auth/me` (Bearer).

---

## 2) Notifications (Message)

### List

`GET /api/mobile/notifications?unreadOnly=false&take=50`

Response:

```json
{
  "unreadCount": 2,
  "items": [
    {
      "id": "...",
      "senderUserId": "...",
      "receiverUserId": "...",
      "message": "[PHIEU_APPROVE|Thu|abc123...] Yêu cầu duyệt phiếu thu PT001 từ UserA. Click để Approve/Deny.",
      "createdAt": "2026-07-20T02:00:00Z",
      "isRead": false,
      "isPhieuApprove": true,
      "phieuLoai": "Thu",
      "phieuToken": "abc123..."
    }
  ]
}
```

### Mark read

`POST /api/mobile/notifications/{id}/read`

### Mark all read

`POST /api/mobile/notifications/read-all`

---

## 3) Duyệt phiếu thu/chi (in-app)

### Lấy chi tiết

`GET /api/mobile/phieu-approve/{phieuToken}`

Response:

```json
{
  "loai": "Thu",
  "phieuId": "...",
  "token": "...",
  "soPhieu": "PT001",
  "detailHtml": "<table>...</table>",
  "approve": null,
  "approveBy": null,
  "approveDate": null,
  "remarks": null,
  "alreadyDecided": false,
  "canDecide": true
}
```

UI:

- Parse `detailHtml` bằng `flutter_html` **hoặc** tự render các field text (khuyến nghị parse HTML đơn giản / hiển thị WebView nhỏ).
- Nếu `alreadyDecided == true` → hiện trạng thái + `approveBy`, **ẩn nút**.
- Nếu `canDecide == false` → hiện không có quyền.
- Nếu `canDecide == true` → 2 nút **Approve** / **Deny**.
- Deny: bắt buộc nhập `remarks`.

### Quyết định

`POST /api/mobile/phieu-approve/{phieuToken}/decide`

Body:

```json
{ "approve": true, "remarks": null }
```

hoặc

```json
{ "approve": false, "remarks": "Sai số tiền" }
```

Success:

```json
{ "flag": true, "message": "Đã Approve phiếu thu." }
```

Error (đã bị người khác duyệt / không quyền):

```json
{ "flag": false, "message": "Phiếu đã được Approve bởi ..." }
```

HTTP thường `400` khi fail — vẫn đọc `message`.

---

## 4) Deep link từ notification

Khi user tap item có `isPhieuApprove == true`:

1. `POST /notifications/{id}/read`
2. Navigate màn Approve với `phieuToken` (= `phieuToken` field, **không** tự parse message trừ khi field null).
3. Gọi `GET /phieu-approve/{token}` rồi hiển thị UI.

Message format (nếu cần parse thủ công):

```text
[PHIEU_APPROVE|{Loai}|{Token}] ...
```

`Loai` = `Thu` | `Chi`.

---

## 5) Push FCM (nên làm)

### Đăng ký token sau login

`POST /api/mobile/device/push-token`

```json
{ "token": "<fcm_device_token>", "platform": "android" }
```

`platform`: `android` | `ios`

### Gỡ token khi logout

`DELETE /api/mobile/device/push-token`

```json
{ "token": "<fcm_device_token>", "platform": "android" }
```

**Lưu ý:** Backend đã lưu device token. Việc **gửi FCM từ server khi có yêu cầu duyệt** có thể đang/ sẽ bổ sung. Flutter vẫn phải:

- Xin quyền notification
- Lấy FCM token → register API
- Handle foreground / background / terminated tap → mở màn Approve bằng `phieuToken` trong data payload (khi server gửi).

Payload FCM kỳ vọng (khi server bật push):

```json
{
  "type": "PHIEU_APPROVE",
  "loai": "Thu",
  "token": "abc123...",
  "title": "Yêu cầu duyệt phiếu thu",
  "body": "PT001 từ UserA"
}
```

---

## 6) SignalR realtime (optional khi app đang mở)

Hub: `/notificationhub?access_token=<jwt>`  
(hoặc `?userid=<usrId>` theo web cũ)

Events:

- `ReceiveNotification` (string message)
- `UpdateUnreadEmailCount` (int)

Khi nhận `ReceiveNotification` → refresh list notifications.

Package: `signalr_netcore` hoặc tương đương.

---

## 7) Đồng bộ với Web — quy tắc bắt buộc

- **Một phiếu chỉ được Approve hoặc Deny một lần** bởi một user.
- Web / Email / Flutter dùng chung token + DB → Flutter **không** cache trạng thái duyệt lâu; luôn GET detail trước khi hiện nút.
- Sau khi decide thành công → refresh notification list.
- User có quyền duyệt: trên web tick `Duyet_Phieu_Thu` / `Duyet_Phieu_Chi` trong User List.

---

## 8) Màn hình cần có

1. **Login** (databaseName, sqlUser, sqlPass, appUser, appPass) — có thể ẩn SQL fields nếu app đã config sẵn env.
2. **Inbox / Notifications** (badge unread).
3. **PhieuApprovePage(token)**:
   - Title: Duyệt phiếu thu/chi + số phiếu
   - Chi tiết
   - TextField remarks
   - Button Approve (green) / Deny (red)
   - State: loading / already decided / no permission / error

---

## 9) Model Dart gợi ý

```dart
class MobileLoginResult {
  final bool flag;
  final String message;
  final String? accessToken;
  final DateTime? expiresAtUtc;
  final String? tenantId;
  final String? usrId;
  final String? name;
}

class AppNotification {
  final String id;
  final String message;
  final DateTime createdAt;
  final bool isRead;
  final bool isPhieuApprove;
  final String? phieuLoai; // Thu | Chi
  final String? phieuToken;
}

class PhieuApproveDetail {
  final String loai;
  final String token;
  final String? soPhieu;
  final String detailHtml;
  final bool alreadyDecided;
  final bool canDecide;
  final bool? approve;
  final String? approveBy;
  final String? remarks;
}
```

---

## 10) Acceptance criteria

- [ ] Login lấy JWT, gọi được `/auth/me`
- [ ] List notifications + unread badge
- [ ] Tap noti phiếu → mở Approve page đúng token
- [ ] Approve thành công → web cũng thấy Approved
- [ ] Deny kèm remarks → web thấy Denied + Remarks
- [ ] User thứ 2 mở cùng phiếu → `alreadyDecided = true`, không bấm được
- [ ] Register FCM token sau login
- [ ] Logout xóa token + clear secure storage

---

## 11) Không làm

- Không tạo API/backend riêng
- Không duyệt local-only / SQLite override trạng thái server
- Không bỏ qua JWT
- Không hardcode HTML approve như email page; dùng API JSON `/api/mobile/phieu-approve`

---

Hết prompt.
