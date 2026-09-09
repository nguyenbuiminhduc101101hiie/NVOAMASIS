using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_EInvoiceExportLog
    {
        [Key]
        public Guid EInvoiceExportLogId { get; set; }
        public string? InvoiceId { get; set; }
        public string? EInvoiceGuid { get; set; }
        public string? LookupCode { get; set; }
        public string? ViewUrl { get; set; }
        public string? SoHoaDonNoiBo { get; set; }
        public Guid? HblId { get; set; }
        public string? HblCode { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ExportType { get; set; }
        public int LineCount { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Continued { get; set; } = true;
        public string? PdfFileName { get; set; }
        public string? PdfFileContent { get; set; }
        public string? XmlFileName { get; set; }
        public string? XmlFileContent { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? PublishedBy { get; set; }
    }
}
