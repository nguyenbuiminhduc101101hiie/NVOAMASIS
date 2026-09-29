# 12.10 Chấm công hằng ngày — triển khai & API mobile

## Triển khai

1. Chạy trên **từng tenant DB + DB template** (chạy lại nhiều lần vẫn an toàn):
   - `Scripts/AlterAttendanceLogsDaily.sql`: thêm cột vào `AttendanceLogs`, index, mã quyền `HR_Attendance`, cấp quyền cho phòng ADMIN.
     Script này tương đương migration `20261001090000_AddHrAttendanceDaily`.
   - `Scripts/Localization_HR.sql`: nhãn giao diện.
2. Restart app (vì localizer có cache).
3. Mở **12.8 Cài đặt nhân sự** và cấu hình:
   - **Chế độ chấm công**:
     - "Theo buổi": như cũ.
     - "Giờ vào – giờ ra": chấm bất kỳ lúc nào, có tính đi muộn / về sớm.
   - Giờ vào làm, nghỉ trưa, làm chiều, tan làm, tan làm T7, số phút cho phép muộn.
   - Chấm công trên điện thoại:
     - Vĩ độ / kinh độ văn phòng: lấy từ Google Maps.
     - Bán kính.
     - Có bắt buộc ở văn phòng mới được chấm không.
   - Danh sách IP văn phòng vẫn đặt ở thông tin công ty (`CompanyInfomation.IPAddress`). Giờ đã hỗ trợ dải IP, ví dụ `115.77.188.0/24`.
4. Cấp quyền `HR_Attendance` cho HR tại **1.6 Phân quyền**:
   - See: xem công của mọi nhân viên theo ngày.
   - Edit: chấm bù và xóa chấm bù.

## Quy tắc

- Mỗi lần chấm là 1 dòng trong `AttendanceLogs`, kèm `Session` (sáng/chiều). Bảng công 12.7 và bảng lương 12.9 không phải sửa gì.
- Ở chế độ giờ vào – giờ ra:
  - Lần chấm **vào** trước giờ nghỉ trưa tính buổi sáng, sau đó tính buổi chiều.
  - Lần chấm **ra** trước giờ làm chiều tính buổi sáng, sau đó tính buổi chiều.
  - Ví dụ:
    - Vào 8:05, ra 17:10: đủ 1 công.
    - Vào 8:00, ra 11:30: 0,5 công, buổi chiều là V.
    - Chỉ có giờ vào mà không có giờ ra (ngày đã qua): trạng thái "Quên chấm ra", buổi chiều là V. HR chấm bù.
- Giờ vào tính theo lần vào sớm nhất; giờ ra tính theo lần ra muộn nhất. Nếu nghỉ phép buổi sáng thì mốc tính đi muộn là giờ làm chiều.
- Nhân viên được **tính là ở văn phòng** khi có một trong hai:
  - IP thuộc danh sách IP văn phòng (web, hoặc điện thoại đang dùng WiFi văn phòng).
  - GPS nằm trong bán kính văn phòng.
- Khi chấm ngoài văn phòng:
  - Nếu cho phép: ghi là "từ xa" (TX).
  - Nếu bật "Điện thoại chỉ chấm được khi ở văn phòng": app bị từ chối chấm công.
- Về chấm bù:
  - Mỗi lần HR chấm bù được lưu với `IsManual = 1`, lý do và người nhập.
  - Chỉ xóa được lần chấm bù. Lần chấm thật của nhân viên luôn được giữ lại.
  - Tháng đã chốt lương thì không sửa được.

## API mobile

Cả 3 API dùng cùng xác thực với các API mobile hiện có: `Authorization: Bearer <token>`, token lấy từ `POST /api/mobile/auth/login`.

### `GET /api/mobile/attendance/today`
```json
{
  "mode": "inout", "date": "2026-09-28T00:00:00", "dayKind": 0,
  "punches": [{ "id": "...", "time": "2026-09-28T08:03:00", "punchType": "in", "session": "morning",
                "source": "MOBILE", "isOnsite": true, "ip": "115.77.188.9", "distanceM": 35,
                "isManual": false, "note": null, "createdBy": null }],
  "canPunch": true, "nextPunchType": "out", "currentSession": null, "remind": false,
  "firstIn": "08:03:00", "lastOut": null, "lateMinutes": 0,
  "buttonText": "Chấm công ra", "hint": "Vào 08:03",
  "workStart": "08:00:00", "workEnd": "17:00:00",
  "hasOfficeLocation": true, "officeRadiusM": 200, "requireOnsite": false
}
```
Cách app dùng dữ liệu này:
- Nút chấm công: hiển thị `buttonText`, và chỉ cho bấm khi `canPunch = true`.
- `dayKind`: 0 là ngày làm, 1 là T7 làm sáng, 2 là ngày nghỉ, 3 là ngày lễ.

### `POST /api/mobile/attendance/punch`
```json
{ "punchType": null, "latitude": 21.028511, "longitude": 105.854165, "accuracyM": 15 }
```
- `punchType`: nhận `"in"`, `"out"`, hoặc `null`. Nếu `null`, server tự chọn: chưa chấm vào thì ghi vào, đã vào thì ghi ra.
- Nên gửi kèm GPS nếu `hasOfficeLocation = true`. Nếu không có GPS, server chỉ kiểm tra được IP.
- Thành công trả `200 { flag: true, message, punchType, time, isOnsite, distanceM }`.
- Bị từ chối trả `422` cùng cấu trúc với `flag: false`. Hiển thị `message` cho người dùng, ví dụ: "Chỉ chấm công được tại văn phòng. Bạn đang cách văn phòng khoảng 850 m (cho phép 200 m)."

### `GET /api/mobile/attendance/history?year=2026&month=9`
Trả về các trường sau:
- `lateCount`, `lateMinutes`.
- `days[]`, mỗi phần tử có:
  - `date`, `code` (X, TX, P, V, X/V...), `firstIn`, `lastOut`, `lateMinutes`, `earlyMinutes`.
  - `status`: một trong `Present`, `Late`, `MissingOut`, `NotYet`, `Leave`, `Absent`, `Off`, `Holiday`, `NotEmployed`, `NoAccount`.
  - `dayKind`.

Lưu ý: GPS do điện thoại gửi lên có thể bị giả lập. Nếu cần chặt hơn, bật "chỉ chấm tại văn phòng" và dựa vào WiFi văn phòng (IP), vì IP không giả được từ phía app.
