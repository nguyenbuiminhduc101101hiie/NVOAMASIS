using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? SenderUserId { get; set; }
        public Guid? ReceiverUserId { get; set; } 
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }

}
