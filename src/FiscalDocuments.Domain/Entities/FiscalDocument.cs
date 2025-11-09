using FiscalDocuments.Domain.Common;
using FiscalDocuments.Domain.Enums;
using System.Xml.Linq;

namespace FiscalDocuments.Domain.Entities;

/// <summary>
/// Entidade que representa um documento fiscal (NFe, CTe, NFSe)
/// </summary>
public class FiscalDocument : BaseEntity
{
    public string DocumentKey { get; private set; } // Chave de acesso única do documento
    public DocumentType Type { get; private set; }
    public string XmlContent { get; private set; }
    public string Cnpj { get; private set; }
    public string UF { get; private set; }
    public DateTime IssueDate { get; private set; }
    public decimal TotalValue { get; private set; }
    public string IssuerName { get; private set; }
    public string RecipientName { get; private set; }
    public string RecipientCnpj { get; private set; }
    public DocumentStatus Status { get; private set; }
    public string? ProcessingNotes { get; private set; }

    // Para garantir idempotência
    public string XmlHash { get; private set; }

    protected FiscalDocument() { } // EF Core

    public FiscalDocument(
        string documentKey,
        DocumentType type,
        string xmlContent,
        string cnpj,
        string uf,
        DateTime issueDate,
        decimal totalValue,
        string issuerName,
        string recipientName,
        string recipientCnpj,
        string xmlHash)
    {
        DocumentKey = documentKey;
        Type = type;
        XmlContent = xmlContent;
        Cnpj = cnpj;
        UF = uf;
        IssueDate = issueDate;
        TotalValue = totalValue;
        IssuerName = issuerName;
        RecipientName = recipientName;
        RecipientCnpj = recipientCnpj;
        XmlHash = xmlHash;
        Status = DocumentStatus.Received;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(DocumentStatus newStatus, string? notes = null)
    {
        Status = newStatus;
        ProcessingNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string xmlContent, decimal totalValue, string xmlHash)
    {
        XmlContent = xmlContent;
        TotalValue = totalValue;
        XmlHash = xmlHash;
        UpdatedAt = DateTime.UtcNow;
    }
}