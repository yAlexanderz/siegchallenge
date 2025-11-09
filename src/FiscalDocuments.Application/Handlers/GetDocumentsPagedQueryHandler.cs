using MediatR;
using FiscalDocuments.Application.Queries;
using FiscalDocuments.Application.DTOs;
using FiscalDocuments.Application.Mappers;
using FiscalDocuments.Domain.Interfaces;

namespace FiscalDocuments.Application.Handlers;

public class GetDocumentsPagedQueryHandler : IRequestHandler<GetDocumentsPagedQuery, PagedResponse<FiscalDocumentDto>>
{
    private readonly IFiscalDocumentRepository _repository;

    public GetDocumentsPagedQueryHandler(IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<FiscalDocumentDto>> Handle(GetDocumentsPagedQuery request, CancellationToken cancellationToken)
    {
        var (documents, totalCount) = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.StartDate,
            request.EndDate,
            request.Cnpj,
            request.UF,
            cancellationToken);

        return new PagedResponse<FiscalDocumentDto>
        {
            Data = documents.Select(FiscalDocumentMapper.ToDto),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}