using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class IssueReportServices(AppDbContext _context, IWebHostEnvironment _env, AccountService asv, HistoryLogService HistoryLogService)
    {
        public async Task<List<M_IssueReports>> Get_IssueReport()
        {
            try
            {

                _context.ChangeTracker.Clear();

                List<M_IssueReports> rs = new();

                rs = await _context.IssueReports
                     .Where(x => x.Continued == true)
                    .OrderBy(x => x.ReportDate).ToListAsync();


                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_IssueReports>();
            }
        }

        public async Task<BoolandMessReponse> Delete_IssueReport(M_IssueReports c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.IssueId == null || c?.IssueId == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.IssueReports.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Issue Reports Enter with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateCreate_IssueReport(M_IssueReports IV)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (IV.IssueId == null || IV.IssueId == Guid.Empty)
                {
                    _context.ChangeTracker.Clear();
                    _context.Add(IV);
                    await _context.SaveChangesAsync();
                    return ["Create new Issue Report Successfully", "1"];
                }
                else
                {
                    _context.ChangeTracker.Clear();

                    _context.Update(IV);
                    await _context.SaveChangesAsync();
                    return ["Update Issue Report Successfully", "1"];
                }
            }
            catch
            {
                return ["Save Issue Report Fail", "0"];
            }
        }

        public async Task<List<M_IssueReports>> Get_IssueReport_create(Guid? id)
        {
            _context.ChangeTracker.Clear();
            var Invoices = await _context.IssueReports.Where(_ => _.IssueId == id).ToListAsync();
            return Invoices;
        }
    }
}
