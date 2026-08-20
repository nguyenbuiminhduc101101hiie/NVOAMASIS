using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;


namespace NVOAMASIS.Services
{
    public class Danhmuctaikhoan_services(AppDbContext _context, IWebHostEnvironment _env, AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<DanhMucTaiKhoan>> GetList_Danhmuctaikhoan()
        {
            try
            {
                _context.ChangeTracker.Clear();
                List<DanhMucTaiKhoan> rs = new();

                rs = _context.DanhMucTaiKhoan.OrderByDescending(x => x.Taikhoan).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<DanhMucTaiKhoan>();
            }
        }

        public async Task<List<string>> GetList_Danhmuctaikhoan_search()
        {
             
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.DanhMucTaiKhoan.Where(x => x.Taikhoan != null)
                    .Select(x => x.Taikhoan).Distinct().OrderBy(x => x).ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        
        }
        //------------------------------------------------------
        public async Task<BoolandMessReponse> CreateDanhMucTaiKhoan_Detail(DanhMucTaiKhoan c)
        {
            try
            {

                _context.ChangeTracker.Clear();
                c.Id = Guid.NewGuid();
                _context.DanhMucTaiKhoan.Add(c!);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "ADD Danh Muc Tai Khoan", "Danhmuctaikhoan", c.Id, c.Taikhoan, c);
                return new BoolandMessReponse(true, "Create Account List Success");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Add Account List with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreateDanhMucTaiKhoan(DanhMucTaiKhoan IV, DanhMucTaiKhoan IV_old)
        {
            try
            {
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                _context.ChangeTracker.Clear();
                if (IV.Id == null || IV.Id == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    await HistoryLogService.LogAsync(usr, "ADD Danh Muc Tai Khoan", "Danhmuctaikhoan", IV.Id, IV.Taikhoan, IV);
                    return ["Create new Account Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    await HistoryLogService.LogAsync(usr, "Update Danh Muc Tai Khoan", "Danhmuctaikhoan",IV.Id, IV.Taikhoan, new { OldData = IV_old, NewData = IV });
                    return ["Update Account Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Account Fail", "0"];
            }
        }


        public async Task<BoolandMessReponse> DeleteDanhMucTaiKhoan_Detail(DanhMucTaiKhoan c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.DanhMucTaiKhoan.Remove(c!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Danh Muc Tai Khoan", "Danhmuctaikhoan", c.Id, c.Taikhoan, new { OldData = c});
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Project with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UpdateDanhMucTaiKhoan_Detail(DanhMucTaiKhoan c, DanhMucTaiKhoan c_old)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.DanhMucTaiKhoan.Update(c);
                await _context.SaveChangesAsync();
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Update Danh Muc Tai Khoan", "Danhmuctaikhoan", c.Id, c.Taikhoan, new { OldData = c_old ,NewData =c });
                return new BoolandMessReponse(true, "Update Account  Success");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Update Account  with error code: " + ex.Message);
            }
        }

        public async Task<List<DanhMucTaiKhoan>> GetListDanhMucTaiKhoan_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.DanhMucTaiKhoan.Where(_ => _.Id == id).ToListAsync();
            return Invoices;
        }

        public async Task<AccountExcelParseResult> PreviewAccountsFromExcelAsync(Stream fileStream)
        {
            return await ParseAccountsFromExcelAsync(fileStream);
        }

        public async Task<BoolandMessReponse> SaveImportedAccountsAsync(List<DanhMucTaiKhoan> accounts)
        {
            try
            {
                if (accounts == null || accounts.Count == 0)
                    return new BoolandMessReponse(false, "Không có dữ liệu để import.");

                _context.ChangeTracker.Clear();
                string currentUser = asv.GetAuth().Result.User?.Identity?.Name ?? "Unknown";

                var existingCodes = (await _context.DanhMucTaiKhoan
                        .Where(x => x.Taikhoan != null && x.Taikhoan != "")
                        .Select(x => x.Taikhoan!)
                        .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var toAdd = new List<DanhMucTaiKhoan>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int duplicateInDb = 0;
                int duplicateInFile = 0;

                foreach (var acc in accounts)
                {
                    var code = acc.Taikhoan?.Trim();
                    if (string.IsNullOrWhiteSpace(code))
                        continue;

                    if (!seen.Add(code))
                    {
                        duplicateInFile++;
                        continue;
                    }

                    if (existingCodes.Contains(code))
                    {
                        duplicateInDb++;
                        continue;
                    }

                    acc.Id = Guid.NewGuid();
                    acc.Taikhoan = code;
                    acc.Userupdate = currentUser;
                    acc.Dateupdate = DateTime.Now.ToString("dd/MMM/yyyy");
                    acc.IsActive ??= true;
                    acc.IsPosting ??= true;
                    acc.Approve ??= true;
                    acc.RequiresCustomer ??= false;
                    acc.RequiresSupplier ??= false;
                    acc.RequiresEmployee ??= false;
                    acc.RequiresShipment ??= false;
                    if (string.IsNullOrWhiteSpace(acc.AccountType))
                        acc.AccountType = InferAccountType(code);
                    if (string.IsNullOrWhiteSpace(acc.Manguyente))
                        acc.Manguyente = InferCurrency(code, acc.Tentaikhoan);

                    toAdd.Add(acc);
                }

                if (toAdd.Count > 0)
                {
                    _context.DanhMucTaiKhoan.AddRange(toAdd);
                    await _context.SaveChangesAsync();
                    await HistoryLogService.LogAsync(currentUser, "Import Danh Muc Tai Khoan Excel", "Danhmuctaikhoan", null, $"{toAdd.Count} accounts", new { Count = toAdd.Count });
                }

                var skipNote = (duplicateInDb + duplicateInFile) > 0
                    ? $" Bỏ qua {duplicateInDb} số TK đã tồn tại, {duplicateInFile} số TK trùng trong file."
                    : "";
                return new BoolandMessReponse(true, $"Import thành công {toAdd.Count} tài khoản.{skipNote}");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Import thất bại: " + ex.Message);
            }
        }

        private async Task<AccountExcelParseResult> ParseAccountsFromExcelAsync(Stream fileStream)
        {
            var result = new AccountExcelParseResult();
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var package = new ExcelPackage(memoryStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet?.Dimension == null)
                {
                    result.ErrorMessage = "Không tìm thấy sheet hoặc dữ liệu trong Excel.";
                    return result;
                }

                int startCol = worksheet.Dimension.Start.Column;
                int endCol = worksheet.Dimension.End.Column;
                int startRow = worksheet.Dimension.Start.Row;
                int endRow = worksheet.Dimension.End.Row;

                int headerRow = -1;
                var colMap = new Dictionary<int, string>();
                int scanTo = Math.Min(endRow, startRow + 20);
                for (int r = startRow; r <= scanTo; r++)
                {
                    var map = new Dictionary<int, string>();
                    bool hasAccountNo = false;
                    for (int c = startCol; c <= endCol; c++)
                    {
                        var key = NormalizeExcelHeader(worksheet.Cells[r, c].Text);
                        if (string.IsNullOrEmpty(key))
                            continue;
                        map[c] = key;
                        if (IsAccountNoHeader(key))
                            hasAccountNo = true;
                    }

                    if (hasAccountNo)
                    {
                        headerRow = r;
                        colMap = map;
                        break;
                    }
                }

                if (headerRow < 0)
                {
                    result.ErrorMessage = "Không tìm thấy cột A/C No. (Số tài khoản) trong file Excel.";
                    return result;
                }

                var existingCodes = (await _context.DanhMucTaiKhoan
                        .Where(x => x.Taikhoan != null && x.Taikhoan != "")
                        .Select(x => x.Taikhoan!)
                        .ToListAsync())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string currentUser = asv.GetAuth().Result.User?.Identity?.Name ?? "Unknown";

                for (int row = headerRow + 1; row <= endRow; row++)
                {
                    var acc = new DanhMucTaiKhoan();
                    for (int c = startCol; c <= endCol; c++)
                    {
                        if (!colMap.TryGetValue(c, out var headerKey))
                            continue;
                        ApplyAccountExcelValue(acc, headerKey, worksheet.Cells[row, c].Text);
                    }

                    var code = acc.Taikhoan?.Trim();
                    bool hasData = !string.IsNullOrWhiteSpace(code)
                        || !string.IsNullOrWhiteSpace(acc.Tentaikhoan)
                        || !string.IsNullOrWhiteSpace(acc.Accountname);

                    if (!hasData)
                    {
                        result.EmptySkipped++;
                        continue;
                    }

                    acc.Taikhoan = code;
                    acc.Id = Guid.NewGuid();
                    acc.Userupdate = currentUser;
                    acc.Dateupdate = DateTime.Now.ToString("dd/MMM/yyyy");
                    acc.IsActive = true;
                    acc.Approve = true;
                    acc.RequiresCustomer ??= false;
                    acc.RequiresSupplier ??= false;
                    acc.RequiresEmployee ??= false;
                    acc.RequiresShipment ??= false;
                    if (string.IsNullOrWhiteSpace(acc.Tentaikhoan) && !string.IsNullOrWhiteSpace(acc.Accountname))
                        acc.Tentaikhoan = acc.Accountname;
                    if (string.IsNullOrWhiteSpace(acc.Accountname) && !string.IsNullOrWhiteSpace(acc.Tentaikhoan))
                        acc.Accountname = acc.Tentaikhoan;
                    if (string.IsNullOrWhiteSpace(acc.Diengiai))
                        acc.Diengiai = acc.Tentaikhoan;
                    if (string.IsNullOrWhiteSpace(acc.AccountType))
                        acc.AccountType = InferAccountType(code);
                    if (string.IsNullOrWhiteSpace(acc.Manguyente))
                        acc.Manguyente = InferCurrency(code, acc.Tentaikhoan);
                    InferRequiredFlags(acc);

                    var previewRow = new AccountExcelPreviewRow
                    {
                        ExcelRow = row,
                        Account = acc
                    };

                    if (string.IsNullOrWhiteSpace(code))
                    {
                        previewRow.Status = AccountExcelRowStatus.Invalid;
                        previewRow.StatusReason = "Thiếu số tài khoản (A/C No.)";
                        previewRow.Selected = false;
                        result.InvalidCount++;
                    }
                    else if (!seenInFile.Add(code))
                    {
                        previewRow.Status = AccountExcelRowStatus.DuplicateFile;
                        previewRow.StatusReason = $"Trùng số TK trong file: {code}";
                        previewRow.Selected = false;
                        result.DuplicateInFile++;
                    }
                    else if (existingCodes.Contains(code))
                    {
                        previewRow.Status = AccountExcelRowStatus.DuplicateDb;
                        previewRow.StatusReason = $"Số TK đã tồn tại: {code}";
                        previewRow.Selected = false;
                        result.DuplicateInDb++;
                    }
                    else
                    {
                        previewRow.Status = AccountExcelRowStatus.New;
                        previewRow.StatusReason = string.IsNullOrWhiteSpace(acc.Tentaikhoan) ? "Thiếu tên tài khoản" : null;
                        previewRow.Selected = true;
                        result.ToImport.Add(acc);
                    }

                    result.Rows.Add(previewRow);
                }

                ApplyParentPostingFlags(result.Rows);
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Không đọc được file Excel: " + ex.Message;
                return result;
            }
        }

        private static void ApplyParentPostingFlags(List<AccountExcelPreviewRow> rows)
        {
            var codes = rows
                .Select(r => r.Account.Taikhoan)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var row in rows.Where(r => r.Status == AccountExcelRowStatus.New && !string.IsNullOrWhiteSpace(r.Account.Taikhoan)))
            {
                var code = row.Account.Taikhoan!;
                bool hasChildren = codes.Any(c =>
                    !c.Equals(code, StringComparison.OrdinalIgnoreCase)
                    && c.StartsWith(code, StringComparison.OrdinalIgnoreCase)
                    && c.Length > code.Length);
                row.Account.IsPosting = !hasChildren;
            }
        }

        private static string NormalizeExcelHeader(string? header)
        {
            if (string.IsNullOrWhiteSpace(header))
                return "";
            var chars = header.Trim().ToLowerInvariant()
                .Where(ch => char.IsLetterOrDigit(ch))
                .ToArray();
            return new string(chars);
        }

        private static bool IsAccountNoHeader(string key) =>
            key is "acno" or "accountno" or "accountnumber" or "taikhoan" or "sotk" or "matk"
                or "sotaikhoan" or "mãtk" or "matàikhoản";

        private static void ApplyAccountExcelValue(DanhMucTaiKhoan acc, string headerKey, string? raw)
        {
            var value = raw?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return;

            switch (headerKey)
            {
                case "acno":
                case "accountno":
                case "accountnumber":
                case "taikhoan":
                case "sotk":
                case "matk":
                case "sotaikhoan":
                    acc.Taikhoan = value;
                    break;
                case "acnamelc":
                case "acname":
                case "tentaikhoan":
                case "tentk":
                    acc.Tentaikhoan = value;
                    break;
                case "accountname":
                case "tenenglish":
                    acc.Accountname = value;
                    break;
                case "diengiai":
                case "description":
                    acc.Diengiai = value;
                    break;
                case "manguyente":
                case "currency":
                case "loaitiente":
                    acc.Manguyente = value.ToUpperInvariant();
                    break;
                case "accounttype":
                case "loaitk":
                    acc.AccountType = value;
                    break;
                case "isposting":
                    acc.IsPosting = ParseExcelBool(value);
                    break;
                case "isactive":
                    acc.IsActive = ParseExcelBool(value);
                    break;
            }
        }

        private static bool ParseExcelBool(string value)
        {
            var v = value.Trim().ToLowerInvariant();
            return v is "1" or "true" or "yes" or "y" or "x" or "có" or "co";
        }

        private static string InferAccountType(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Asset";
            var first = code.Trim()[0];
            return first switch
            {
                '1' or '2' => "Asset",
                '3' => "Liability",
                '4' or '9' => "Equity",
                '5' or '7' => "Revenue",
                '6' or '8' => "Expense",
                _ => "Asset"
            };
        }

        private static string InferCurrency(string? code, string? name)
        {
            var text = $"{code} {name}".ToUpperInvariant();
            if (text.Contains("USD") || text.Contains("ĐÔ") || text.Contains("DO LA") || text.Contains("DOLLAR"))
                return "USD";
            return "VND";
        }

        private static void InferRequiredFlags(DanhMucTaiKhoan acc)
        {
            var code = acc.Taikhoan ?? "";
            if (code.StartsWith("131"))
                acc.RequiresCustomer = true;
            if (code.StartsWith("331"))
                acc.RequiresSupplier = true;
            if (code.StartsWith("334"))
                acc.RequiresEmployee = true;
        }
    }
}
