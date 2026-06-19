namespace NVOAMASIS.Models
{
    public sealed class BillLayoutFormCreateRequest
    {
        public string FormName { get; init; } = string.Empty;
        public BillLayoutFormKind FormKind { get; init; } = BillLayoutFormKind.Sea;
    }
}
