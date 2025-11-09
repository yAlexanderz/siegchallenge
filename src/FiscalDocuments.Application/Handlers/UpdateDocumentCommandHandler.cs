using MediatR;
using FiscalDocuments.Application.Commands;
using FiscalDocuments.Application.DTOs;
using FiscalDocuments.Application.Services;
using FiscalDocuments.Application.Mappers;
using FiscalDocuments.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FiscalDocuments.Application.Handlers;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, FiscalDocumentDetailDto>
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly IXmlParserService _xmlParser;
    private readonly ILogger<UpdateDocumentCommandHandler> _logger;

    public UpdateDocumentCommandHandler(
        IFiscalDocumentRepository repository,
        IXmlParserService xmlParser,
        ILogger<UpdateDocumentCommandHandler> logger)
    {
        _repository = repository;
        _xmlParser = xmlParser;
        _logger = logger;
    }

    public async Task<FiscalDocumentDetailDto> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document == null)
        {
            throw new KeyNotFoundException($"Documento com ID {request.DocumentId} não encontrado");
        }

        var parsedData = _xmlParser.Parse(request.XmlContent);
        var xmlHash = CalculateHash(request.XmlContent);

        document.Update(request.XmlContent, parsedData.TotalValue, xmlHash);
        await _repository.UpdateAsync(document, cancellationToken);

        _logger.LogInformation("Documento atualizado. DocumentId: {DocumentId}", document.Id);

        return FiscalDocumentMapper.ToDetailDto(document);
    }

    private static string CalculateHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashBytes);
    }
}