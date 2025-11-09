using FiscalDocuments.Domain.Enums;

namespace FiscalDocuments.Application.DTOs;

public class FiscalDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentKey { get; set; } = string.Empty;
    public DocumentType Type { get; set; }
    public string TypeDescription { get; set; } = string.Empty;
    public string CnpjMasked { get; set; } = string.Empty; // CNPJ mascarado para segurança
    public string UF { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public decimal TotalValue { get; set; }
    public string IssuerName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientCnpjMasked { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}