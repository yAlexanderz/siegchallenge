using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocuments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FiscalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentKey = table.Column<string>(type: "nvarchar(44)", maxLength: 44, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    XmlContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    UF = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IssuerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecipientCnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProcessingNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlHash = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocuments_Cnpj_IssueDate",
                table: "FiscalDocuments",
                columns: new[] { "Cnpj", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocuments_DocumentKey",
                table: "FiscalDocuments",
                column: "DocumentKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocuments_IssueDate",
                table: "FiscalDocuments",
                column: "IssueDate");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocuments_UF_IssueDate",
                table: "FiscalDocuments",
                columns: new[] { "UF", "IssueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocuments_XmlHash",
                table: "FiscalDocuments",
                column: "XmlHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiscalDocuments");
        }
    }
}
