using System;

namespace NVOAMASIS.Models
{
    public class FinancialReportSnapshotLine
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SnapshotId { get; set; }

        public string ReportCode { get; set; } = "BALANCE_SHEET";
        public string LineCode { get; set; } = "";
        public string? ParentLineCode { get; set; }
        public string LineName { get; set; } = "";

        public int SortOrder { get; set; }
        public int LevelNo { get; set; }

        public bool IsBold { get; set; }
        public bool IsTotalLine { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public FinancialReportSnapshot? Snapshot { get; set; }
    }
}
