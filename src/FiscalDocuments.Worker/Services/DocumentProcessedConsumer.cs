using System.Text;
using System.Text.Json;
using FiscalDocuments.Domain.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Polly;

namespace FiscalDocuments.Worker.Services;

/// <summary>
/// Consumidor RabbitMQ com resiliência, retry e backoff exponencial
/// </summary>
public class DocumentProcessedConsumer : BackgroundService
{
    private readonly ILogger<DocumentProcessedConsumer> _logger;
    private readonly string _connectionString;
    private IConnection? _connection;
    private IModel? _channel;
    private const string QueueName = "fiscal-documents-processed";
    private const int MaxRetryAttempts = 5;

    public DocumentProcessedConsumer(ILogger<DocumentProcessedConsumer> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetValue<string>("RabbitMQ:ConnectionString")
            ?? "amqp://guest:guest@localhost:5672";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Política de retry com backoff exponencial
        var retryPolicy = Policy
            .Handle<BrokerUnreachableException>()
            .Or<Exception>()
            .WaitAndRetryAsync(
                MaxRetryAttempts,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "Falha ao conectar ao RabbitMQ. Tentativa {RetryCount} de {MaxRetries}. Aguardando {TimeSpan}s",
                        retryCount, MaxRetryAttempts, timeSpan.TotalSeconds);
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            InitializeRabbitMQ();
            await Task.CompletedTask;
        });

        if (_channel == null)
        {
            _logger.LogError("Não foi possível estabelecer conexão com RabbitMQ após {MaxRetries} tentativas", MaxRetryAttempts);
            return;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            await ProcessMessageAsync(ea, stoppingToken);
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("Consumidor RabbitMQ iniciado e aguardando mensagens...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso");
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        var retryCount = 0;
        var maxRetries = 3;

        while (retryCount < maxRetries)
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var documentEvent = JsonSerializer.Deserialize<DocumentProcessedEvent>(message);

                if (documentEvent == null)
                {
                    _logger.LogWarning("Mensagem inválida recebida e será descartada");
                    _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation(
                    "Processando documento fiscal. ID: {DocumentId}, Tipo: {Type}, CNPJ: {Cnpj}, Valor: {Value:C}",
                    documentEvent.DocumentId,
                    documentEvent.Type,
                    MaskCnpj(documentEvent.Cnpj),
                    documentEvent.TotalValue);

                await GenerateDocumentSummaryAsync(documentEvent, cancellationToken);
                await IndexDocumentForSearchAsync(documentEvent, cancellationToken);

                // Confirmar processamento (ACK)
                _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                _logger.LogInformation("Documento processado com sucesso. ID: {DocumentId}", documentEvent.DocumentId);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex,
                    "Erro ao processar mensagem. Tentativa {RetryCount} de {MaxRetries}",
                    retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Falha definitiva ao processar mensagem após {MaxRetries} tentativas. Mensagem será rejeitada",
                        maxRetries);

                    //DLQ
                    _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    return;
                }

                //backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), cancellationToken);
            }
        }
    }

    private async Task GenerateDocumentSummaryAsync(DocumentProcessedEvent documentEvent, CancellationToken cancellationToken)
    {
        // Simular geração de resumo do documento
        var summary = new
        {
            DocumentId = documentEvent.DocumentId,
            Type = documentEvent.Type.ToString(),
            Summary = $"Documento {documentEvent.Type} processado. Emitente CNPJ {MaskCnpj(documentEvent.Cnpj)}, " +
                     $"UF {documentEvent.UF}, Valor Total: R$ {documentEvent.TotalValue:N2}",
            ProcessedAt = documentEvent.ProcessedAt
        };

        // Em produção, aqui salvaria em cache, índice de busca, etc.
        _logger.LogInformation("Resumo gerado: {@Summary}", summary);
        await Task.CompletedTask;
    }

    private async Task IndexDocumentForSearchAsync(DocumentProcessedEvent documentEvent, CancellationToken cancellationToken)
    {
        // Simular indexação para mecanismo de busca (ElasticSearch, Azure Search, etc.)
        var indexData = new
        {
            Id = documentEvent.DocumentId,
            DocumentKey = documentEvent.DocumentKey,
            Type = documentEvent.Type.ToString(),
            Cnpj = documentEvent.Cnpj,
            UF = documentEvent.UF,
            TotalValue = documentEvent.TotalValue,
            IssueDate = documentEvent.IssueDate,
            IndexedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Documento indexado para busca: {@IndexData}", indexData);
        await Task.CompletedTask;
    }

    private static string MaskCnpj(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj) || cnpj.Length < 8)
            return "***";

        return $"{cnpj.Substring(0, 2)}.***.***/{cnpj.Substring(cnpj.Length - 4)}";
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}