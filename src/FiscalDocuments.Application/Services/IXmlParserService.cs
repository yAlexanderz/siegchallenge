using FiscalDocuments.Application.Models;

namespace FiscalDocuments.Application.Services;

public interface IXmlParserService
{
    ParsedDocumentData Parse(string xmlContent);
}