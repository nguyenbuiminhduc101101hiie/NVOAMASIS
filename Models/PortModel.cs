using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class PortModel
    {
        [Key]
        public Guid PORT_ID { get; set; }
        public string? PORT_CODE { get; set; }
        public string? PORT { get; set; }
        public string? MARKETCODETS { get; set; }
        public string? MARKETCODESALE { get; set; }
        public string? TEL { get; set; }
        public string? FAX { get; set; }
        public string? ADDRESS { get; set; }
        public string? COUNTRY { get; set; }
        public DateTime? DATEEXP { get; set; }
        public string? IG { get; set; }
        public string? TRADECODE { get; set; }
        public string? OverW20 { get; set; }
        public string? OverW40 { get; set; }
        public bool? APPROVE { get; set; }
        public bool? CONTINUED { get; set; }
        public bool? EDITABLE { get; set; }
        public string? USERID { get; set; }
        public DateTime? UPDATETIME { get; set; }
        public bool? show { get; set; } = true;
        public string? dept { get; set; }
    }

    public class PortExcelPreviewRow
    {
        public int ExcelRow { get; set; }
        public PortModel Port { get; set; } = new();
        public string Status { get; set; } = PortExcelRowStatus.New;
        public string? StatusReason { get; set; }
        public bool Selected { get; set; }
        public bool CanImport => Status == PortExcelRowStatus.New;
    }

    public static class PortExcelRowStatus
    {
        public const string New = "New";
        public const string DuplicateDb = "DuplicateDb";
        public const string DuplicateFile = "DuplicateFile";
        public const string Invalid = "Invalid";
    }

    public class PortExcelParseResult
    {
        public string? ErrorMessage { get; set; }
        public List<PortExcelPreviewRow> Rows { get; set; } = new();
        public List<PortModel> ToImport { get; set; } = new();
        public int DuplicateInDb { get; set; }
        public int DuplicateInFile { get; set; }
        public int EmptySkipped { get; set; }
        public int InvalidCount { get; set; }
    }
}
