using MediatR;
using FiscalDocuments.Application.DTOs;

namespace FiscalDocuments.Application.Queries;

public class GetDocumentByIdQuery : IRequest<FiscalDocumentDetailDto?>
{
    public Guid DocumentId { get; set; }
}