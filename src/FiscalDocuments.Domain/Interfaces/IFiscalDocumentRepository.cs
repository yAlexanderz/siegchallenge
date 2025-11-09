using FiscalDocuments.Domain.Entities;

namespace FiscalDocuments.Domain.Interfaces;

public interface IFiscalDocumentRepository
{
    Task<FiscalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FiscalDocument?> GetByDocumentKeyAsync(string documentKey, CancellationToken cancellationToken = default);
    Task<FiscalDocument?> GetByXmlHashAsync(string xmlHash, CancellationToken cancellationToken = default);
    Task<(IEnumerable<FiscalDocument> Documents, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? cnpj = null,
        string? uf = null,
        CancellationToken cancellationToken = default);
    Task<FiscalDocument> AddAsync(FiscalDocument document, CancellationToken cancellationToken = default);
    Task UpdateAsync(FiscalDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}