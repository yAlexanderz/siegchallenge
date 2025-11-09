using MediatR;
using FiscalDocuments.Application.Commands;
using FiscalDocuments.Application.DTOs;
using FiscalDocuments.Application.Services;
using FiscalDocuments.Domain.Entities;
using FiscalDocuments.Domain.Interfaces;
using FiscalDocuments.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FiscalDocuments.Application.Handlers;

/// <summary>
/// Handler para processar upload de documentos fiscais com idempotência
/// </summary>
public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IFiscalDocumentRepository _repository;
    private readonly IXmlParserService _xmlParser;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IFiscalDocumentRepository repository,
        IXmlParserService xmlParser,
        IEventPublisher eventPublisher,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _repository = repository;
        _xmlParser = xmlParser;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Calcular hash do XML para garantir idempotência
        var xmlHash = CalculateHash(request.XmlContent);

        // Verificar se documento já existe pelo hash
        var existingByHash = await _repository.GetByXmlHashAsync(xmlHash, cancellationToken);
        if (existingByHash != null)
        {
            _logger.LogInformation("Documento duplicado detectado pelo hash. DocumentId: {DocumentId}", existingByHash.Id);
            return new UploadDocumentResponse
            {
                DocumentId = existingByHash.Id,
                DocumentKey = existingByHash.DocumentKey,
                Message = "Documento já processado anteriormente (idempotência garantida)",
                IsNewDocument = false
            };
        }

        var parsedData = _xmlParser.Parse(request.XmlContent);

        var existingByKey = await _repository.GetByDocumentKeyAsync(parsedData.DocumentKey, cancellationToken);
        if (existingByKey != null)
        {
            _logger.LogInformation("Documento com mesma chave encontrado. DocumentKey: {DocumentKey}", parsedData.DocumentKey);
            return new UploadDocumentResponse
            {
                DocumentId = existingByKey.Id,
                DocumentKey = existingByKey.DocumentKey,
                Message = "Documento com esta chave já existe",
                IsNewDocument = false
            };
        }

        var document = new FiscalDocument(
            parsedData.DocumentKey,
            parsedData.Type,
            request.XmlContent,
            parsedData.Cnpj,
            parsedData.UF,
            parsedData.IssueDate,
            parsedData.TotalValue,
            parsedData.IssuerName,
            parsedData.RecipientName,
            parsedData.RecipientCnpj,
            xmlHash
        );

        await _repository.AddAsync(document, cancellationToken);

        document.UpdateStatus(Domain.Enums.DocumentStatus.Processed);
        await _repository.UpdateAsync(document, cancellationToken);

        _logger.LogInformation("Documento fiscal criado com sucesso. DocumentId: {DocumentId}, CNPJ: {Cnpj}",
            document.Id, MaskCnpj(document.Cnpj));

        var documentEvent = new DocumentProcessedEvent
        {
            DocumentId = document.Id,
            DocumentKey = document.DocumentKey,
            Type = document.Type,
            Cnpj = document.Cnpj,
            UF = document.UF,
            TotalValue = document.TotalValue,
            IssueDate = document.IssueDate,
            ProcessedAt = DateTime.UtcNow
        };

        await _eventPublisher.PublishDocumentProcessedAsync(documentEvent, cancellationToken);

        return new UploadDocumentResponse
        {
            DocumentId = document.Id,
            DocumentKey = document.DocumentKey,
            Message = "Documento processado com sucesso",
            IsNewDocument = true
        };
    }

    private static string CalculateHash(string content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hashBytes);
    }

    private static string MaskCnpj(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj) || cnpj.Length < 8)
            return "***";

        return $"{cnpj.Substring(0, 2)}.***.***/{cnpj.Substring(cnpj.Length - 4)}";
    }
}