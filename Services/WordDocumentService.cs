using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Interface;
using NVOAMASIS.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace NVOAMASIS.Services
{
    public class WordDocumentService : IWordDocumentService
    {
        //private readonly AppDbContext _context;
        private readonly AppDbContext _contextchat;
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadPath;

        public WordDocumentService(AppDbContext context, AppDbContext contextchat ,IWebHostEnvironment environment)
        {
            
            _contextchat = contextchat;
            _environment = environment;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "word-documents");
            
            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<WordDocument> SaveDocumentAsync(string fileName, string content, string filePath, string? description = null)
        {
            var document = new WordDocument
            {
                FileName = fileName,
                Content = content,
                FilePath = filePath,
                Description = description
            };

            _contextchat.WordDocuments.Add(document);
            await _contextchat.SaveChangesAsync();
            return document;
        }

        public async Task<IEnumerable<WordDocument>> GetAllActiveDocumentsAsync()
        {
            return await _contextchat.WordDocuments
                .Where(d => d.IsActive)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<WordDocument?> GetDocumentByIdAsync(Guid id)
        {
            return await _contextchat.WordDocuments
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);
        }

        public async Task<bool> DeactivateDocumentAsync(Guid id)
        {
            var document = await _contextchat.WordDocuments
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (document == null)
                return false;

            document.IsActive = false;
            await _contextchat.SaveChangesAsync();

            // Optionally delete physical file
            try
            {
                if (File.Exists(document.FilePath))
                {
                    File.Delete(document.FilePath);
                }
            }
            catch
            {
                // Log error but don't fail the operation
            }

            return true;
        }

        public async Task<string> SaveFileToDiskAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public async Task<string> ExtractTextFromWordDocument(Stream stream)
        {
            try
            {
                // Copy stream to memory stream to avoid position issues
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                using var wordDocument = WordprocessingDocument.Open(memoryStream, false);
                var body = wordDocument.MainDocumentPart?.Document?.Body;
                
                if (body == null)
                {
                    Console.WriteLine("WordDocumentService: Document body is null");
                    return await TryAlternativeExtraction(memoryStream);
                }

                var text = new System.Text.StringBuilder();
                
                // Method 1: Get all text elements from the document with proper formatting
                var paragraphs = body.Elements<Paragraph>();
                
                foreach (var paragraph in paragraphs)
                {
                    var paragraphText = paragraph.InnerText;
                    if (!string.IsNullOrWhiteSpace(paragraphText))
                    {
                        text.AppendLine(paragraphText.Trim());
                    }
                }

                var result = text.ToString();
                Console.WriteLine($"WordDocumentService: Method 1 - Extracted text length: {result.Length}");
                
                // If no text found, try alternative method
                if (string.IsNullOrWhiteSpace(result))
                {
                    Console.WriteLine("WordDocumentService: Method 1 failed, trying alternative method");
                    return await TryAlternativeExtraction(memoryStream);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WordDocumentService: Error extracting text: {ex.Message}");
                Console.WriteLine($"WordDocumentService: Stack trace: {ex.StackTrace}");
                return string.Empty;
            }
        }

        private async Task<string> TryAlternativeExtraction(Stream stream)
        {
            try
            {
                // Ensure stream is at beginning
                stream.Position = 0;
                using var wordDocument = WordprocessingDocument.Open(stream, false);
                var body = wordDocument.MainDocumentPart?.Document?.Body;
                
                if (body == null) return string.Empty;

                var text = new System.Text.StringBuilder();
                
                // Method 2: Try getting text from paragraphs
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    var paragraphText = paragraph.InnerText;
                    if (!string.IsNullOrWhiteSpace(paragraphText))
                    {
                        text.AppendLine(paragraphText);
                    }
                }

                var result = text.ToString();
                Console.WriteLine($"WordDocumentService: Method 2 - Extracted text length: {result.Length}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WordDocumentService: Alternative extraction failed: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
