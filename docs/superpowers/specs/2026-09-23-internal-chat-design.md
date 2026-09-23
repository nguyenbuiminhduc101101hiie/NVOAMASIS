# Chat nội bộ giữa user — Design

- Ngày: 2026-09-23
- Tham khảo: Booking Chat v2.0 của LogisticsExport (`docs/frontend/chat.md`, `BookingChatHub`, `SqlBookingChatService`)
- Phạm vi: chỉ web Blazor Server (không REST API/hub cho mobile, không FCM push)

## 1. Mục tiêu

Nhân viên trong cùng tenant nhắn tin trực tiếp 1-1 và theo nhóm, realtime, có đính kèm file/ảnh, read receipt, typing indicator, tìm kiếm tin nhắn và badge chưa đọc trên app bar.

Đồng thời vá lỗ hổng xác thực của `NotificationHub` (mục 8).

Không thay đổi trang Message/`Notifications` hiện tại — vẫn dùng cho thông báo hệ thống (duyệt phiếu, nghỉ phép…).

## 2. Khác biệt so với LogisticsExport (cố ý)

| LogisticsExport | NVOAMASIS | Lý do |
|---|---|---|
| Chat gắn booking | Conversation 1-1 / nhóm độc lập | Yêu cầu nghiệp vụ |
| SignalR hub + client websocket | `ChatNotifier` singleton in-process | Component Blazor Server đã chạy trên server; không cần kết nối thứ hai |
| Load toàn bộ lịch sử + toàn bộ bảng Users | Phân trang 50 tin, chỉ join sender của trang | Hiệu năng |
| `IsReadByCurrentUser` luôn `true` với tin của mình → ✓✓ luôn hiện | Tính từ `LastReadAt` của người nhận | Sửa bug |
| Push chạy `Task.Run` dùng DbContext của scope đã dispose | Không có push (chỉ web) | — |
| Search client-side trên list đã tải | Search server-side | Vì có phân trang |
| Upload file qua REST | `InputFile` trong component | Blazor Server |

## 3. Dữ liệu (EF migration, trong DB của từng tenant)

### `ChatConversations`
| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier PK | |
| Type | int | 1 = Direct, 2 = Group |
| Name | nvarchar(200) null | Chỉ nhóm |
| DirectKey | nvarchar(80) null | `"{minUserId:N}_{maxUserId:N}"`; unique index có filter `DirectKey IS NOT NULL` → không tạo trùng hội thoại 1-1 |
| CreatedByUserId | uniqueidentifier | |
| CreatedAt | datetime2 | UTC |
| LastMessageAt | datetime2 null | UTC, để sắp xếp danh sách |
| LastMessagePreview | nvarchar(200) null | Denormalize cho sidebar |

### `ChatParticipants`
| Cột | Kiểu | Ghi chú |
|---|---|---|
| ConversationId | uniqueidentifier | PK (ConversationId, UserId) |
| UserId | uniqueidentifier | = `AuthUser.UsrId`; index riêng |
| Role | int | 1 = Owner, 2 = Member |
| JoinedAt | datetime2 | UTC |
| LastReadAt | datetime2 null | UTC; nguồn cho unread và read receipt |

Rời nhóm = xoá dòng participant (tin nhắn cũ giữ nguyên, sender vẫn hiện tên qua `UserList`).

### `ChatMessages`
| Cột | Kiểu | Ghi chú |
|---|---|---|
| Id | uniqueidentifier PK | |
| ConversationId | uniqueidentifier | Index (ConversationId, CreatedAt) |
| SenderUserId | uniqueidentifier | Guid.Empty cho tin System |
| Kind | int | 1 = Text, 2 = File, 3 = System (vd "A đã thêm B vào nhóm") |
| Text | nvarchar(4000) | Rỗng với File |
| AttachmentPath | nvarchar(500) null | Đường dẫn tương đối trong thư mục chat |
| AttachmentName | nvarchar(260) null | |
| AttachmentContentType | nvarchar(150) null | |
| AttachmentSizeBytes | bigint null | |
| CreatedAt | datetime2 | UTC |

### Quy tắc tính
- **Unread** của user trong 1 conversation = số message có `CreatedAt > participant.LastReadAt` (hoặc tất cả nếu null) và `SenderUserId != user` và `Kind != System`. Tổng unread tính bằng 1 query group-by cho tất cả conversation của user.
- **Read receipt 1-1**: tin của mình hiện ✓ (đã gửi), ✓✓ khi `other.LastReadAt >= message.CreatedAt`.
- **Read receipt nhóm**: chỉ dưới tin cuối cùng của mình, "Đã xem bởi N"; tooltip liệt kê tên.

## 4. Service

### `ChatService` (scoped)
Dùng `IDbContextFactory<AppDbContext>` (context ngắn hạn cho mỗi thao tác) để tránh truy cập đồng thời một DbContext khi event realtime đến giữa lúc đang render. Mọi method nhận `currentUserId` và **kiểm tra membership**; không phải thành viên → `UnauthorizedAccessException`.

- `GetConversationsAsync(userId)` → danh sách kèm tên hiển thị (1-1: tên người kia), preview, `LastMessageAt`, unread.
- `GetTotalUnreadAsync(userId)`
- `GetOrCreateDirectAsync(userId, otherUserId)` — dựa trên `DirectKey`; bắt lỗi unique khi 2 người mở cùng lúc → đọc lại.
- `CreateGroupAsync(userId, name, memberIds)` — người tạo là Owner; ≥ 2 thành viên khác.
- `AddMembersAsync`, `RemoveMemberAsync` (chỉ Owner), `RenameGroupAsync` (chỉ Owner), `LeaveGroupAsync` (Owner rời → chuyển Owner cho thành viên tham gia sớm nhất; nhóm rỗng thì giữ lịch sử, không xoá).
- `GetMessagesAsync(userId, conversationId, beforeCreatedAt?, take = 50)` → tin mới nhất trước, UI đảo lại.
- `SearchMessagesAsync(userId, conversationId, keyword, take = 50)` — `LIKE` trên `Text` và `AttachmentName`.
- `SendTextAsync(userId, conversationId, text)` — trim, không rỗng, ≤ 4000 ký tự; cập nhật `LastMessageAt/Preview`; sender tự động mark read.
- `SendAttachmentAsync(userId, conversationId, fileName, contentType, size, stream)` — ≤ 25 MB; ghi file trước rồi mới lưu DB; DB lỗi → xoá file.
- `MarkReadAsync(userId, conversationId)` — đặt `LastReadAt = CreatedAt` của tin mới nhất; bỏ qua nếu không đổi.
- `GetReadStatesAsync(userId, conversationId)` → `LastReadAt` của các participant khác (cho read receipt).

Sau mỗi thao tác ghi thành công, `ChatService` gọi `ChatNotifier` để phát sự kiện tới participants.

### `ChatNotifier` (singleton)
Pub/sub in-memory, key `(tenantKey, userId)`; `tenantKey` = `ITenantContext.TenantId` hoặc `"default"` khi tắt MultiTenant.

- `IDisposable Subscribe(tenantKey, userId, IChatListener listener)`
- `Publish(tenantKey, IEnumerable<Guid> userIds, ChatEvent evt)`
- Sự kiện: `MessageReceived(message)`, `ReadUpdated(conversationId, userId, lastReadAt)`, `Typing(conversationId, userId, name)`, `ConversationChanged(conversationId)` (tạo/đổi tên/thêm/xoá thành viên).
- Listener lỗi không làm hỏng các listener khác (try/catch + log). Thread-safe (`ConcurrentDictionary`).
- Payload tin nhắn trung lập (không có `IsOwnMessage`); UI tự so `SenderUserId` với user hiện tại.

**Giới hạn đã chấp nhận:** chỉ đúng khi chạy 1 instance. Scale-out → thay implementation bằng SignalR + Redis backplane, giữ nguyên interface.

### Lưu file
Thư mục `{ContentRoot}/App_Data/chat/{tenantKey}/{conversationId}/{messageId}_{safeFileName}` (ngoài `wwwroot`). Cấu hình ghi đè bằng `Chat:StorageRoot` trong appsettings. Tên file được sanitize (bỏ ký tự path).

### `ChatAttachmentController`
`GET /api/chat/attachments/{messageId}` — `[Authorize]` (cookie), user từ claim `ClaimTypes.Sid`, kiểm tra membership, stream file. Query `?inline=1` cho ảnh (thumbnail/preview), mặc định download.

## 5. Giao diện

- **App bar** (`MainLayout`): icon chat + badge tổng unread; subscribe `ChatNotifier` để cập nhật realtime. Click → mở tab "Chat" (cơ chế `OpenTab` hiện có).
- **Snackbar** khi có tin mới và user không đang xem đúng conversation đó; click → mở tab Chat tại conversation đó.
- **Trang Chat** (`Components/Chat/Pages/ChatIndex.razor`), bố cục 2 cột:
  - Trái: ô tìm hội thoại, nút "Tin nhắn mới" (dialog chọn 1 user → chat 1-1; chọn nhiều + đặt tên → nhóm), danh sách conversation (avatar chữ cái, tên, preview, thời gian, badge unread), sắp theo `LastMessageAt`.
  - Phải: header (tên, số thành viên, nút tìm kiếm, menu nhóm: đổi tên / thành viên / rời nhóm), danh sách tin có separator theo ngày, cuộn lên tải thêm 50 tin, bubble của mình bên phải.
  - Ảnh: thumbnail, click xem lớn. File khác: card icon + tên + kích thước, click tải.
  - Typing: "A đang nhập…" / "Nhiều người đang nhập…"; gửi tối đa 1 lần/2 giây, tự ẩn sau 4 giây.
  - Input: textarea tự giãn, Enter gửi, Shift+Enter xuống dòng, nút đính kèm (`InputFile`, `OpenReadStream(25 MB)`), nút gửi disable khi rỗng/đang gửi.
  - Tìm kiếm: ô tìm trên header → danh sách kết quả; click → nhảy tới tin (tải trang chứa tin đó).
  - Mark read khi conversation đang mở và nhận tin mới từ người khác.
- Chuỗi hiển thị qua `IStringLocalizer<SharedResource>` như các trang khác.
- Component con tách riêng: `ConversationList`, `MessagePane`, `MessageBubble`, `ChatComposer`, `NewConversationDialog`, `GroupMembersDialog`.

## 6. Xử lý lỗi

- Lỗi validate/không có quyền → Snackbar tiếng Việt; không hiển thị `Exception.ToString()`.
- File > 25 MB bị chặn ở client (trước khi đọc stream) và server.
- Lỗi DB khi gửi → giữ nội dung trong ô nhập để gửi lại.
- Conversation bị xoá khỏi (bị kick) trong lúc đang mở → nhận `ConversationChanged`, đóng pane, báo "Bạn không còn trong nhóm này".

## 7. Kiểm thử

Không có test project. Xác minh bằng `dotnet build` và chạy app với 2 trình duyệt (2 user, cùng tenant):
1-1 gửi/nhận realtime; tạo nhóm 3 người; thêm/xoá/rời nhóm; ảnh + file ≥ 20 MB và > 25 MB (bị chặn); ✓/✓✓ và "Đã xem bởi N"; typing; tìm kiếm + nhảy tới tin; badge app bar tăng/giảm; tải thêm lịch sử khi cuộn; user không phải thành viên gọi `/api/chat/attachments/{id}` → 403/404.

## 8. Vá lỗ hổng `NotificationHub`

Hiện trạng:
- `CustomUserIdProvider` lấy user id từ `?userid=` khi không có claim → ai biết GUID của người khác có thể nhận thông báo của họ.
- `NotificationHub` không có `[Authorize]`; client ẩn danh gọi được `TestBroadcast` (gửi tới tất cả) và `NotifyUnreadEmailChanged` (sửa badge của bất kỳ ai).

Sửa:
- `MobileJwtTokenService` thêm `CreateHubToken(...)` — JWT ngắn hạn (10 phút), cùng claims.
- `MainLayout.StartHub`: bỏ `?userid=`, dùng `AccessTokenProvider` trả về token mới mỗi lần (re)connect. `HubConnection` chạy trên server nên token không lộ ra trình duyệt.
- `NotificationHub`: `[Authorize(AuthenticationSchemes = JwtBearer)]`; xoá `TestBroadcast` và `NotifyUnreadEmailChanged` (server đã dùng `IHubContext`, không có nơi nào gọi 2 method này).
- `CustomUserIdProvider`: bỏ fallback query string.
- Mobile không bị ảnh hưởng: đã dùng `?access_token=<jwt>` (đã được `OnMessageReceived` xử lý).

## 9. Xoá chat (bổ sung 2026-09-24)

Script: `Scripts/AlterChatTables_AddDelete.sql` (chạy sau `CreateChatTables.sql`) / migration `20260924090000_AddChatDelete`.

| Chức năng | Ai | Hành vi |
|---|---|---|
| Thu hồi tin nhắn | Người gửi, trong 24 giờ | `ChatMessages.DeletedAt` = now; xoá hẳn `Text` + thông tin file, xoá file trên đĩa. Mọi người thấy "Tin nhắn đã được thu hồi" (event `ChatMessageRecalled`). Không tính unread, không xuất hiện trong tìm kiếm. Nếu là tin cuối → `LastMessagePreview = ""` (UI hiển thị chuỗi thu hồi). |
| Xoá hội thoại phía mình | Mọi thành viên | `ChatParticipants.ClearedAt` = now, `IsHidden` = 1, `LastReadAt` = now. Tin có `CreatedAt <= ClearedAt` không còn hiển thị/tìm/tải file với user đó. Tin mới → `IsHidden` = 0 cho mọi participant. Mở lại chat 1-1 qua "Tin nhắn mới" → hiện lại, lịch sử cũ vẫn ẩn. Event `ChatConversationHidden` chỉ gửi cho chính user. |
| Giải tán nhóm | Trưởng nhóm | `DELETE ChatConversations` (FK cascade xoá participants + messages), xoá thư mục file của nhóm. Event `ChatConversationDeleted` → thành viên đang mở thấy "Nhóm đã bị giải tán". Không thể hoàn tác. |

## 10. Trả lời, chuyển tiếp, ghim (bổ sung 2026-09-25)

Script: `Scripts/AlterChatTables_AddForwardPin.sql` (chạy sau `AlterChatTables_AddDelete.sql`) / migration `20260925090000_AddChatForwardPin`. Cột mới trên `ChatMessages`: `ReplyToMessageId`, `IsForwarded`, `PinnedAt`, `PinnedByUserId`; index lọc `IX_ChatMessages_Pinned`.

| Chức năng | Hành vi |
|---|---|
| Trả lời | Rê chuột vào tin → ↩. Ô nhập hiện "Đang trả lời X" (huỷ được). Tin gốc phải cùng hội thoại, không phải tin hệ thống. Bubble hiện khối trích dẫn (tên + 2 dòng nội dung), bấm → nhảy tới tin gốc. Tin gốc bị thu hồi → trích dẫn hiện "Tin nhắn đã được thu hồi". |
| Chuyển tiếp | Chọn tối đa 10 đích: hội thoại đang có và/hoặc người dùng (tự tạo chat 1-1). Tin mới gắn nhãn "↪ Chuyển tiếp", là của người chuyển tiếp. File được **sao chép** sang thư mục hội thoại đích → thu hồi/giải tán bản gốc không ảnh hưởng. Không chuyển tiếp được tin đã thu hồi hoặc nằm trong phần lịch sử đã xoá phía mình. |
| Ghim | Mọi thành viên được ghim/bỏ ghim; tối đa 3 tin/hội thoại, ghim tin thứ 4 thì tin ghim cũ nhất tự bỏ. Thanh ghim dưới header (tin mới nhất, mở rộng xem cả 3), bấm → nhảy tới tin. Tin hệ thống "đã ghim / đã bỏ ghim". Thu hồi tin đang ghim → tự bỏ ghim. Event `ChatPinsChanged`. |

Sửa kèm: tin hệ thống (tạo nhóm, đổi tên, thêm/xoá thành viên, rời nhóm, ghim) giờ được publish `ChatMessageReceived` nên hiện realtime trong khung chat đang mở.

## 11. Chỉnh sửa tin nhắn (bổ sung 2026-09-26)

Script: `Scripts/AlterChatTables_AddEdit.sql` (chạy sau `AlterChatTables_AddForwardPin.sql`) / migration `20260926090000_AddChatEdit`. Cột mới `ChatMessages.EditedAt`.

- Chỉ người gửi, chỉ tin chữ (không áp dụng file/ảnh), chưa thu hồi, trong 24 giờ. Không lưu nội dung cũ.
- ⋮ → "Chỉnh sửa": ô nhập điền sẵn nội dung, thanh "Đang chỉnh sửa tin nhắn" (✕ hoặc Esc để huỷ), Enter/✓ để lưu; nút đính kèm ẩn khi đang sửa. Trả lời và chỉnh sửa loại trừ nhau.
- Tin đã sửa hiện "(đã chỉnh sửa)" cạnh giờ gửi (tooltip = thời điểm sửa). Event `ChatMessageEdited` cập nhật realtime bubble, trích dẫn trả lời, thanh ghim và preview danh sách (nếu là tin cuối).
