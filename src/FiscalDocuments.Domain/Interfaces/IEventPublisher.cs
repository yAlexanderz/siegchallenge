namespace FiscalDocuments.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishDocumentProcessedAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class;
}