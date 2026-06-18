# Hướng dẫn lấy file `.mrt` theo từng công ty để mở Stimulsoft Designer

Tài liệu này mô tả cách lấy template Bill Sea **riêng của từng công ty** (multi-tenant) từ database, mở bằng **Stimulsoft Report Designer**, chỉnh sửa và đưa lại vào hệ thống.

---

## 1. Cách hệ thống đang lưu

| Template | Cột SQL (bảng `CompanyInfomation`) | File mặc định trong project |
|----------|-------------------------------------|-----------------------------|
| Bill Sea trang chính | `BillSeaLayoutMrt` | `wwwroot/Reports/BillSea_NVOCC.mrt` |
| Bill Sea trang đính kèm | `BillSeaLayoutAttMrt` | `wwwroot/Reports/BillSea_NVOCC_Att_1.mrt` |

- Mỗi **database tenant** (mỗi công ty login SQL riêng) có bảng `CompanyInfomation` riêng.
- Sau khi user bấm **Lưu layout công ty** (menu **1.17**), nội dung file `.mrt` được lưu dạng **`VARBINARY(MAX)`** — bản chất vẫn là file `.mrt` (XML Stimulsoft), không phải định dạng khác.
- Nếu cột **NULL** hoặc rỗng → công ty đó đang dùng **file mặc định** trong `wwwroot/Reports/`.

---

## 2. Kiểm tra công ty đã có bản riêng chưa

Kết nối đúng **database của công ty** (tenant), chạy:

```sql
SELECT
    CompanyID,
    Name,
    CASE
        WHEN BillSeaLayoutMrt IS NULL THEN N'Chưa có — dùng BillSea_NVOCC.mrt mặc định'
        ELSE CONCAT(N'Có bản riêng (', DATALENGTH(BillSeaLayoutMrt) / 1024.0, N' KB)')
    END AS MainLayout,
    CASE
        WHEN BillSeaLayoutAttMrt IS NULL THEN N'Chưa có — dùng BillSea_NVOCC_Att_1.mrt mặc định'
        ELSE CONCAT(N'Có bản riêng (', DATALENGTH(BillSeaLayoutAttMrt) / 1024.0, N' KB)')
    END AS AttachLayout
FROM dbo.CompanyInfomation;
```

---

## 3. Lấy file `.mrt` để mở Stimulsoft

### Trường hợp A — Công ty **chưa** lưu bản riêng

Mở trực tiếp file trong project (dùng chung làm template gốc):

- `wwwroot/Reports/BillSea_NVOCC.mrt`
- `wwwroot/Reports/BillSea_NVOCC_Att_1.mrt`

Copy ra thư mục làm việc (ví dụ `D:\BillSeaDesign\TenCongTy\`) rồi mở bằng Stimulsoft Designer.

### Trường hợp B — Công ty **đã** lưu bản riêng trong DB

Cần **export byte** từ SQL ra file `.mrt` trên disk.

#### Cách 1: PowerShell (khuyến nghị trên Windows)

1. Sửa thông tin kết nối và tên file output trong script bên dưới.
2. Chạy PowerShell **trên máy có quyền đọc database tenant**.

```powershell
# --- Cấu hình ---
$server   = "localhost"           # SQL Server
$database = "TenDatabaseTenant"   # Database của công ty
$outputDir = "D:\BillSeaExport" # Thư mục lưu file .mrt

$connString = "Server=$server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True;"

Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection $connString
$conn.Open()

$query = @"
SELECT BillSeaLayoutMrt, BillSeaLayoutAttMrt
FROM dbo.CompanyInfomation;
"@

$cmd = $conn.CreateCommand()
$cmd.CommandText = $query
$reader = $cmd.ExecuteReader()
$reader.Read() | Out-Null

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

function Export-MrtColumn($columnIndex, $fileName, $defaultSourcePath) {
    if ($reader.IsDBNull($columnIndex)) {
        if (Test-Path $defaultSourcePath) {
            Copy-Item $defaultSourcePath (Join-Path $outputDir $fileName) -Force
            Write-Host "[$fileName] Chua co ban rieng — da copy template mac dinh."
        } else {
            Write-Warning "[$fileName] NULL va khong tim thay file mac dinh: $defaultSourcePath"
        }
        return
    }

    $bytes = $reader.GetValue($columnIndex)
    $path = Join-Path $outputDir $fileName
    [System.IO.File]::WriteAllBytes($path, $bytes)
    Write-Host "[$fileName] Da export $([math]::Round($bytes.Length/1KB, 1)) KB -> $path"
}

# Đường dẫn file mặc định trong project (sửa nếu repo ở chỗ khác)
$projectRoot = "C:\Users\PCPV\source\repos\NVOAMASIS"
$defaultMain  = Join-Path $projectRoot "wwwroot\Reports\BillSea_NVOCC.mrt"
$defaultAtt   = Join-Path $projectRoot "wwwroot\Reports\BillSea_NVOCC_Att_1.mrt"

Export-MrtColumn 0 "BillSea_NVOCC.mrt" $defaultMain
Export-MrtColumn 1 "BillSea_NVOCC_Att_1.mrt" $defaultAtt

$reader.Close()
$conn.Close()
Write-Host "Xong. Mo file .mrt bang Stimulsoft Report Designer."
```

#### Cách 2: SQL Server Management Studio (thủ công)

1. Chạy query chỉ lấy một cột (ví dụ trang chính):

```sql
SELECT BillSeaLayoutMrt
FROM dbo.CompanyInfomation;
```

2. Trong kết quả, click ô `BillSeaLayoutMrt` → **Save Results As…** (hoặc copy binary).
3. Lưu với đuôi **`.mrt`** (ví dụ `BillSea_NVOCC.mrt`).

> Lưu ý: Một số phiên bản SSMS lưu binary không thuận tiện — nên dùng PowerShell ở Cách 1.

#### Cách 3: `sqlcmd` + script nhỏ

Có thể viết thêm tool C#/console trong solution nếu cần export hàng loạt nhiều tenant — hiện tại PowerShell đủ cho từng DB.

---

## 4. Mở và design trên Stimulsoft

1. Cài **Stimulsoft Reports** (phiên bản tương thích project, hiện report ghi `ReportVersion` trong file `.mrt`).
2. Mở **Stimulsoft Report Designer**.
3. **File → Open** → chọn file vừa export (`BillSea_NVOCC.mrt` hoặc `BillSea_NVOCC_Att_1.mrt`).
4. Chỉnh layout, band, component, expression… như report thông thường.
5. **File → Save** (vẫn định dạng `.mrt`).

### Lưu ý khi design

| Nội dung | Gợi ý |
|----------|--------|
| Connection string DB | Có thể bị mã hóa trong file; khi export PDF trên app, hệ thống **ghi đè** connection theo tenant đang login. |
| Logo / `FormBillSea` | Runtime inject từ `CompanyInfomation.FormBillSea` — không cần nhúng cứng logo vào `.mrt` nếu dùng component `Text1` / resource `logo_bill`. |
| Data source `{HBL...}` | Giữ nguyên expression/data source có sẵn trừ khi biết rõ thay đổi schema. |
| Editor web (menu 1.17) | Chỉ sửa line, label, vị trí logo… **Không** thay thế toàn bộ Designer; hai kênh có thể dùng song song. |

---

## 5. Đưa file đã design lại vào hệ thống

Sau khi chỉnh trên Stimulsoft, cần **import byte** vào đúng cột của **đúng database tenant**.

### Cách 1: PowerShell import

```powershell
$server   = "localhost"
$database = "TenDatabaseTenant"
$filePath = "D:\BillSeaExport\BillSea_NVOCC.mrt"   # file vua design
$column   = "BillSeaLayoutMrt"                      # hoac BillSeaLayoutAttMrt

$bytes = [System.IO.File]::ReadAllBytes($filePath)
$connString = "Server=$server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True;"

Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection $connString
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "UPDATE dbo.CompanyInfomation SET $column = @data;"
$p = $cmd.Parameters.Add("@data", [System.Data.SqlDbType]::VarBinary, -1)
$p.Value = $bytes
$rows = $cmd.ExecuteNonQuery()
$conn.Close()

Write-Host "Da cap nhat $rows dong. Cot: $column"
```

Lặp lại với `BillSea_NVOCC_Att_1.mrt` → cột `BillSeaLayoutAttMrt`.

### Cách 2: Qua app (không cần Stimulsoft)

- Menu **1.17** → **Tải form** → chỉnh trên web → **Lưu layout công ty**  
  (chỉ phù hợp chỉnh line/label/vị trí, không phải full Designer).

### Cách 3: Khôi phục template mặc định

Trong app: **Khôi phục mặc định** (menu 1.17), hoặc SQL:

```sql
UPDATE dbo.CompanyInfomation
SET BillSeaLayoutMrt = NULL,
    BillSeaLayoutAttMrt = NULL;
```

---

## 6. Quy trình gợi ý (tóm tắt)

```
1. Xác định database tenant của công ty
2. Chạy SQL kiểm tra (mục 2)
3. Export .mrt (PowerShell mục 3B) → thư mục D:\BillSeaExport\TenCongTy\
4. Mở Stimulsoft Designer → chỉnh → Save
5. Import lại DB (mục 5) hoặc test export Bill Sea trên app
6. (Tuỳ chọn) Backup file .mrt trước khi import: copy sang thư mục version control nội bộ
```

---

## 7. File liên quan trong project

| File | Mục đích |
|------|----------|
| `Scripts/Alter_CompanyInfomation_BillSeaLayoutTemplates.sql` | Tạo cột lưu template |
| `Services/BillSeaReportTemplateService.cs` | Load/save template theo tenant |
| `Components/CompanyInformation/Pages/BillSeaLayoutEditorPanel.razor` | Editor web menu 1.17 |
| `Services/ShipmentServices.cs` → `ExportBillSea` | Export PDF dùng template tenant |

---

## 8. Câu hỏi thường gặp

**Hỏi:** File trong DB có mở trực tiếp bằng Stimulsoft không?  
**Đáp:** Không mở thẳng từ SQL — cần export ra file `.mrt` trên disk trước.

**Hỏi:** Sửa file trong `wwwroot/Reports/` có ảnh hưởng công ty đã lưu bản riêng không?  
**Đáp:** **Không.** Công ty đã lưu bản riêng đọc từ DB. Chỉ ảnh hưởng tenant chưa lưu hoặc sau khi **Khôi phục mặc định**.

**Hỏi:** Mỗi tenant có database khác nhau — export nhầm DB?  
**Đáp:** Luôn kiểm tra tên database khi login SQL và cột `Name` trong `CompanyInfomation` trước khi import.

---

*Tài liệu tạo cho NVOAMASIS — Bill Sea layout multi-tenant.*
