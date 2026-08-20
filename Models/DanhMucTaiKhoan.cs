using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class DanhMucTaiKhoan
    {
        [Key]
        public Guid Id { get; set; }
        public string? Taikhoan { get; set; }
        public string? Tentaikhoan { get; set; }
        public string? Accountname { get; set; }
        public string? Manguyente { get; set; }
        public bool? Approve { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public bool? Tinhdauky_Cuoiky { get; set; }
        public string? AccountType { get; set; }
        public bool? IsPosting { get; set; } = true;
        public bool? RequiresCustomer { get; set; } =false;
        public bool? RequiresSupplier { get; set; } = false;
        public bool? RequiresEmployee { get; set; } = false;
        public bool? RequiresShipment { get; set; } = false;
        public bool? IsActive { get; set; } = true;
        public string? Diengiai { get; set; }
    }

    public class AccountExcelPreviewRow
    {
        public int ExcelRow { get; set; }
        public DanhMucTaiKhoan Account { get; set; } = new();
        public string Status { get; set; } = AccountExcelRowStatus.New;
        public string? StatusReason { get; set; }
        public bool Selected { get; set; }
        public bool CanImport => Status == AccountExcelRowStatus.New;
    }

    public static class AccountExcelRowStatus
    {
        public const string New = "New";
        public const string DuplicateDb = "DuplicateDb";
        public const string DuplicateFile = "DuplicateFile";
        public const string Invalid = "Invalid";
    }

    public class AccountExcelParseResult
    {
        public string? ErrorMessage { get; set; }
        public List<AccountExcelPreviewRow> Rows { get; set; } = new();
        public List<DanhMucTaiKhoan> ToImport { get; set; } = new();
        public int DuplicateInDb { get; set; }
        public int DuplicateInFile { get; set; }
        public int EmptySkipped { get; set; }
        public int InvalidCount { get; set; }
    }
}
