namespace FiscalDocuments.Application.DTOs;

public class UploadDocumentResponse
{
    public Guid DocumentId { get; set; }
    public string DocumentKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsNewDocument { get; set; }
}