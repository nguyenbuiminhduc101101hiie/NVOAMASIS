using Microsoft.EntityFrameworkCore;

namespace NVOAMASIS.Models
{
    [Keyless]
    public class FilterCondition
    {
        public string PropertyName { get; set; } = string.Empty;
        public string Operator { get; set; } = "Contains";
        public string Valuestring { get; set; } = string.Empty;
        public Guid ValueGuid { get; set; } = Guid.Empty;
    }
    public class PropertyOption
    {
        public string PropertyName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

}
