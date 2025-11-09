namespace FiscalDocuments.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string ConnectionString { get; set; } = "amqp://guest:guest@localhost:5672";
}