using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class CustomerCode
    {
        [Key]
        public Guid? CustomerCode_ID { get; set; }
        public string? CustomerCode_Ref { get; set; }
        public bool? Used { get; set; }
    }
}
