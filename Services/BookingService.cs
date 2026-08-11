using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.Office.Interop.Excel;
using Stimulsoft.Report;
using Stimulsoft.Report.Blazor;
using System.Buffers;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using ZXing;

namespace NVOAMASIS.Services
{
    public class BookingService(AppDbContext _context, IWebHostEnvironment _env, IJSRuntime JSRuntime, AccountService asv)
    {
        public async Task<List<Booking>> GetListSBooking()
        {

            var Invoices = await _context.CONTAINEROUTBOUNDNOTIFY_sale.OrderByDescending(x => x.UpdateTime).ToListAsync();

            if (Invoices == null)
                return new List<Booking>();
            //Invoices.Insert(0, new Booking { BookingNo = "" });
            return Invoices;
        }
        public async Task<List<Booking>> GetListBooking_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.CONTAINEROUTBOUNDNOTIFY_sale.Where(_ => _.ContainerOutBoundNotifyId == id).ToListAsync();
            return Invoices;
        }

        public async Task<List<string>> GetList_From()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.UserList.Where(x => x.NickName != null)
                    .Select(x => x.NickName).Distinct().ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<List<PortModel>> GetList_PORT()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Port.OrderBy(x => x.PORT).ToListAsync();
                rs.Insert(0, new PortModel { PORT = "" });
                return rs;
            }
            catch (Exception ex)
            {
                return new List<PortModel>();
            }
           
        }
        public async Task<List<string>> GetList_PORT( string searchValue)
        {
            try 
            { 
            _context.ChangeTracker.Clear();
            var rs = await _context.Port
                .Where(x => x.PORT_CODE != null &&
                            (EF.Functions.Like(x.PORT_CODE, $"%{searchValue}%") ||
                             EF.Functions.Like(x.PORT, $"%{searchValue}%")))
                .Select(x => x.PORT +"-"+x.PORT_CODE)
                .Distinct()
                .ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<List<string>> GetList_Vessel()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CONTAINEROUTBOUNDNOTIFY_sale.Where(x => x.Vessel != null)
                    .Select(x => x.Vessel).Distinct().ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<List<string>> GetList_Voy(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.VesselSpace.Where(x => x.Voy != null)
                    .Select(x => x.Voy).Distinct().ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        


        public async Task<List<string>> GetList_Com(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Commondity.Where(x => x.Commondity != null)
                    .Select(x => x.Commondity).Distinct().ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        public async Task<List<string>> GetList_OPS(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.CONTAINEROUTBOUNDNOTIFY_sale.Where(x => x.OPSCode != null)
                    .Select(x => x.OPSCode).Distinct().ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<List<string>> GetlistCus_trucking(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Customer.Where(x => x.MainCode.Contains("Trucking"))
                    .Select(x => x.COMPANY).Distinct().ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<string>> GetList_Terminal(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Terminal.Where(x => x.TermiNalName != null)
                    .Select(x => x.TermiNalName).Distinct().ToListAsync();
               
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        public async Task<List<string>> GetList_Terminal_Codeha(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Terminal.Where(x => x.TermiNalName != null)
                    .Select(x => string.IsNullOrEmpty(x.Codeha)
                        ? x.TermiNalName!
                        : x.TermiNalName + "-" + x.Codeha)
                    .Distinct().ToListAsync();

                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        public async Task<List<string>> GetList_Port_2char(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Port
                    .Where(x => x.PORT_CODE != null)
                    .Select(x => x.PORT_CODE.Substring(3, 2)) // Bỏ 3 ký tự đầu, lấy 2 ký tự sau
                    .Distinct()
                    .ToListAsync();
                    rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }
        public async Task<List<string>> GetList_Port_2char_To(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Port
                    .Where(x => x.PORT_CODE != null)
                    .Select(x => x.PORT_CODE.Substring(3, 2)) // Bỏ 3 ký tự đầu, lấy 2 ký tự sau
                    .Distinct()
                    .ToListAsync();
                    rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        public async Task<List<string>> GetList_PortCode(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.Port
                    .Where(x => x.PORT_CODE != null)
                    .Select(x => x.PORT_CODE) // Bỏ 3 ký tự đầu, lấy 2 ký tự sau
                    .Distinct()
                    .ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        public async Task<List<string>> GetList_User(string searchValue)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var rs = await _context.UserList.Where(x => x.Name != null)
                    .Select(x => x.Name).Distinct().OrderBy(x => x).ToListAsync();
                rs.Insert(0, "");
                return rs!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null!;
            }
        }

        public async Task<int?> GetRefno_INV()
        {
            try
            {
                _context.ChangeTracker.Clear();
                var bkno =await _context.SGN_bkno
                                .Where(r => r.continued == true)
                                .OrderBy(r => r.autonumber) // Sắp xếp theo Autonumber tăng dần
                                .FirstOrDefaultAsync(); // Lấy dòng có giá trị Autonumber nhỏ nhất
             

                bkno.continued = false;
                bkno.userget = asv.GetAuth().Result.User.Identity.Name;
                bkno.timeget = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                await _context.SaveChangesAsync();
                return bkno.autonumber!;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<string>> UpdateOrCreateBBookingRequest(Booking IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.ContainerOutBoundNotifyId == null || IV.ContainerOutBoundNotifyId == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Booking Request Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Booking Request Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Booking Request Fail", "0"];
            }
        }

        public async Task<BoolandMessReponse> UpdateAnBookingRequest(Models.Booking BT)
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.CONTAINEROUTBOUNDNOTIFY_sale.Update(BT);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Update Booking Successfully");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Update Booking Fail with Error code:" + ex.Message);
            }
        }


        public async Task<Booking> GetDetailBookingFromID(Guid? id)
        {
            _context.ChangeTracker.Clear();

            var bk = await _context.CONTAINEROUTBOUNDNOTIFY_sale.Where(_ => _.ContainerOutBoundNotifyId == id).FirstOrDefaultAsync();
            return bk;
        }

        public async Task<BoolandMessReponse> DeleteBooking(Models.Booking IVM)
        {
            _context.ChangeTracker.Clear();
            try
            {
                if (IVM?.ContainerOutBoundNotifyId == null || IVM?.ContainerOutBoundNotifyId == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                // Xóa các dòng trong bảng InvoiceDetail có Invoice_No khớp với InvFobCmt_ID
                var invoiceDetails = _context.CONTAINEROUTBOUNDNOTIFY_sale
                                             .Where(detail => detail.ContainerOutBoundNotifyId == IVM.ContainerOutBoundNotifyId)
                                             .ToList();

                if (invoiceDetails.Any())
                {
                    _context.CONTAINEROUTBOUNDNOTIFY_sale.RemoveRange(invoiceDetails);
                }

                // Xóa bản ghi trong bảng INV_FOB_CMT
                _context?.CONTAINEROUTBOUNDNOTIFY_sale.Remove(IVM!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Deleted");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot delete with error code: " + ex.Message);

            }
        }

        public async Task<string?> GetEmailAsync(string? user)
        {
            try
            {
                _context.ChangeTracker.Clear();

                var rs = await _context.UserList
                                       .Where(x => x.Name == user)
                                       .Select(x => x.Email) // Chỉ lấy cột Email
                                       .FirstOrDefaultAsync(); // Lấy giá trị đầu tiên hoặc null

                return rs; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi: {ex.Message}");
                return null;
            }
        }

        

    }

}

