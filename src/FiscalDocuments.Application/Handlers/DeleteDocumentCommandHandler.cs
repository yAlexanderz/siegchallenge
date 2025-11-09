using MediatR;
using FiscalDocuments.Application.Commands;
using FiscalDocuments.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FiscalDocuments.Application.Handlers;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Unit>
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly ILogger<DeleteDocumentCommandHandler> _logger;

    public DeleteDocumentCommandHandler(
        IFiscalDocumentRepository repository,
        ILogger<DeleteDocumentCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null)
        {
            throw new KeyNotFoundException($"Documento com ID {request.DocumentId} não encontrado");
        }

        await _repository.DeleteAsync(request.DocumentId, cancellationToken);

        _logger.LogInformation("Documento excluído. DocumentId: {DocumentId}", request.DocumentId);

        return Unit.Value;
    }
}