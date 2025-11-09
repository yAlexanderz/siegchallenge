using FiscalDocuments.Domain.Enums;

namespace FiscalDocuments.Domain.Events;

public class DocumentProcessedEvent
{
    public Guid DocumentId { get; set; }
    public string DocumentKey { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ProcessedAt { get; set; }
}