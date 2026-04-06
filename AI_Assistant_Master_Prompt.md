# 🤖 MASTER PROMPT: Tích hợp AI Assistant vào dự án .NET Blazor

> **Mục đích**: Hướng dẫn AI (Copilot/ChatGPT) tạo AI Assistant chatbot cho bất kỳ dự án .NET Blazor nào.
> **Tác giả**: Auto-generated từ project LMS-GS
> **Ngày tạo**: 25/03/2026

---

## 📋 PROMPT CHÍNH (Copy và dùng cho project mới)

```
Hãy tích hợp AI Assistant (ChatGPT) vào dự án .NET Blazor của tôi với đầy đủ các tính năng sau:

## 1. YÊU CẦU CHỨC NĂNG

### A. Chat AI cơ bản:
- Giao diện chat floating (nút FAB góc phải dưới)
- Gửi/nhận tin nhắn với OpenAI API (model gpt-4o-mini)
- Lưu lịch sử hội thoại trong session
- Xóa lịch sử chat

### B. Truy vấn dữ liệu hệ thống (Deep Query):
- Tự động phát hiện intent câu hỏi của user
- Truy vấn database qua EF Core dựa trên keyword
- Gửi dữ liệu thực tế kèm câu hỏi cho ChatGPT để phân tích
- Trả lời chi tiết với bảng, thống kê, so sánh

### C. Tìm kiếm Web (External Search):
- Tự nhận diện câu hỏi cần tìm kiếm internet
- Tìm kiếm qua DuckDuckGo API (miễn phí, không cần API key)
- Tổng hợp kết quả web và gửi cho ChatGPT xử lý

### D. Giao diện nâng cao:
- 3 chế độ kích thước: Normal / Expanded / Fullscreen
- Nút copy nội dung tin nhắn
- Render Markdown (bảng, bold, code, headers, lists)
- Animation typing (3 chấm nhảy)
- Badge thông báo khi có tin nhắn mới
- Responsive trên mobile
- Auto-grow input (tự mở rộng khi gõ nhiều dòng)
- Câu hỏi nhanh (quick buttons)
- Hiển thị trạng thái: đang tìm web / đang truy vấn DB / đang suy nghĩ

## 2. CẤU TRÚC FILE CẦN TẠO

### Models/ChatGPTModels.cs
```csharp
// Classes cần tạo:
// - ChatMessage (Role, Content, Timestamp)
// - OpenAIChatRequest (model, messages, temperature, max_tokens)
// - OpenAIMessage (role, content)
// - OpenAIChatResponse (id, choices, usage)
// - OpenAIChoice (message, finish_reason)
// - OpenAIUsage (prompt_tokens, completion_tokens, total_tokens)
// - ChatGPTSettings (ApiKey, Model, BaseUrl, MaxTokens, Temperature)
```

### Services/WebSearchService.cs
```csharp
// - SearchAsync(query, maxResults) → string
// - SearchDuckDuckGoAsync(query) → DuckDuckGo Instant Answer API
// - SearchDuckDuckGoHtmlAsync(query, max) → Scrape HTML results
// - FetchPageContentAsync(url, maxChars) → Extract text from URL
// - ParseDdgHtmlResults, ExtractDdgUrl, StripHtml, ExtractTextFromHtml
// - Models: SearchResult, DdgResponse, DdgTopic, DdgInfobox, DdgInfoboxItem
```

### Services/ChatGPTService.cs
```csharp
// Inject: HttpClient, ChatGPTSettings, AppDbContext, ILogger, WebSearchService
//
// - SendMessageAsync(history, message) → gửi đến OpenAI API
//   + Build system prompt với schema DB + context tổng quan
//   + Include conversation history (last 20 messages)
//
// - QueryDataAsync(userQuestion) → deep query DB
//   + Detect intent qua keywords
//   + Gọi các method query chi tiết theo từng entity
//   + Nếu là external query → gọi WebSearchService
//
// - IsExternalQuery(q) → nhận diện câu hỏi bên ngoài
// - GetDatabaseContextAsync() → thống kê tổng quan
// - Get[Entity]DetailedAsync(q) → query chi tiết cho từng bảng
//   VD: GetJobDetailedAsync, GetCustomerDetailedAsync, ...
```

### Components/ChatGPT/Pages/ChatGPTPanel.razor
```razor
// Blazor component with:
// - <style> block cho toàn bộ CSS
// - FAB button (MudFab)
// - Chat window với 3 size mode
// - Header (icon, title, status, action buttons)
// - Messages area (user bubbles, AI bubbles with markdown)
// - Copy button per message
// - Typing animation
// - Input area (auto-grow, Enter to send)
// - @code block: ToggleChat, SendMessage, CopyMessage, RenderMarkdown, ...
```

### wwwroot/MyJS.js (thêm functions)
```javascript
// - scrollChatToBottom()
// - copyToClipboard(text)
// - renderMarkdown(text) [optional, server-side preferred]
```

### appsettings.json (thêm section)
```json
{
  "ChatGPT": {
    "ApiKey": "sk-proj-YOUR_KEY_HERE",
    "Model": "gpt-4o-mini",
    "BaseUrl": "https://api.openai.com/v1",
    "MaxTokens": 2048,
    "Temperature": 0.7
  }
}
```

### Program.cs (thêm DI registration)
```csharp
// builder.Services.Configure<ChatGPTSettings>(builder.Configuration.GetSection("ChatGPT"));
// builder.Services.AddHttpClient<ChatGPTService>();
// builder.Services.AddScoped<ChatGPTService>();
// builder.Services.AddHttpClient<WebSearchService>();
// builder.Services.AddScoped<WebSearchService>();
```

### MainLayout.razor (thêm component)
```razor
// Thêm <ChatGPTPanel /> trong <AuthorizeView><Authorized> block
```

## 3. CÁCH TÙY BIẾN CHO PROJECT MỚI

### System Prompt:
- Thay đổi tên hệ thống, mô tả nghiệp vụ
- Liệt kê cấu trúc DB (tên bảng, cột quan trọng, quan hệ)
- Quy tắc trả lời phù hợp ngữ cảnh

### Deep Query:
- Mỗi DbSet → 1 method GetXxxDetailedAsync()
- Keywords trigger cho từng entity
- JOIN queries cho quan hệ phức tạp
- Format kết quả dạng markdown table

### IsExternalQuery:
- Thêm/bớt keywords theo domain
- Thêm internal keywords cho entity mới

## 4. QUAN TRỌNG

- Đọc hiểu TOÀN BỘ Models trong project trước khi code
- Đọc AppDbContext để biết tên DbSet chính xác
- Đọc _Imports.razor để biết namespace đã có
- Test build sau mỗi file (dotnet build --no-restore)
- Sử dụng AsNoTracking() cho tất cả query read-only
- Clear ChangeTracker trước mỗi batch query
- Wrap tất cả query trong try-catch để tránh crash
```

---

## 🔧 PROMPT PHỤ: Phân tích DB trước khi code

```
Hãy đọc và phân tích toàn bộ cấu trúc dữ liệu của project này:

1. Đọc file Data/AppDbContext.cs → liệt kê tất cả DbSet
2. Đọc tất cả file trong thư mục Models/ → ghi nhận tên property, kiểu dữ liệu, nullable
3. Xác định quan hệ giữa các bảng (FK, 1-n, n-n)
4. Tạo sơ đồ quan hệ dạng text:
   Entity1 -> Entity2 (1-n, via FK_column)
5. Liệt kê các trường trạng thái (status), phân loại (type), boolean quan trọng
6. Xác định trường ngày tháng để hỗ trợ query theo thời gian

Output: Bảng tổng hợp tất cả entity với cột: Tên bảng | DbSet name | Các cột chính | FK | Quan hệ
```

---

## 📝 PROMPT PHỤ: Tạo Deep Query cho 1 entity mới

```
Tôi có thêm entity [TÊN_ENTITY] trong hệ thống với các trường:
- [field1]: [type] - [mô tả]
- [field2]: [type] - [mô tả]
- ...
- Quan hệ: [entity] -> [entity khác] qua [FK]

Hãy:
1. Thêm method Get[Entity]DetailedAsync(string q) vào ChatGPTService.cs
2. Thêm keywords trigger vào QueryDataAsync()
3. Thêm mô tả entity vào system prompt
4. Thêm 1 quick button liên quan vào ChatGPTPanel.razor
5. Cập nhật GetFullOverviewAsync() để đếm entity mới
6. Cập nhật IsExternalQuery() thêm internal keywords mới
```

---

## 🎨 PROMPT PHỤ: Tùy chỉnh giao diện

```
Hãy tùy chỉnh giao diện AI Assistant:
- Màu chủ đạo: [HEX color 1] → [HEX color 2] (gradient)
- Tên hiển thị: [Tên Assistant]
- Subtitle: [Mô tả ngắn]
- Icon: [Material Icon name]
- Quick buttons: [Danh sách câu hỏi nhanh]
- Kích thước mặc định: [Normal/Expanded]
```

---

## 🔑 CHECKLIST TRIỂN KHAI

- [ ] Lấy OpenAI API Key từ https://platform.openai.com/api-keys
- [ ] Thêm API Key vào appsettings.json (section ChatGPT)
- [ ] Tạo Models/ChatGPTModels.cs
- [ ] Tạo Services/WebSearchService.cs
- [ ] Tạo Services/ChatGPTService.cs (đã đọc hiểu DB)
- [ ] Tạo Components/ChatGPT/Pages/ChatGPTPanel.razor
- [ ] Thêm JS functions vào wwwroot (scrollChatToBottom, copyToClipboard)
- [ ] Đăng ký services trong Program.cs
- [ ] Thêm ChatGPTPanel vào MainLayout.razor
- [ ] Build test: `dotnet build --no-restore` → 0 errors
- [ ] Test runtime: hỏi câu nội bộ + câu bên ngoài
- [ ] Publish: `dotnet publish -c release -o ./publish`

---

## 📦 PACKAGES CẦN CÓ (thường đã có sẵn)

```xml
<!-- Không cần thêm package mới -->
<!-- Chỉ cần project đã có: -->
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="MudBlazor" />
<!-- HttpClient có sẵn trong .NET -->
<!-- System.Text.Json có sẵn trong .NET -->
```

---

## 💡 TIPS

1. **Model gpt-4o-mini** giá rẻ nhất, phù hợp cho production (~$0.15/1M input tokens)
2. **DuckDuckGo API** miễn phí, không giới hạn, không cần key
3. **MaxTokens 2048** đủ cho hầu hết câu trả lời, tăng lên 4096 nếu cần bảng dài
4. **Temperature 0.7** cân bằng giữa sáng tạo và chính xác
5. **AsNoTracking()** quan trọng để tránh memory leak khi query nhiều
6. **Conversation history last 20** messages để tránh vượt token limit
7. **System prompt** nên liệt kê schema DB đầy đủ để AI hiểu cấu trúc
8. **Markdown rendering** server-side (C#) ổn định hơn JS-based

---

## 📄 VÍ DỤ SỬ DỤNG PROMPT

### Cho project E-Commerce:
```
Hãy tích hợp AI Assistant vào dự án .NET Blazor E-Commerce của tôi.
[Paste master prompt ở trên]

Các entity chính:
- Product (ProductId, Name, Price, CategoryId, Stock, Status)
- Order (OrderId, CustomerId, TotalAmount, Status, CreatedDate)
- OrderItem (OrderItemId, OrderId, ProductId, Quantity, UnitPrice)
- Customer (CustomerId, Name, Email, Phone, Address)
- Category (CategoryId, Name, ParentId)
- Payment (PaymentId, OrderId, Amount, Method, Status)

Quan hệ: Order -> OrderItem (1-n) -> Product
         Order -> Customer (n-1)
         Order -> Payment (1-1)
         Product -> Category (n-1)

Quick buttons:
- 📊 Doanh thu hôm nay
- 📦 Đơn hàng chờ xử lý  
- 🏆 Top sản phẩm bán chạy
- 👥 Khách hàng VIP
- 🌐 Tìm kiếm thông tin
```

### Cho project HR Management:
```
Hãy tích hợp AI Assistant vào dự án .NET Blazor HR Management.
[Paste master prompt ở trên]

Các entity chính:
- Employee (EmployeeId, Name, Department, Position, Salary, JoinDate, Status)
- Attendance (AttendanceId, EmployeeId, Date, CheckIn, CheckOut, Status)
- LeaveRequest (LeaveId, EmployeeId, Type, StartDate, EndDate, Status)
- Payroll (PayrollId, EmployeeId, Month, BaseSalary, Bonus, Deductions, NetPay)
- Department (DeptId, Name, ManagerId)

Quick buttons:
- 👥 Tổng quan nhân sự
- 📅 Chấm công hôm nay
- 💰 Bảng lương tháng này
- 📝 Đơn nghỉ phép chờ duyệt
- 🌐 Tìm kiếm quy định lao động
```
