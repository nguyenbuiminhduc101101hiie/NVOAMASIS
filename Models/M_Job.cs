using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NVOAMASIS.Models
{
    public class M_Job
    {
        [Key]
        public Guid JobID { get; set; }
        public string? FLC { get; set; }
        public string? JobNo { get; set; }
        public string? Loai { get; set; }
        public string? Loai_Nvo_Fre { get; set; }
        public string? statusJob { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Editable { get; set; }
        public bool? Continued { get; set; }
        public string? Userupdate { get; set; }
        public DateTime? Dateupdate { get; set; }
        public DateTime? Datecreate { get; set; }
        public string? Branch { get; set; }
        public string? Salecode { get; set; }
        public string? CompanyCode { get; set; }
        public string? BookingNo { get; set; }


        [JsonIgnore]
        public List<M_MBL>? MBLs { get; set; }
        public M_Job DeepCopy()
        {
            string json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<M_Job>(json);
        }
    }

}