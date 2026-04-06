using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class Permission_M
    {
        [Key] public Guid? PermissionId { get; set; }
        public Guid? MenuId { get; set; }
        public string? MenuName { get; set; }

        public string? UserName { get; set; }
        public bool? See {  get; set; }
        public bool? Edit {  get; set; }
        public bool? Del {  get; set; }
        public bool? Approve {  get; set; }
        public bool? Add {  get; set; }
    }
}
