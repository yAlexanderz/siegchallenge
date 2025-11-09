using FiscalDocuments.Domain.Entities;
using FiscalDocuments.Domain.Interfaces;
using FiscalDocuments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace FiscalDocuments.Infrastructure.Repositories;

public class FiscalDocumentRepository : IFiscalDocumentRepository
{
    private readonly FiscalDocumentsDbContext _context;

    public FiscalDocumentRepository(FiscalDocumentsDbContext context)
    {
        _context = context;
    }

    public async Task<FiscalDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalDocuments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<FiscalDocument?> GetByDocumentKeyAsync(string documentKey, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalDocuments
            .FirstOrDefaultAsync(d => d.DocumentKey == documentKey, cancellationToken);
    }

    public async Task<FiscalDocument?> GetByXmlHashAsync(string xmlHash, CancellationToken cancellationToken = default)
    {
        return await _context.FiscalDocuments
            .FirstOrDefaultAsync(d => d.XmlHash == xmlHash, cancellationToken);
    }

    public async Task<(IEnumerable<FiscalDocument> Documents, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? cnpj = null,
        string? uf = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FiscalDocuments.AsQueryable();

        // Aplicar filtros
        if (startDate.HasValue)
            query = query.Where(d => d.IssueDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(d => d.IssueDate <= endDate.Value);

        if (!string.IsNullOrEmpty(cnpj))
            query = query.Where(d => d.Cnpj == cnpj);

        if (!string.IsNullOrEmpty(uf))
            query = query.Where(d => d.UF == uf);

        var totalCount = await query.CountAsync(cancellationToken);

        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (documents, totalCount);
    }

    public async Task<FiscalDocument> AddAsync(FiscalDocument document, CancellationToken cancellationToken = default)
    {
        await _context.FiscalDocuments.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    public async Task UpdateAsync(FiscalDocument document, CancellationToken cancellationToken = default)
    {
        _context.FiscalDocuments.Update(document);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);
        if (document != null)
        {
            _context.FiscalDocuments.Remove(document);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}