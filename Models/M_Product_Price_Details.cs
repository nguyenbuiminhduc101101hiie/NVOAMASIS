using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Product_Price_Details
    {

        [Key] 
             
        public Guid id { get; set; }
        public Guid? product_price_id { get; set; }
        public Guid? itemid { get; set; }
        public double? price { get; set; }
        public string? cur { get; set; }
        public string? note { get; set; }
        public string? UserUPdate { get; set; }
        public string? DateUpdate { get; set; }

        public DateTime? NgayBaoGia { get; set; }
        public bool? Approve { get; set; } = false;
        public string? CreateAt { get; set; }

        public string? UserCreate { get; set; }
    }
}
