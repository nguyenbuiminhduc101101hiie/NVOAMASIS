using Microsoft.EntityFrameworkCore;

namespace NVOAMASIS.Models
{
    [Keyless]
    public class ListRef
    {
        public string? gFLC { get; set; }
        public string? REF { get; set; }
        public string? MBL { get; set; }
        public int? TotalHBL { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
