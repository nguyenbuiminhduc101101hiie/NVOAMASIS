using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Net;
using System.Security.Claims;
using static MudBlazor.CategoryTypes;

namespace NVOAMASIS.Services
{
    public class SharedServices(AppDbContext _context, CustomAuthenticationStateProvider _auth, NavigationManager nav, HistoryLogService HistoryLogService,AccountService asv, IJSRuntime JSRuntime, GlobalServices gl,DNTUServices DNTUSV, QuotationService Quosv, PhieuThu_Chi_Services ThuchiSV,IssueReportServices IRSV)
    {
        //demo

        public async Task<List<Permission_M>> GetListPerMission()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var result = await _context.Permissions    
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw;
            }
        }
        public async Task<List<FormMenu>> GetListFormMenu()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var result = await _context.MenuNames.OrderBy(x=>x.Title)
                    .ToListAsync();
                return result;
            }
            catch
            {
                throw;
            }
        }
        public FormMenu GetMenufrommName(string menuname)
        {
            try
            {
                var result = _context.MenuNames
                    .Where(x => x.MenuName.ToLower() == menuname.ToLower())
                    .FirstOrDefault();
                return result;
            }
            catch
            {
                throw;
            }
        }

        public FormMenu GetMenufromid(Guid? id)
        {
            try
            {
                var result = _context.MenuNames
                    .Where(x => x.MenuID == id)
                    .FirstOrDefault();
                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<BoolandMessReponse> UpdateOrCreatePermission(Permission_M p,Permission_M p_old)
        {
            try
            {
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                if (p.PermissionId == null || p.PermissionId == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(p);
                    await _context.SaveChangesAsync();
  
                    await HistoryLogService.LogAsync(usr, "ADD Permission", "Permission", p.PermissionId, p.MenuName, p);
                    return new BoolandMessReponse(true, "Create Permission Success");
                }
                else
                {
                    _context.ChangeTracker.Clear();
                    _context.Update(p);
                    await _context.SaveChangesAsync();
             
                    await HistoryLogService.LogAsync(usr, "Update Permission", "Permission", p.PermissionId, p.MenuName, new { OldData = p_old, NewData = p });
                    return new BoolandMessReponse(true, "Update Permission Success");
                }
            }
            catch(Exception ex)
            {
                return new BoolandMessReponse(false,"Cannot Update or Add Permission with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeletePermission(Permission_M p)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (p?.PermissionId == null || p?.PermissionId == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Permissions.Remove(p!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Permission", "Permission", p.PermissionId, p.MenuName, new { OldData = p});
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Permission with error code: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DeletePermissionbyUser(string usr)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if(string.IsNullOrEmpty(usr))
                    return new BoolandMessReponse(true, "Nothing to Delete");

                var rs = await _context.Permissions.Where(x => x.UserName == usr).ToListAsync();
                if(!rs.Any())
                    return new BoolandMessReponse(true, "Nothing to Delete");
                _context?.Permissions.RemoveRange(rs);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Permission with error code: " + ex.Message);
            }
        }
        public async Task<bool> CheckPermission(string Username, string func, string MenuName)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = _context.Permissions.FromSqlInterpolated($"select * from [Permissions] where username = {Username} and MenuName = {MenuName}")
                    .FirstOrDefault();
                if (rs == null)
                    return false;
                return func.ToLower() switch{
                    "view" => rs.See!.Value,
                    "edit" => rs.Edit!.Value,
                    "delete" => rs.Del!.Value,
                    "approve" => rs.Approve!.Value,
                    "add" => rs.Add!.Value,
                    _ => false
                };

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<HashSet<string>> GetUserViewPermissionsForUser(ClaimsPrincipal user)
        {
            var username = user.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            _context.ChangeTracker.Clear();
            var menuNames = await _context.Permissions
                .AsNoTracking()
                .Where(x => x.UserName == username && x.See == true)
                .Select(x => x.MenuName)
                .ToListAsync();

            return menuNames
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m!.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<BoolandMessReponse> UploadfiletoFTP_DNTU(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_DNTU p)
                {
                    p.Files = string.IsNullOrEmpty(p.Files) ? Link : (p.Files + ";" + Link);
                    var rs = await DNTUSV.UpdateDeNghiTamUng_Detail(p);
                }
               

                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }
        //public async Task<BoolandMessReponse> UploadfiletoFTP_Phieuthu(IBrowserFile file, object model)
        //{
        //    try
        //    {
        //        //ftp server details
        //        var ftpServer = "ftp://115.165.166.130";
        //        var ftpUsername = "hinh";
        //        var ftpPassword = "qweQWE123!@#";
        //        var fileName = file.Name;
        //        int dotIndex = fileName.IndexOf('.');

        //        string namePart = fileName.Substring(0, dotIndex);
        //        string extensionPart = fileName.Substring(dotIndex);

        //        var ID = System.Guid.NewGuid();
        //        var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
        //        // Create FTP request
        //        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
        //        request.Method = WebRequestMethods.Ftp.UploadFile;
        //        request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

        //        // Upload file in chunks (streaming)
        //        try
        //        {
        //            using (Stream ftpStream = await request.GetRequestStreamAsync())
        //            {
        //                using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
        //                {
        //                    byte[] buffer = new byte[8192]; // 8KB buffer
        //                    int bytesRead;

        //                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //                    {
        //                        await ftpStream.WriteAsync(buffer, 0, bytesRead);
        //                    }
        //                }
        //            }

        //            // Nhận phản hồi từ server sau khi upload
        //            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
        //            {
        //                if (response.StatusCode == FtpStatusCode.ClosingData)
        //                {
        //                    Console.WriteLine("Upload thành công!");
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
        //            return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
        //        }

        //        // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
        //        if (model is M_PhieuThu p)
        //        {
        //            p.Files = string.IsNullOrEmpty(p.Files) ? Link : (p.Files + ";" + Link);
        //            var rs = await ThuchiSV.UpdatePhieuthu_Detail(p);
        //        }


        //        return new BoolandMessReponse(true, "Upload Successful!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
        //    }
        //}
        public async Task<BoolandMessReponse> UploadfiletoFTP_DNHU(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_DNHU p)
                {
                    p.Files = string.IsNullOrEmpty(p.Files) ? Link : (p.Files + ";" + Link);
                    var rs = await DNTUSV.UpdatedeNghiHoanUng_Detail(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UploadfiletoFTP_DNTT(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_DNTT_Logistics p)
                {
                    p.Chungtukemtheo = string.IsNullOrEmpty(p.Chungtukemtheo) ? Link : (p.Chungtukemtheo + ";" + Link);
                    var rs = await DNTUSV.UpdateDNTT_Detail(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> UploadfiletoFTP_RFQ(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_RFQ p)
                {
                    p.filesPath = string.IsNullOrEmpty(p.filesPath) ? Link : (p.filesPath + ";" + Link);
                    var rs = await Quosv.UpdateRFQ_Detail(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UploadfiletoFTP_Issue(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_IssueReports p)
                {
                    p.ScreenshotUrl = string.IsNullOrEmpty(p.ScreenshotUrl) ? Link : (p.ScreenshotUrl + ";" + Link);
                    var rs = await IRSV.UpdateCreate_IssueReport(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UploadfiletoFTP(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_Duan p)
                {
                    p.Links = string.IsNullOrEmpty(p.Links) ? Link : (p.Links + ";" + Link);
                    var rs = await gl.UpdateDuAnDetail(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UploadfiletoFTP_YeuCauTuVanHQCO(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_YeuCauTuVanTTHQ p)
                {
                    p.Links = string.IsNullOrEmpty(p.Links) ? Link : (p.Links + ";" + Link);
                    var rs = await gl.UpdateYeuCauTuVanHQCODetail(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UploadfiletoFTP_YeuCauTrucking(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_YeuCauTrucking p)
                {
                    p.Links = string.IsNullOrEmpty(p.Links) ? Link : (p.Links + ";" + Link);
                    var rs = await gl.UpdateYCTruckingDetail(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> UploadfiletoFTP_TKHQ(IBrowserFile file, object model)
        {
            try
            {
                //ftp server details
                var ftpServer = "ftp://115.165.166.130";
                var ftpUsername = "hinh";
                var ftpPassword = "qweQWE123!@#";
                var fileName = file.Name;
                int dotIndex = fileName.IndexOf('.');

                string namePart = fileName.Substring(0, dotIndex);
                string extensionPart = fileName.Substring(dotIndex);

                var ID = System.Guid.NewGuid();
                var Link = $"{ftpServer}/{namePart}-{ID}{extensionPart}";
                // Create FTP request
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Link);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

                // Upload file in chunks (streaming)
                try
                {
                    using (Stream ftpStream = await request.GetRequestStreamAsync())
                    {
                        using (Stream fileStream = file.OpenReadStream(maxAllowedSize: long.MaxValue))
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;

                            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await ftpStream.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }

                    // Nhận phản hồi từ server sau khi upload
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        if (response.StatusCode == FtpStatusCode.ClosingData)
                        {
                            Console.WriteLine("Upload thành công!");
                        }
                        else
                        {
                            Console.WriteLine($"Upload không thành công. Trạng thái: {response.StatusDescription}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi upload file: {ex.Message}");
                    return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
                }

                // Thêm đối tượng vào DbSet và lưu vào cơ sở dữ liệu
                if (model is M_TKHQ_Thongtinchunglohang p)
                {
                    p.Links = string.IsNullOrEmpty(p.Links) ? Link : (p.Links + ";" + Link);
                    var rs = await gl.UpdateTKHQDetail(p);
                }


                return new BoolandMessReponse(true, "Upload Successful!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Upload Fail with Error: " + ex.Message);
            }
        }
        /// <summary>
        /// Creates one permission row per selected menu for a user. Skips menus that already have a row for that user.
        /// </summary>
        public async Task<BoolandMessReponse> CreatePermissionsBulk(
            string? userName,
            IReadOnlyList<Guid> menuIds,
            bool see,
            bool edit,
            bool del,
            bool approve,
            bool add)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    return new BoolandMessReponse(false, "User is required");
                if (menuIds == null || menuIds.Count == 0)
                    return new BoolandMessReponse(false, "Select at least one form");

                _context.ChangeTracker.Clear();
                var existingMenuIds = await _context.Permissions
                    .AsNoTracking()
                    .Where(x => x.UserName == userName && x.MenuId != null)
                    .Select(x => x.MenuId!.Value)
                    .ToListAsync();

                var existingSet = existingMenuIds.ToHashSet();
                var distinctNew = menuIds.Distinct().Where(id => !existingSet.Contains(id)).ToList();
                var skipped = menuIds.Count - distinctNew.Count;

                if (distinctNew.Count == 0)
                    return new BoolandMessReponse(false,
                        skipped > 0
                            ? "All selected forms already have permission for this user."
                            : "Nothing to add.");

                // Avoid EF OPENJSON(... WITH ...) translation for in-memory Guid lists — some SQL Server
                // batches then fail with "Incorrect syntax near the keyword 'WITH'" unless prefixed with ';'.
                List<FormMenu> menus;
                if (distinctNew.Count == 1)
                {
                    var onlyId = distinctNew[0];
                    menus = await _context.MenuNames
                        .AsNoTracking()
                        .Where(m => m.MenuID == onlyId)
                        .ToListAsync();
                }
                else
                {
                    var sqlParameters = distinctNew
                        .Select((id, i) => new SqlParameter($"@m{i}", id))
                        .Cast<object>()
                        .ToArray();
                    var inClause = string.Join(", ", distinctNew.Select((_, i) => $"@m{i}"));
                    var sql = $";SELECT * FROM [MenuNames] WHERE [MenuID] IN ({inClause})";
                    menus = await _context.MenuNames
                        .FromSqlRaw(sql, sqlParameters)
                        .AsNoTracking()
                        .ToListAsync();
                }

                var missingIds = distinctNew.Except(menus.Select(m => m.MenuID)).ToList();

                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                var list = new List<Permission_M>();
                foreach (var menu in menus)
                {
                    list.Add(new Permission_M
                    {
                        PermissionId = Guid.NewGuid(),
                        MenuId = menu.MenuID,
                        MenuName = menu.MenuName,
                        UserName = userName,
                        See = see,
                        Edit = edit,
                        Del = del,
                        Approve = approve,
                        Add = add
                    });
                }

                if (list.Count == 0)
                    return new BoolandMessReponse(false, "No valid menus to add.");

                _context.Permissions.AddRange(list);
                await _context.SaveChangesAsync();

                await HistoryLogService.LogAsync(usr, "ADD Permission (bulk)", "Permission", Guid.Empty, userName,
                    new { UserName = userName, AddedMenus = menus.Select(m => m.MenuName).ToList(), Count = list.Count });

                var msg = $"Added {list.Count} permission(s).";
                if (skipped > 0)
                    msg += $" Skipped {skipped} already assigned.";
                if (missingIds.Count > 0)
                    msg += $" {missingIds.Count} menu id(s) were not found.";
                return new BoolandMessReponse(true, msg);
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot add permissions: " + ex.Message);
            }
        }
        public async Task<BoolandMessReponse> DuplicatePermission(string UserRef, string UserChild)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var ListPerRef = _context.Permissions.Where(x => x.UserName == UserRef).ToList();
                List<Permission_M> listpers = new();
                foreach(var item in ListPerRef)
                {
                    var p = new Permission_M();
                    p.PermissionId = Guid.NewGuid();
                    p.MenuId = item.MenuId;
                    p.MenuName = item.MenuName;
                    p.Add = item.Add;
                    p.See = item.See;
                    p.Edit = item.Edit;
                    p.Del = item.Del;
                    p.Approve = item.Approve;
                    p.UserName = UserChild;
                    listpers.Add(p);
                }
                _context.Permissions.AddRange(listpers);
                var rs = await _context.SaveChangesAsync();

         
                return new BoolandMessReponse(true, "Duplicate Successfully!");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Duplicate with Error: " + ex.Message);

            }

        }

        /// <summary>
        /// Syncs missing permissions from a department template onto existing users in that department.
        /// Only adds menus the user does not already have; does not overwrite existing rows.
        /// </summary>
        public async Task<BoolandMessReponse> SyncPermissionsFromTemplate(string? dept)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dept))
                    return new BoolandMessReponse(false, "Department is required.");

                _context.ChangeTracker.Clear();
                var deptKey = dept.Trim();
                var deptLower = deptKey.ToLower();

                var templates = await _context.PermissionTemplate
                    .AsNoTracking()
                    .Where(x => x.Dept != null && x.Dept.ToLower() == deptLower)
                    .ToListAsync();

                if (templates.Count == 0)
                    return new BoolandMessReponse(false, $"No permission template found for department '{deptKey}'.");

                // Deduplicate template by MenuId (keep first)
                var templateByMenu = new Dictionary<Guid, PermissionTemplate>();
                foreach (var t in templates)
                {
                    if (!t.MenuId.HasValue)
                        continue;
                    templateByMenu.TryAdd(t.MenuId.Value, t);
                }

                if (templateByMenu.Count == 0)
                    return new BoolandMessReponse(false, $"Template '{deptKey}' has no valid menu entries.");

                var users = await _context.UserList
                    .AsNoTracking()
                    .Where(x => x.Department != null && x.Department.ToLower() == deptLower)
                    .ToListAsync();

                if (users.Count == 0)
                    return new BoolandMessReponse(false, $"No users found in department '{deptKey}'.");

                var toAdd = new List<Permission_M>();
                var usersUpdated = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var user in users)
                {
                    if (string.IsNullOrWhiteSpace(user.Usr))
                        continue;

                    var userName = user.Usr;
                    var existingMenuIds = await _context.Permissions
                        .AsNoTracking()
                        .Where(x => x.UserName == userName && x.MenuId != null)
                        .Select(x => x.MenuId!.Value)
                        .ToListAsync();
                    var existingSet = existingMenuIds.ToHashSet();

                    foreach (var (menuId, tmpl) in templateByMenu)
                    {
                        if (existingSet.Contains(menuId))
                            continue;

                        toAdd.Add(new Permission_M
                        {
                            PermissionId = Guid.NewGuid(),
                            MenuId = menuId,
                            MenuName = tmpl.MenuName,
                            UserName = userName,
                            See = tmpl.canView ?? false,
                            Add = tmpl.canAdd ?? false,
                            Edit = tmpl.canEdit ?? false,
                            Del = tmpl.canDelete ?? false,
                            Approve = tmpl.canApprove ?? false
                        });
                        usersUpdated.Add(userName);
                    }
                }

                if (toAdd.Count == 0)
                    return new BoolandMessReponse(true,
                        $"Already up to date. {users.Count} user(s) in '{deptKey}' already have all {templateByMenu.Count} template permission(s).");

                _context.Permissions.AddRange(toAdd);
                await _context.SaveChangesAsync();

                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Sync Permission From Template", "Permission", Guid.Empty, deptKey,
                    new
                    {
                        Dept = deptKey,
                        TemplateCount = templateByMenu.Count,
                        UsersAffected = usersUpdated.Count,
                        PermissionsAdded = toAdd.Count,
                        Users = usersUpdated.ToList()
                    });

                return new BoolandMessReponse(true,
                    $"Synced '{deptKey}': added {toAdd.Count} permission(s) for {usersUpdated.Count}/{users.Count} user(s).");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Sync Fail: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
