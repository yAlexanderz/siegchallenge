namespace FiscalDocuments.Application.DTOs;

public class FiscalDocumentDetailDto : FiscalDocumentDto
{
    public string XmlContent { get; set; } = string.Empty;
    public string? ProcessingNotes { get; set; }
}