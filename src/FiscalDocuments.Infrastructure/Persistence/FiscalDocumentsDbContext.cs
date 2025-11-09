using FiscalDocuments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FiscalDocuments.Infrastructure.Persistence;

public class FiscalDocumentsDbContext : DbContext
{
    public FiscalDocumentsDbContext(DbContextOptions<FiscalDocumentsDbContext> options) : base(options)
    {}

    public DbSet<FiscalDocument> FiscalDocuments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FiscalDocument>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DocumentKey)
                .IsRequired()
                .HasMaxLength(44);

            entity.HasIndex(e => e.DocumentKey)
                .IsUnique();

            entity.HasIndex(e => e.XmlHash)
                .IsUnique();

            entity.Property(e => e.Cnpj)
                .IsRequired()
                .HasMaxLength(14);

            entity.Property(e => e.UF)
                .IsRequired()
                .HasMaxLength(2);

            entity.Property(e => e.TotalValue)
                .HasPrecision(18, 2);

            entity.Property(e => e.IssuerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.RecipientName)
                .HasMaxLength(200);

            entity.Property(e => e.RecipientCnpj)
                .HasMaxLength(14);

            entity.Property(e => e.XmlContent)
                .IsRequired();

            entity.HasIndex(e => e.IssueDate);
            entity.HasIndex(e => new { e.Cnpj, e.IssueDate });
            entity.HasIndex(e => new { e.UF, e.IssueDate });
        });
    }
}