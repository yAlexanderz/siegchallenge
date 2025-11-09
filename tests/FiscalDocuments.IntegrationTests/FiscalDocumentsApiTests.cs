using FiscalDocuments.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Text;

namespace FiscalDocuments.IntegrationTests;

[TestFixture]
public class FiscalDocumentsApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<FiscalDocumentsDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    //DbContext In-Memory
                    services.AddDbContext<FiscalDocumentsDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDatabase");
                    });

                    var sp = services.BuildServiceProvider();

                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<FiscalDocumentsDbContext>();

                    db.Database.EnsureCreated();
                });
            });
    }

    [SetUp]
    public void SetUp()
    {
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory?.Dispose();
    }

    [Test]
    public async Task GetDocuments_ShouldReturnPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/fiscaldocuments?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("data");
        content.Should().Contain("pageNumber");
    }

    [Test]
    public async Task UploadDocument_ValidXml_ShouldReturnSuccess()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<nfeProc xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <NFe>
        <infNFe Id=""NFe12345678901234567890123456789012345678901234"">
            <ide>
                <dhEmi>2025-01-15T10:30:00-03:00</dhEmi>
            </ide>
            <emit>
                <CNPJ>12345678901234</CNPJ>
                <xNome>Empresa Teste</xNome>
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
                    <vNF>1000.00</vNF>
                </ICMSTot>
            </total>
        </infNFe>
    </NFe>
</nfeProc>";

        var content = new MultipartFormDataContent();
        var xmlContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xml));
        xmlContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
        content.Add(xmlContent, "XmlFile", "test.xml");

        // Act
        var response = await _client.PostAsync("/api/v1/fiscaldocuments/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("documentId"); // Corrigido: era "id"
        responseContent.Should().Contain("documentKey");
        responseContent.Should().Contain("message");
    }

    [Test]
    public async Task UploadDocument_DuplicateXml_ShouldReturnIdempotentResponse()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<nfeProc xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <NFe>
        <infNFe Id=""NFe99999999999999999999999999999999999999999999"">
            <ide>
                <dhEmi>2025-01-15T10:30:00-03:00</dhEmi>
            </ide>
            <emit>
                <CNPJ>11111111111111</CNPJ>
                <xNome>Empresa Teste Duplicado</xNome>
                <enderEmit>
                    <UF>PE</UF>
                </enderEmit>
            </emit>
            <dest>
                <CNPJ>22222222222222</CNPJ>
                <xNome>Cliente Teste Duplicado</xNome>
            </dest>
            <total>
                <ICMSTot>
                    <vNF>5000.00</vNF>
                </ICMSTot>
            </total>
        </infNFe>
    </NFe>
</nfeProc>";

        var content1 = new MultipartFormDataContent();
        var xmlContent1 = new ByteArrayContent(Encoding.UTF8.GetBytes(xml));
        xmlContent1.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
        content1.Add(xmlContent1, "XmlFile", "duplicate.xml");

        var content2 = new MultipartFormDataContent();
        var xmlContent2 = new ByteArrayContent(Encoding.UTF8.GetBytes(xml));
        xmlContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
        content2.Add(xmlContent2, "XmlFile", "duplicate.xml");

        // Act - Upload primeira vez
        var response1 = await _client.PostAsync("/api/v1/fiscaldocuments/upload", content1);
        var responseContent1 = await response1.Content.ReadAsStringAsync();

        // Act - Upload segunda vez (mesmo XML)
        var response2 = await _client.PostAsync("/api/v1/fiscaldocuments/upload", content2);
        var responseContent2 = await response2.Content.ReadAsStringAsync();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        //idempotência
        responseContent1.Should().Contain("documentId");
        responseContent2.Should().Contain("documentId");

        responseContent2.Should().Contain("\"isNewDocument\":false");
        responseContent2.Should().Contain("idempotência");
    }
}