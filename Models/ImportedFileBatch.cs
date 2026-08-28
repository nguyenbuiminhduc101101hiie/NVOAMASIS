namespace NVOAMASIS.Models;

public sealed class ImportedFileBatch : IEquatable<ImportedFileBatch>
{
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public int RowCount { get; set; }
    public string? UserImport { get; set; }

    public string DisplayLabel => $"{FileName}  ·  {ImportDate:dd/MM/yyyy}";

    public bool Equals(ImportedFileBatch? other)
        => other is not null
           && string.Equals(FileName, other.FileName, StringComparison.OrdinalIgnoreCase)
           && ImportDate.Date == other.ImportDate.Date;

    public override bool Equals(object? obj) => Equals(obj as ImportedFileBatch);

    public override int GetHashCode()
        => HashCode.Combine(FileName.ToUpperInvariant(), ImportDate.Date);
}

public readonly record struct ImportedFileSource(
    string TableName,
    string FileColumn,
    string DateColumn,
    string UserColumn);
