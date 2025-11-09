using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Net.Http.Headers;
using System.Text;

namespace FiscalDocuments.LoadTests;

public class UploadLoadTest
{
    public static void Run()
    {
        var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<nfeProc xmlns=""http://www.portalfiscal.inf.br/nfe"">
    <NFe>
        <infNFe Id=""NFe{0}"">
            <ide>
                <dhEmi>2025-01-15T10:30:00-03:00</dhEmi>
            </ide>
            <emit>
                <CNPJ>12345678901234</CNPJ>
                <xNome>Empresa Load Test LTDA</xNome>
                <enderEmit>
                    <UF>PE</UF>
                </enderEmit>
            </emit>
            <dest>
                <CNPJ>98765432109876</CNPJ>
                <xNome>Cliente Load Test</xNome>
            </dest>
            <total>
                <ICMSTot>
                    <vNF>5000.00</vNF>
                </ICMSTot>
            </total>
        </infNFe>
    </NFe>
</nfeProc>";

        var httpFactory = Http.CreateDefaultClient();

        var scenario = Scenario.Create("upload_fiscal_documents", async context =>
        {
            // Gerar chave única com 44 caracteres
            var uniqueKey = GenerateUniqueKey();
            var xml = string.Format(xmlContent, uniqueKey);

            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(xml));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");
            content.Add(fileContent, "XmlFile", $"load-test-{uniqueKey}.xml");

            var request = Http.CreateRequest("POST", "http://[::1]:5000/api/v1/fiscaldocuments/upload")
                .WithBody(content);

            var response = await Http.Send(httpFactory, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
    }

    private static string GenerateUniqueKey()
    {
        var random = new Random();
        var key = new StringBuilder(44);
        
        for (int i = 0; i < 44; i++)
        {
            key.Append(random.Next(0, 10));
        }
        
        return key.ToString();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Teste de Carga - Upload de Documentos Fiscais ===");
        Console.WriteLine("Pressione ENTER para iniciar o teste...");
        Console.ReadLine();

        UploadLoadTest.Run();

        Console.WriteLine("\nTeste concluído! Pressione ENTER para sair...");
        Console.ReadLine();
    }
}