using FiscalDocuments.Domain.Enums;

namespace FiscalDocuments.Application.Models;

public class ParsedDocumentData
{
    public string DocumentKey { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal TotalValue { get; set; }
    public string IssuerName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientCnpj { get; set; } = string.Empty;
}