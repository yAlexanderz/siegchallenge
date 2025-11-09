using FluentValidation;
using FiscalDocuments.API.Middleware;
using FiscalDocuments.Application.Services;
using FiscalDocuments.Application.Validators;
using FiscalDocuments.Domain.Interfaces;
using FiscalDocuments.Infrastructure.Messaging;
using FiscalDocuments.Infrastructure.Persistence;
using FiscalDocuments.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog para logging estruturado
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/fiscaldocuments-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configurar DbContext com SQL Server
builder.Services.AddDbContext<FiscalDocumentsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar repositórios
builder.Services.AddScoped<IFiscalDocumentRepository, FiscalDocumentRepository>();

// Registrar serviços
builder.Services.AddScoped<IXmlParserService, XmlParserService>();

// Registrar RabbitMQ
builder.Services.AddSingleton<IEventPublisher>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMqEventPublisher>>();
    var connectionString = builder.Configuration.GetValue<string>("RabbitMQ:ConnectionString")
        ?? "amqp://guest:guest@localhost:5672";
    return new RabbitMqEventPublisher(logger, connectionString);
});

// Registrar MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(FiscalDocuments.Application.Commands.UploadDocumentCommand).Assembly));

// Registrar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UploadDocumentCommandValidator>();

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Gerenciamento de Documentos Fiscais - Sieg Challenge",
        Version = "v1",
        Description = "API REST para upload, armazenamento e gerenciamento de documentos fiscais XML (NFe, CTe, NFSe)",
        Contact = new OpenApiContact
        {
            Name = "Equipe de Desenvolvimento",
            Email = "yago.alexander3@gmail.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Aplicar migrations automaticamente (apenas para bancos relacionais)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FiscalDocumentsDbContext>();

    // Verificar se está usando provider relacional (SQL Server) antes de migrar
    if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
    {
        dbContext.Database.Migrate();
    }
    else
    {
        // Para In-Memory (usado nos testes), apenas garantir que está criado
        dbContext.Database.EnsureCreated();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fiscal Documents API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Classe parcial pública para testes de integração
public partial class Program { }