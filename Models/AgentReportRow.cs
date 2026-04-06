using System;

namespace NVOAMASIS.Models
{
    // DTO for 7.4 Agent Report
    public class AgentReportRow
    {
        public int No { get; set; }
        public string? JobFileNo { get; set; }
        public string? POL { get; set; }
        public string? POD { get; set; }
        public string? Carrier { get; set; } // Agent name
        public string? HBL { get; set; }
        public string? MBL { get; set; }
        public double? Amount { get; set; }
        public string? Client { get; set; }
        public string? CustomerId { get; set; }
        public string? Agentname { get; set; }
    }
}
