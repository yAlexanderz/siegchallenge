using System.Globalization;
using System.Xml.Linq;
using FiscalDocuments.Application.Models;
using FiscalDocuments.Domain.Enums;

namespace FiscalDocuments.Application.Services;

/// <summary>
/// Serviço para fazer parse de XMLs fiscais (NFe, CTe, NFSe)
/// </summary>
public class XmlParserService : IXmlParserService
{
    public ParsedDocumentData Parse(string xmlContent)
    {
        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root;

        if (root == null)
            throw new InvalidOperationException("XML inválido");

        var docType = DetectDocumentType(root);

        return docType switch
        {
            DocumentType.NFe => ParseNFe(root),
            DocumentType.CTe => ParseCTe(root),
            DocumentType.NFSe => ParseNFSe(root),
            _ => throw new NotSupportedException($"Tipo de documento {docType} não suportado")
        };
    }

    private DocumentType DetectDocumentType(XElement root)
    {
        if (root.Name.LocalName.Contains("nfeProc") || root.Name.LocalName.Contains("NFe"))
            return DocumentType.NFe;
        if (root.Name.LocalName.Contains("cteProc") || root.Name.LocalName.Contains("CTe"))
            return DocumentType.CTe;
        if (root.Name.LocalName.Contains("nfse") || root.Name.LocalName.Contains("NFSe"))
            return DocumentType.NFSe;

        throw new InvalidOperationException("Tipo de documento fiscal não identificado");
    }

    private ParsedDocumentData ParseNFe(XElement root)
    {
        XNamespace ns = root.GetDefaultNamespace();

        var infNFe = root.Descendants(ns + "infNFe").FirstOrDefault();
        if (infNFe == null)
            throw new InvalidOperationException("Estrutura NFe inválida");

        var ide = infNFe.Element(ns + "ide");
        var emit = infNFe.Element(ns + "emit");
        var dest = infNFe.Element(ns + "dest");
        var total = infNFe.Element(ns + "total")?.Element(ns + "ICMSTot");

        var chave = infNFe.Attribute("Id")?.Value?.Replace("NFe", "") ?? "";

        return new ParsedDocumentData
        {
            DocumentKey = chave,
            Type = DocumentType.NFe,
            Cnpj = emit?.Element(ns + "CNPJ")?.Value ?? "",
            UF = emit?.Element(ns + "enderEmit")?.Element(ns + "UF")?.Value ?? "",
            IssueDate = DateTime.Parse(ide?.Element(ns + "dhEmi")?.Value ?? DateTime.UtcNow.ToString()),
            TotalValue = decimal.Parse(total?.Element(ns + "vNF")?.Value ?? "0", CultureInfo.InvariantCulture),
            IssuerName = emit?.Element(ns + "xNome")?.Value ?? "",
            RecipientName = dest?.Element(ns + "xNome")?.Value ?? "",
            RecipientCnpj = dest?.Element(ns + "CNPJ")?.Value ?? dest?.Element(ns + "CPF")?.Value ?? ""
        };
    }

    private ParsedDocumentData ParseCTe(XElement root)
    {
        XNamespace ns = root.GetDefaultNamespace();

        var infCte = root.Descendants(ns + "infCte").FirstOrDefault();
        if (infCte == null)
            throw new InvalidOperationException("Estrutura CTe inválida");

        var ide = infCte.Element(ns + "ide");
        var emit = infCte.Element(ns + "emit");
        var dest = infCte.Element(ns + "dest");
        var vPrest = infCte.Element(ns + "vPrest");

        var chave = infCte.Attribute("Id")?.Value?.Replace("CTe", "") ?? "";

        return new ParsedDocumentData
        {
            DocumentKey = chave,
            Type = DocumentType.CTe,
            Cnpj = emit?.Element(ns + "CNPJ")?.Value ?? "",
            UF = emit?.Element(ns + "enderEmit")?.Element(ns + "UF")?.Value ?? "",
            IssueDate = DateTime.Parse(ide?.Element(ns + "dhEmi")?.Value ?? DateTime.UtcNow.ToString()),
            TotalValue = decimal.Parse(vPrest?.Element(ns + "vTPrest")?.Value ?? "0", CultureInfo.InvariantCulture),
            IssuerName = emit?.Element(ns + "xNome")?.Value ?? "",
            RecipientName = dest?.Element(ns + "xNome")?.Value ?? "",
            RecipientCnpj = dest?.Element(ns + "CNPJ")?.Value ?? ""
        };
    }

    private ParsedDocumentData ParseNFSe(XElement root)
    {
        // Implementação simplificada para NFSe (varia muito por município)
        XNamespace ns = root.GetDefaultNamespace();

        var prestador = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Prestador");
        var tomador = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Tomador");
        var valores = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Valores");

        return new ParsedDocumentData
        {
            DocumentKey = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "Numero")?.Value ?? Guid.NewGuid().ToString(),
            Type = DocumentType.NFSe,
            Cnpj = prestador?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Cnpj")?.Value ?? "",
            UF = prestador?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Uf")?.Value ?? "",
            IssueDate = DateTime.Parse(root.Descendants().FirstOrDefault(e => e.Name.LocalName == "DataEmissao")?.Value ?? DateTime.UtcNow.ToString()),
            TotalValue = decimal.Parse(valores?.Descendants().FirstOrDefault(e => e.Name.LocalName == "ValorServicos")?.Value ?? "0", CultureInfo.InvariantCulture),
            IssuerName = prestador?.Descendants().FirstOrDefault(e => e.Name.LocalName == "RazaoSocial")?.Value ?? "",
            RecipientName = tomador?.Descendants().FirstOrDefault(e => e.Name.LocalName == "RazaoSocial")?.Value ?? "",
            RecipientCnpj = tomador?.Descendants().FirstOrDefault(e => e.Name.LocalName == "Cnpj")?.Value ?? ""
        };
    }
}