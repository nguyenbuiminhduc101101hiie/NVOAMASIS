namespace NVOAMASIS.Models
{
    public class M_Duyet_DNTT
    {
        public Guid Id { get; set; }
        public Guid? IdDntt { get; set; }
        public bool? Approve { get; set; }
        public DateTime? Ngayduyet { get; set; }
        public string? Userduyet { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public string? So {  get; set; }
        public double? Thanhtien { get; set; }
        public string? Cur { get; set; }
        public string? Kinhgui { get; set; }
        public string? Bophan { get; set; }
        public int? SoLanGuiEmail { get; set; } = 0;
        public string? Note {  get; set; }  
        public string? EmailUserDeNghi { get; set; }
    }
}
