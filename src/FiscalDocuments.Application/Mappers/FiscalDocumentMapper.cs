using FiscalDocuments.Application.DTOs;
using FiscalDocuments.Domain.Entities;

namespace FiscalDocuments.Application.Mappers;

public static class FiscalDocumentMapper
{
    public static FiscalDocumentDto ToDto(FiscalDocument document)
    {
        return new FiscalDocumentDto
        {
            Id = document.Id,
            DocumentKey = document.DocumentKey,
            Type = document.Type,
            TypeDescription = document.Type.ToString(),
            CnpjMasked = MaskCnpj(document.Cnpj),
            UF = document.UF,
            IssueDate = document.IssueDate,
            TotalValue = document.TotalValue,
            IssuerName = document.IssuerName,
            RecipientName = document.RecipientName,
            RecipientCnpjMasked = MaskCnpj(document.RecipientCnpj),
            Status = document.Status,
            StatusDescription = document.Status.ToString(),
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    public static FiscalDocumentDetailDto ToDetailDto(FiscalDocument document)
    {
        return new FiscalDocumentDetailDto
        {
            Id = document.Id,
            DocumentKey = document.DocumentKey,
            Type = document.Type,
            TypeDescription = document.Type.ToString(),
            CnpjMasked = MaskCnpj(document.Cnpj),
            UF = document.UF,
            IssueDate = document.IssueDate,
            TotalValue = document.TotalValue,
            IssuerName = document.IssuerName,
            RecipientName = document.RecipientName,
            RecipientCnpjMasked = MaskCnpj(document.RecipientCnpj),
            Status = document.Status,
            StatusDescription = document.Status.ToString(),
            XmlContent = document.XmlContent,
            ProcessingNotes = document.ProcessingNotes,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }

    private static string MaskCnpj(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj) || cnpj.Length < 8)
            return "***";

        // Mascara CNPJ
        return $"{cnpj.Substring(0, 2)}.***.***/{cnpj.Substring(cnpj.Length - 4)}";
    }
}