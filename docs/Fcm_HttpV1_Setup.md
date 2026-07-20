# FCM HTTP v1 — Push duyệt phiếu (NVOAMASIS)

Firebase project: **lms-qrscan** (Sender ID `171373076136`)  
Android package: **com.vinalink.qrscan**  
API: FCM HTTP v1 + OAuth service account (Legacy Server Key đã tắt).

## Ops checklist

1. Tải service account JSON từ Firebase Console → Project settings → Service accounts → Generate new private key.
2. Đặt file:
   - Local: `NVOAMASIS/Secrets/firebase-service-account.json`
   - Prod: cùng relative path trên server (hoặc absolute path trong config).
3. Không commit file thật (`Secrets/` đã trong `.gitignore`).
4. Config:

```json
"Fcm": {
  "Enabled": true,
  "ProjectId": "lms-qrscan",
  "ServiceAccountPath": "Secrets/firebase-service-account.json"
}
```

5. Restart NVOAMASIS sau khi thêm file / đổi config.
6. Tenant DB phải có bảng `DevicePushToken` (`Scripts/Create_DevicePushToken.sql`).

## Verify

- Flutter login → `[FCM] ready` / `[Push] registered`
- DB: row `DevicePushToken` cho user
- Web: Gửi yêu cầu duyệt
- Log: `FCM HTTP v1: sent N/N for PHIEU_APPROVE...`
  - 401 → sai service account
  - 404 → sai `ProjectId`
  - File missing → `Không tìm thấy Firebase service account JSON`
