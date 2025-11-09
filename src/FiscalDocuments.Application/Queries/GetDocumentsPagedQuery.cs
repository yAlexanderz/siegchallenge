using MediatR;
using FiscalDocuments.Application.DTOs;

namespace FiscalDocuments.Application.Queries;

public class GetDocumentsPagedQuery : IRequest<PagedResponse<FiscalDocumentDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Cnpj { get; set; }
    public string? UF { get; set; }
}