using System;
using System.Collections.Generic;

namespace NVOAMASIS.Models
{
    public class FinancialReportSnapshot
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ReportCode { get; set; } = "BALANCE_SHEET";
        public int FiscalYear { get; set; }
        public int PeriodMonth { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalSource { get; set; }
        public decimal Difference { get; set; }

        public string Status { get; set; } = "DRAFT";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public string? Note { get; set; }

        public List<FinancialReportSnapshotLine> Lines { get; set; } = new();
    }
}
