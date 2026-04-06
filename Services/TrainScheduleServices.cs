using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Interface;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Internal;
using static MudBlazor.CategoryTypes;
using static Stimulsoft.Report.StiRecentConnections;

namespace NVOAMASIS.Services
{
    public class TrainScheduleServices(AppDbContext _context,AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<TrainSchedule>> GetListStrainSchedule()
        {
  
            var Invoices = await _context.TERMINALDEPARTURE.ToListAsync();
            if (Invoices == null)
                return new List<TrainSchedule>();
            return Invoices;
        }
        public async Task<List<TrainSchedule>> GetListStrainSchedule_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.TERMINALDEPARTURE.Where(_ => _.TerminalDeparture_ID == id).ToListAsync();
            return Invoices;
        }
        public async Task<TrainSchedule> GetDetailtrainscheduleFromID(Guid? id)
        {
            _context.ChangeTracker.Clear();

            var Train = await _context.TERMINALDEPARTURE.Where(_ => _.TerminalDeparture_ID == id).FirstOrDefaultAsync();
            return Train;
        }

        public async Task<BoolandMessReponse> DeleteTrainSchedule(Models.TrainSchedule IVM)
        {
            _context.ChangeTracker.Clear();
            try
            {
                if (IVM?.TerminalDeparture_ID == null || IVM?.TerminalDeparture_ID == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");
                // Xóa các dòng trong bảng InvoiceDetail có Invoice_No khớp với InvFobCmt_ID
                var invoiceDetails = _context.TERMINALDEPARTURE
                                             .Where(detail => detail.TerminalDeparture_ID == IVM.TerminalDeparture_ID)
                                             .ToList();

                if (invoiceDetails.Any())
                {
                    _context.TERMINALDEPARTURE.RemoveRange(invoiceDetails);
                }

                // Xóa bản ghi trong bảng INV_FOB_CMT
                _context?.TERMINALDEPARTURE.Remove(IVM!);
                await _context?.SaveChangesAsync()!;
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                await HistoryLogService.LogAsync(usr, "Delete Train Schedule", "TrainSchedule", IVM.TerminalDeparture_ID, IVM.Code, new { OldData = IVM });
              
                return new BoolandMessReponse(true, "Deleted");

            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot delete with error code: " + ex.Message);

            }
        }

        public async Task<List<string>> UpdateOrCreateTrainSchedule(TrainSchedule IV)
        {
            try
            {
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                _context.ChangeTracker.Clear();
                if (IV.TerminalDeparture_ID == null || IV.TerminalDeparture_ID == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
    
                    return ["Create new Train Schedule Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
               

                    return ["Update Train Schedule Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Train Schedule Fail", "0"];
            }
        }

        public async Task<BoolandMessReponse> UpdateAnTrainSchedule(Models.TrainSchedule IV, TrainSchedule IV_old)
        {
            try
            {
                string usr = asv.GetAuth().Result.User.Identity!.Name!;
                _context.ChangeTracker.Clear();
                _context.TERMINALDEPARTURE.Update(IV);
                await _context.SaveChangesAsync();
                await HistoryLogService.LogAsync(usr, "Update Train Schedule", "TrainSchedule", IV.TerminalDeparture_ID, IV.Code, new { OldData = IV_old, NewData = IV });
                return new BoolandMessReponse(true, "Update Train Schedule Successfully");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Update Train Schedule Fail with Error code:" + ex.Message);
            }
        }

    }

}
    
