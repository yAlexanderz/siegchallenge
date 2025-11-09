using NUnit.Framework;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using FiscalDocuments.Application.Commands;
using FiscalDocuments.Application.Handlers;
using FiscalDocuments.Application.Services;
using FiscalDocuments.Domain.Interfaces;
using FiscalDocuments.Domain.Entities;
using FiscalDocuments.Application.Models;
using FiscalDocuments.Domain.Enums;

namespace FiscalDocuments.UnitTests.Handlers;

[TestFixture]
public class UploadDocumentCommandHandlerTests
{
    private Mock<IFiscalDocumentRepository> _repositoryMock = null!;
    private Mock<IXmlParserService> _xmlParserMock = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private Mock<ILogger<UploadDocumentCommandHandler>> _loggerMock = null!;
    private UploadDocumentCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IFiscalDocumentRepository>();
        _xmlParserMock = new Mock<IXmlParserService>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<UploadDocumentCommandHandler>>();

        _handler = new UploadDocumentCommandHandler(
            _repositoryMock.Object,
            _xmlParserMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Handle_NewDocument_ShouldCreateSuccessfully()
    {
        // Arrange
        var command = new UploadDocumentCommand
        {
            XmlContent = "<nfeProc><infNFe Id=\"NFe12345678901234567890123456789012345678901234\"></infNFe></nfeProc>",
            FileName = "test.xml"
        };

        var parsedData = new ParsedDocumentData
        {
            DocumentKey = "12345678901234567890123456789012345678901234",
            Type = DocumentType.NFe,
            Cnpj = "12345678901234",
            UF = "PE",
            IssueDate = DateTime.UtcNow,
            TotalValue = 1000.50m,
            IssuerName = "Empresa Teste",
            RecipientName = "Cliente Teste",
            RecipientCnpj = "98765432109876"
        };

        _xmlParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(parsedData);
        _repositoryMock.Setup(x => x.GetByXmlHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FiscalDocument?)null);
        _repositoryMock.Setup(x => x.GetByDocumentKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FiscalDocument?)null);
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<FiscalDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FiscalDocument doc, CancellationToken ct) => doc);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsNewDocument.Should().BeTrue();
        result.DocumentKey.Should().Be(parsedData.DocumentKey);
        result.Message.Should().Contain("sucesso");

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<FiscalDocument>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(x => x.PublishDocumentProcessedAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DuplicateDocumentByHash_ShouldReturnExisting_Idempotence()
    {
        // Arrange
        var existingDocument = new FiscalDocument(
            "12345678901234567890123456789012345678901234",
            DocumentType.NFe,
            "<xml>content</xml>",
            "12345678901234",
            "PE",
            DateTime.UtcNow,
            1000m,
            "Empresa",
            "Cliente",
            "98765432109876",
            "hash123");

        var command = new UploadDocumentCommand
        {
            XmlContent = "<xml>content</xml>",
            FileName = "test.xml"
        };

        _repositoryMock.Setup(x => x.GetByXmlHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDocument);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsNewDocument.Should().BeFalse();
        result.Message.Should().Contain("idempotência");
        result.DocumentId.Should().Be(existingDocument.Id);

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<FiscalDocument>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisherMock.Verify(x => x.PublishDocumentProcessedAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_DuplicateDocumentByKey_ShouldReturnExisting()
    {
        // Arrange
        var existingDocument = new FiscalDocument(
            "12345678901234567890123456789012345678901234",
            DocumentType.NFe,
            "<xml>old</xml>",
            "12345678901234",
            "PE",
            DateTime.UtcNow,
            1000m,
            "Empresa",
            "Cliente",
            "98765432109876",
            "oldHash");

        var command = new UploadDocumentCommand
        {
            XmlContent = "<xml>new</xml>",
            FileName = "test.xml"
        };

        var parsedData = new ParsedDocumentData
        {
            DocumentKey = "12345678901234567890123456789012345678901234",
            Type = DocumentType.NFe,
            Cnpj = "12345678901234",
            UF = "PE",
            IssueDate = DateTime.UtcNow,
            TotalValue = 1000m,
            IssuerName = "Empresa",
            RecipientName = "Cliente",
            RecipientCnpj = "98765432109876"
        };

        _xmlParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(parsedData);
        _repositoryMock.Setup(x => x.GetByXmlHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FiscalDocument?)null);
        _repositoryMock.Setup(x => x.GetByDocumentKeyAsync(parsedData.DocumentKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingDocument);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsNewDocument.Should().BeFalse();
        result.DocumentKey.Should().Be(existingDocument.DocumentKey);

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<FiscalDocument>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}