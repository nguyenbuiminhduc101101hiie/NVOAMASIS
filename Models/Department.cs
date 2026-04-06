using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class Department
    {
        [Key]
        public Guid DepartmentId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public bool? Editable { get; set; }
        public string? UserId { get; set; }
        public string? UpdateTime { get; set; }
    }
}
