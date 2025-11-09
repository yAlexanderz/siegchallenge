using MediatR;
using FiscalDocuments.Application.DTOs;

namespace FiscalDocuments.Application.Commands;

public class UpdateDocumentCommand : IRequest<FiscalDocumentDetailDto>
{
    public Guid DocumentId { get; set; }
    public string XmlContent { get; set; } = string.Empty;
}