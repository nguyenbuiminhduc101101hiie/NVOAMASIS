namespace NVOAMASIS.Models
{
    /// <summary>Metadata đính kèm (không chứa byte) — hydrate từ DB cho grid/menu, NotMapped trên M_SI.</summary>
    public sealed class M_SI_AttachmentRef
    {
        public Guid AttachmentId { get; set; }
        public string FileName { get; set; } = "";
    }
}
