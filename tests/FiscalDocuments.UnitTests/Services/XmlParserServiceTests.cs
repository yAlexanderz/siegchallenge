using FluentAssertions;
using FiscalDocuments.Application.Services;
using NUnit.Framework;

namespace FiscalDocuments.UnitTests.Services;

[TestFixture]
public class XmlParserServiceTests
{
    private XmlParserService _xmlParserService = null!;

    [SetUp]
    public void Setup()
    {
        _xmlParserService = new XmlParserService();
    }

    [Test]
    public void Parse_ValidNFeXml_ShouldParseSuccessfully()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<nfeProc xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <NFe>
        <infNFe Id=""NFe12345678901234567890123456789012345678901234"">
            <ide>
                <dhEmi>2025-01-15T10:30:00-03:00</dhEmi>
            </ide>
            <emit>
                <CNPJ>12345678901234</CNPJ>
                <xNome>Empresa Teste LTDA</xNome>
                <enderEmit>
                    <UF>PE</UF>
                </enderEmit>
            </emit>
            <dest>
                <CNPJ>98765432109876</CNPJ>
                <xNome>Cliente Teste</xNome>
            </dest>
            <total>
                <ICMSTot>
                    <vNF>1500.75</vNF>
                </ICMSTot>
            </total>
        </infNFe>
    </NFe>
</nfeProc>";

        // Act
        var result = _xmlParserService.Parse(xmlContent);

        // Assert
        result.Should().NotBeNull();
        result.DocumentKey.Should().Be("12345678901234567890123456789012345678901234");
        result.Cnpj.Should().Be("12345678901234");
        result.IssuerName.Should().Be("Empresa Teste LTDA");
        result.RecipientCnpj.Should().Be("98765432109876");
        result.RecipientName.Should().Be("Cliente Teste");
        result.IssueDate.Should().Be(DateTime.Parse("2025-01-15T10:30:00-03:00"));
        result.TotalValue.Should().Be(1500.75m);
        result.UF.Should().Be("PE");
    }

    [Test]
    public void Parse_InvalidXml_ShouldThrowException()
    {
        // Arrange
        var xmlContent = "<invalid>xml";

        // Act & Assert
        Assert.Throws<System.Xml.XmlException>(() => _xmlParserService.Parse(xmlContent));
    }

    [Test]
    public void Parse_EmptyXml_ShouldThrowException()
    {
        // Arrange
        var xmlContent = "";

        // Act & Assert
        Assert.Throws<System.Xml.XmlException>(() => _xmlParserService.Parse(xmlContent));
    }
}