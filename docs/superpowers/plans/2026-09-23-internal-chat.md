# Chat nội bộ Implementation Plan

> Spec: `docs/superpowers/specs/2026-09-23-internal-chat-design.md`. Thực thi inline trong phiên (không có test project → xác minh bằng `dotnet build` + chạy app 2 user).

**Goal:** Chat 1-1 + nhóm realtime cho web Blazor Server, kèm vá lỗ hổng `NotificationHub`.

**Architecture:** 3 bảng EF (`ChatConversations`, `ChatParticipants`, `ChatMessages`), `ChatService` (scoped, `IDbContextFactory<AppDbContext>`), `ChatNotifier` (singleton pub/sub in-process theo tenant+user), `ChatFileStorage` (đĩa, ngoài wwwroot), `ChatAttachmentController`, UI MudBlazor mở dạng tab.

**Tech Stack:** .NET 8 Blazor Server, EF Core 8 (SQL Server), MudBlazor 6.12.

## File map

| File | Trách nhiệm |
|---|---|
| `Models/Chat/ChatEntities.cs` | Entity + enum |
| `Models/Chat/ChatDtos.cs` | DTO cho UI và event |
| `Data/AppDbContext.cs` | DbSet + cấu hình key/index |
| `Migrations/20260923100000_AddInternalChat.cs` | Migration SQL idempotent |
| `Scripts/CreateChatTables.sql` | Script tay tương đương cho từng tenant DB |
| `Scripts/Localization_Chat.sql` | Chuỗi vi/en/zh |
| `Services/Chat/ChatNotifier.cs` | Pub/sub in-process |
| `Services/Chat/ChatFileStorage.cs` | Lưu/đọc/xoá file đính kèm |
| `Services/Chat/ChatService.cs` | Nghiệp vụ + kiểm tra membership + publish |
| `Services/Chat/ChatIdentity.cs` | Lấy userId/tenantKey từ claims |
| `Controllers/ChatAttachmentController.cs` | Tải file có kiểm tra quyền |
| `Components/Chat/Pages/ChatIndex.razor` | Trang 2 cột, điều phối state |
| `Components/Chat/Pages/ConversationList.razor` | Danh sách hội thoại |
| `Components/Chat/Pages/MessageBubble.razor` | 1 tin nhắn |
| `Components/Chat/Pages/ChatComposer.razor` | Ô nhập + đính kèm + typing |
| `Components/Chat/Pages/NewConversationDialog.razor` | Tạo 1-1 / nhóm |
| `Components/Chat/Pages/GroupMembersDialog.razor` | Thành viên, đổi tên, thêm/xoá |
| `Services/Chat/ChatNavigator.cs` | Scoped: yêu cầu mở hội thoại từ snackbar |
| `wwwroot/css/chat.css`, `wwwroot/js/chat.js` | Style + cuộn/giữ vị trí |
| `Components/Layout/MainLayout.razor` | Icon + badge, snackbar, dispose, hub token |
| `Hubs/NotificationHub.cs`, `Hubs/CustomUserIdProvider.cs`, `Services/MobileJwtTokenService.cs`, `Program.cs` | Vá lỗ hổng + đăng ký DI |

## Tasks

- [x] 1. Entities, DTOs, DbContext config, migration + SQL script → build
- [x] 2. `ChatNotifier`, `ChatIdentity`, `ChatFileStorage` → build
- [x] 3. `ChatService` đầy đủ method theo spec §4 → build
- [x] 4. `ChatAttachmentController` + DI trong `Program.cs` → build
- [x] 5. UI: `ChatIndex` + component con + css/js → build
- [x] 6. `MainLayout`: icon chat + badge realtime + snackbar + `ChatNavigator` + `IAsyncDisposable` → build
- [x] 7. Vá `NotificationHub` (JWT ngắn hạn, `[Authorize]`, bỏ `?userid=`, xoá method client-callable) → build
- [x] 8. `Scripts/Localization_Chat.sql`
- [ ] 9. Áp script lên DB dev, chạy app, kiểm tra theo spec §7
- [ ] 10. Commit (khi user đồng ý)
