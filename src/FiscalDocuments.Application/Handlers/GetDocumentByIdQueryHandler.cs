using MediatR;
using FiscalDocuments.Application.Queries;
using FiscalDocuments.Application.DTOs;
using FiscalDocuments.Application.Mappers;
using FiscalDocuments.Domain.Interfaces;

namespace FiscalDocuments.Application.Handlers;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, FiscalDocumentDetailDto?>
{
    private readonly IFiscalDocumentRepository _repository;

    public GetDocumentByIdQueryHandler(IFiscalDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<FiscalDocumentDetailDto?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
        return document != null ? FiscalDocumentMapper.ToDetailDto(document) : null;
    }
}