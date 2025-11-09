using MediatR;
using FiscalDocuments.Application.DTOs;

namespace FiscalDocuments.Application.Commands;

public class UploadDocumentCommand : IRequest<UploadDocumentResponse>
{
    public string XmlContent { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}