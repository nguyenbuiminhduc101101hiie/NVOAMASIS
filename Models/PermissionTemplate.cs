using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class PermissionTemplate
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? MenuId { get; set; }
        public string? MenuName { get; set; }
        public string? Dept { get; set; }
        public bool? canView { get; set; } = true;
        public bool? canAdd { get; set; } = true;
        public bool? canEdit { get; set; } = true;
        public bool? canDelete { get; set; } = true;
        public bool? canApprove { get; set; } = true;

    }
}
