using Microsoft.AspNetCore.Http;

namespace FiscalDocuments.Application.DTOs;

public class UploadDocumentRequest
{
    public IFormFile XmlFile { get; set; } = null!;
}