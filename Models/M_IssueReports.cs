using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_IssueReports
    {
        [Key]
        public Guid IssueId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? StepsToReproduce { get; set; }
        public string? ExpectedResult { get; set; }
        public string? ActualResult { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
        public string? Reporter { get; set; }
        public string? AssignedTo { get; set; }
        public string? ReportDate { get; set; }
        public DateTime? ResolveDate { get; set; }
        public string? ScreenshotUrl { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public string? UserHandle { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Continued { get; set; } = true;
        public bool? Editable { get; set; } = true;

        public string? No {  get; set; }    

    }
}
