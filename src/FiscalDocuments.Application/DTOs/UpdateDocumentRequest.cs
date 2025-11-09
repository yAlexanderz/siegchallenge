using Microsoft.AspNetCore.Http;

namespace FiscalDocuments.Application.DTOs;

public class UpdateDocumentRequest
{
    public IFormFile XmlFile { get; set; } = null!;
}