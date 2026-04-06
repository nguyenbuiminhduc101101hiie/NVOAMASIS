using NVOAMASIS.Models;

namespace NVOAMASIS.Interface
{
    public interface IWordDocumentService
    {
        Task<WordDocument> SaveDocumentAsync(string fileName, string content, string filePath, string? description = null);
        Task<IEnumerable<WordDocument>> GetAllActiveDocumentsAsync();
        Task<WordDocument?> GetDocumentByIdAsync(Guid id);
        Task<bool> DeactivateDocumentAsync(Guid id);
        Task<string> SaveFileToDiskAsync(IFormFile file);
        Task<string> ExtractTextFromWordDocument(Stream stream);
    }
}
