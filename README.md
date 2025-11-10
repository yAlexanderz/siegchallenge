# Sistema de Gerenciamento de Documentos Fiscais - .NET 8

API REST profissional para upload, armazenamento, processamento e gerenciamento de documentos fiscais XML (NFe, CTe, NFSe) seguindo Clean Architecture e boas práticas de desenvolvimento.

## 📋 Índice

- [Características](#características)
- [Arquitetura](#arquitetura)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Pré-requisitos](#pré-requisitos)
- [Como Executar Localmente (SEM Docker)](#como-executar-localmente-sem-docker)
- [Endpoints da API](#endpoints-da-api)
- [Tratamento de Dados Sensíveis](#tratamento-de-dados-sensíveis)
- [Idempotência](#idempotência)
- [Resiliência](#resiliência)
- [Como Executar os Testes](#como-executar-os-testes)
- [Testes de Carga](#testes-de-carga)
- [Testes de Arquitetura](#testes-de-arquitetura)
- [Decisões de Arquitetura](#decisões-de-arquitetura)
- [Melhorias Futuras](#melhorias-futuras)

## 🚀 Características

### Funcionalidades Principais

- ✅ **Upload de XMLs Fiscais**: Suporte para NFe, CTe e NFSe
- ✅ **Listagem Paginada**: Com filtros por data, CNPJ e UF
- ✅ **Consulta de Detalhes**: Recuperação completa de documento específico
- ✅ **Atualização de Documentos**: Substituição de XML existente
- ✅ **Exclusão de Documentos**: Remoção segura
- ✅ **Publicação em RabbitMQ**: Eventos de documentos processados
- ✅ **Consumidor Resiliente**: Worker com retry, backoff exponencial e DLQ

### Qualidade e Segurança

- ✅ **Clean Architecture**: Separação clara de responsabilidades
- ✅ **CQRS com MediatR**: Commands e Queries separados
- ✅ **Repository Pattern**: Abstração da camada de dados
- ✅ **Idempotência**: Proteção contra duplicação via hash SHA256
- ✅ **Validação com FluentValidation**: Validação robusta de entrada
- ✅ **Logging Estruturado com Serilog**: Logs detalhados e mascarados
- ✅ **Tratamento Global de Erros**: Middleware customizado
- ✅ **Documentação Swagger/OpenAPI**: Documentação interativa
- ✅ **Testes Abrangentes**: Unitários, Integração, Arquitetura e Carga

## 🏗 Arquitetura

### Clean Architecture - Camadas

```
┌─────────────────────────────────────────────┐
│           FiscalDocuments.API               │
│         (Controllers, Middleware)           │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│       FiscalDocuments.Application           │
│  (Commands, Queries, Handlers, DTOs)        │
└─────────────────┬───────────────────────────┘
                  │
┌─────────────────▼───────────────────────────┐
│         FiscalDocuments.Domain              │
│    (Entities, Interfaces, Events)           │
└─────────────────△───────────────────────────┘
                  │
┌─────────────────┴───────────────────────────┐
│      FiscalDocuments.Infrastructure         │
│  (Repositories, DbContext, RabbitMQ)        │
└─────────────────────────────────────────────┘
```

### Fluxo de Processamento

```
1. Cliente → API Controller
2. Controller → MediatR Command/Query
3. MediatR → Handler
4. Handler → Domain + Repository
5. Handler → Event Publisher (RabbitMQ)
6. Worker → Consome RabbitMQ → Processa Evento
```

## 🛠 Tecnologias Utilizadas

### Core
- **.NET 8**: Framework principal
- **ASP.NET Core**: API REST
- **Entity Framework Core 8**: ORM
- **SQL Server**: Banco de dados relacional

### Bibliotecas e Frameworks
- **MediatR**: Implementação de CQRS e Mediator Pattern
- **FluentValidation**: Validação de dados
- **Serilog**: Logging estruturado
- **RabbitMQ.Client**: Mensageria
- **Polly**: Resiliência e retry policies
- **Swashbuckle (Swagger)**: Documentação OpenAPI

### Testes
- **NUnit**: Framework de testes
- **FluentAssertions**: Assertions fluentes
- **Moq**: Mocking
- **NBomber**: Testes de carga
- **NetArchTest**: Validação de arquitetura

## 📦 Pré-requisitos

### Obrigatórios

1. **.NET 8 SDK**
```bash
# Verificar instalação
dotnet --version
# Deve retornar 8.0.x ou superior
```

2. **SQL Server** (LocalDB, Express ou completo)
```bash
# LocalDB (incluído no Visual Studio)
# OU SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads
```

3. **RabbitMQ**
```bash
# Windows (Chocolatey)
choco install rabbitmq

# Windows (Installer)
# Download: https://www.rabbitmq.com/install-windows.html

# Verificar se está rodando
# Acessar: http://localhost:15672
# Usuário padrão: guest/guest
```

## 🚀 Como Executar Localmente (SEM Docker)

### Passo 1: Clonar o Repositório

```bash
git clone https://github.com/yAlexanderz/siegchallenge.git
cd FiscalDocuments
```

### Passo 2: Configurar Connection Strings

Edite o arquivo `src/FiscalDocuments.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FiscalDocumentsDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "ConnectionString": "amqp://guest:guest@localhost:5672"
  }
}
```

**Alternativas de Connection String:**

- **SQL Server Express**: `Server=localhost\\SQLEXPRESS;Database=FiscalDocumentsDb;Trusted_Connection=True;TrustServerCertificate=True;`
- **SQL Server com autenticação**: `Server=localhost;Database=FiscalDocumentsDb;User Id=sa;Password=SuaSenha123;TrustServerCertificate=True;`

### Passo 3: Restaurar Pacotes

```bash
dotnet restore
```

### Passo 4: Aplicar Migrations e Criar Banco de Dados

```bash
cd src/FiscalDocuments.API
dotnet ef database update

# Se não tiver dotnet ef instalado:
dotnet tool install --global dotnet-ef
```

**Alternativa (migrations são aplicadas automaticamente no startup):**
As migrations serão aplicadas automaticamente quando a API iniciar pela primeira vez.

### Passo 5: Iniciar RabbitMQ

```bash
# Windows - Iniciar serviço
net start RabbitMQ

# Verificar Management UI
# http://localhost:15672
# Usuário: guest / Senha: guest
```

### Passo 6: Executar a API

```bash
# Na pasta src/FiscalDocuments.API
dotnet run

# A API estará disponível em:
# http://localhost:5000
```

**Swagger UI**: Acesse `http://localhost:5000/index.html`

### Passo 7: Executar o Worker (Consumidor RabbitMQ)

**Em outro terminal:**

```bash
cd src/FiscalDocuments.Worker
dotnet run
```

O Worker ficará escutando a fila do RabbitMQ e processando eventos.

### Passo 8: Testar a API

#### Usando Swagger UI
1. Acesse `http://localhost:5000/index.html`
2. Navegue até o endpoint `POST /api/v1/fiscaldocuments/upload`
3. Faça upload de um arquivo XML de teste

#### Usando cURL

```bash
curl -X POST "http://localhost:5000/api/v1/fiscaldocuments/upload" \
  -H "accept: application/json" \
  -H "Content-Type: multipart/form-data" \
  -F "XmlFile=@caminho/para/seu/arquivo.xml"
```

#### Arquivo XML de Teste (NFe)

Crie um arquivo `nfe-teste.xml`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">
    <NFe>
        <infNFe Id="NFe35210112345678901234550010000000011234567890">
            <ide>
                <dhEmi>2025-01-15T10:30:00-03:00</dhEmi>
            </ide>
            <emit>
                <CNPJ>12345678901234</CNPJ>
                <xNome>Empresa Teste LTDA</xNome>
                <enderEmit>
                    <UF>PE</UF>
                </enderEmit>
            </emit>
            <dest>
                <CNPJ>98765432109876</CNPJ>
                <xNome>Cliente Teste SA</xNome>
            </dest>
            <total>
                <ICMSTot>
                    <vNF>1500.00</vNF>
                </ICMSTot>
            </total>
        </infNFe>
    </NFe>
</nfeProc>
```

## 📡 Endpoints da API

### Base URL
```
http://localhost:5000/api/v1/fiscaldocuments
```

### 1. Upload de Documento Fiscal

**POST** `/upload`

Faz upload de um arquivo XML fiscal (NFe, CTe, NFSe).

**Request:**
```
Content-Type: multipart/form-data
Body: XmlFile (arquivo .xml)
```

**Response 200 OK:**
```json
{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "documentKey": "35210112345678901234550010000000011234567890",
  "message": "Documento processado com sucesso",
  "isNewDocument": true
}
```

### 2. Listar Documentos (Paginado)

**GET** `/?pageNumber=1&pageSize=10&startDate=2025-01-01&cnpj=12345678901234&uf=PE`

**Query Parameters:**
- `pageNumber` (int, opcional): Número da página (padrão: 1)
- `pageSize` (int, opcional): Itens por página (padrão: 10, máx: 100)
- `startDate` (datetime, opcional): Data inicial do filtro
- `endDate` (datetime, opcional): Data final do filtro
- `cnpj` (string, opcional): CNPJ para filtro (14 dígitos)
- `uf` (string, opcional): UF para filtro (2 letras)

**Response 200 OK:**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "documentKey": "35210112345678901234550010000000011234567890",
      "type": 1,
      "typeDescription": "NFe",
      "cnpjMasked": "12.***.***/**34",
      "uf": "PE",
      "issueDate": "2025-01-15T10:30:00Z",
      "totalValue": 1500.00,
      "issuerName": "Empresa Teste LTDA",
      "recipientName": "Cliente Teste SA",
      "recipientCnpjMasked": "98.***.***/**76",
      "status": 3,
      "statusDescription": "Processed",
      "createdAt": "2025-01-15T13:45:00Z",
      "updatedAt": "2025-01-15T13:45:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 3. Consultar Documento por ID

**GET** `/{id}`

**Response 200 OK:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "documentKey": "35210112345678901234550010000000011234567890",
  "type": 1,
  "typeDescription": "NFe",
  "cnpjMasked": "12.***.***/**34",
  "uf": "PE",
  "issueDate": "2025-01-15T10:30:00Z",
  "totalValue": 1500.00,
  "issuerName": "Empresa Teste LTDA",
  "recipientName": "Cliente Teste SA",
  "recipientCnpjMasked": "98.***.***/**76",
  "status": 3,
  "statusDescription": "Processed",
  "xmlContent": "<?xml version=\"1.0\"...",
  "processingNotes": null,
  "createdAt": "2025-01-15T13:45:00Z",
  "updatedAt": "2025-01-15T13:45:00Z"
}
```

**Response 404 Not Found:**
```json
{
  "message": "Documento com ID {id} não encontrado"
}
```

### 4. Atualizar Documento

**PUT** `/{id}`

**Request:**
```
Content-Type: multipart/form-data
Body: XmlFile (arquivo .xml)
```

**Response 200 OK:** Retorna o documento atualizado (mesma estrutura do GET por ID)

### 5. Excluir Documento

**DELETE** `/{id}`

**Response 204 No Content:** Documento excluído com sucesso

**Response 404 Not Found:** Documento não encontrado

## 🔒 Tratamento de Dados Sensíveis

### Estratégias Implementadas

1. **Mascaramento de CNPJ em Logs e Respostas**
```csharp
// CNPJ original: 12345678901234
// CNPJ mascarado: 12.***.***/**34
```

- Logs estruturados sempre usam CNPJ mascarado
- DTOs de resposta retornam `cnpjMasked` e `recipientCnpjMasked`
- CNPJ completo armazenado apenas no banco de dados

2. **Logging Estruturado com Serilog**
```csharp
_logger.LogInformation(
    "Documento criado. DocumentId: {DocumentId}, CNPJ: {Cnpj}",
    document.Id,
    MaskCnpj(document.Cnpj) // Sempre mascarado
);
```

3. **Configurações Sensíveis no appsettings.json**
- Connection strings nunca commitadas com credenciais reais
- Uso de User Secrets em Development
- Variáveis de ambiente em Production

4. **Conteúdo XML Completo**
- Disponível apenas no endpoint de detalhes (GET /{id})
- Não exposto em listagens
- Logs nunca incluem conteúdo XML completo

5. **HTTPS Obrigatório em Produção**
- Certificados SSL/TLS
- Redirecionamento HTTP → HTTPS

### Exemplo de Configuração de User Secrets

```bash
# Inicializar User Secrets
cd src/FiscalDocuments.API
dotnet user-secrets init

# Adicionar secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=SenhaSegura;"
dotnet user-secrets set "RabbitMQ:ConnectionString" "amqp://user:pass@host:5672"
```

## 🔄 Idempotência

### Estratégia Implementada

A API garante idempotência através de **hash SHA256 do conteúdo XML**:

1. **Cálculo do Hash**
```csharp
using var sha256 = SHA256.Create();
var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xmlContent));
var xmlHash = Convert.ToBase64String(hashBytes);
```

2. **Verificação de Duplicidade**
- Antes de criar um documento, verifica se já existe um com o mesmo hash
- Se existir, retorna o documento existente com `isNewDocument: false`
- Garante que o mesmo XML nunca seja processado duas vezes

3. **Dupla Proteção**
- **Por Hash (XmlHash)**: Conteúdo idêntico
- **Por Chave de Documento (DocumentKey)**: Mesma chave de acesso fiscal

4. **Benefícios**
- Retry seguro em caso de timeout
- Proteção contra upload acidental duplicado
- Consistência de dados

### Exemplo de Resposta Idempotente

```json
{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "documentKey": "35210112345678901234550010000000011234567890",
  "message": "Documento já processado anteriormente (idempotência garantida)",
  "isNewDocument": false
}
```

## 🛡 Resiliência

### Worker RabbitMQ Resiliente

O consumidor RabbitMQ implementa múltiplas camadas de resiliência:

1. **Retry com Backoff Exponencial (Polly)**
```csharp
var retryPolicy = Policy
    .Handle<BrokerUnreachableException>()
    .WaitAndRetryAsync(
        5, // Máximo de 5 tentativas
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        // 2s, 4s, 8s, 16s, 32s
    );
```

2. **Reconexão Automática**
```csharp
var factory = new ConnectionFactory
{
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
};
```

3. **Prefetch e Manual ACK**
```csharp
_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
// Processa uma mensagem por vez

_channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
// ACK manual após processamento bem-sucedido
```

4. **Dead Letter Queue (DLQ)**
```csharp
// Após 3 falhas, rejeita e envia para DLQ
if (retryCount >= maxRetries)
{
    _channel.BasicNack(deliveryTag: ea.DeliveryTag, 
                      multiple: false, 
                      requeue: false);
}
```

5. **Circuit Breaker (Futuro)**
- Planejado para versões futuras usando Polly

### Entity Framework - Resiliência de Conexão

```csharp
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);
});
```

## 🧪 Como Executar os Testes

### Testes Unitários

```bash
cd tests/FiscalDocuments.UnitTests
dotnet test --logger "console;verbosity=detailed"
```

**Cobertura:**
- Handlers (Commands e Queries)
- Serviços (XmlParserService)
- Lógica de domínio
- Edge cases e validações

### Testes de Integração

```bash
cd tests/FiscalDocuments.IntegrationTests
dotnet test --logger "console;verbosity=detailed"
```

**Cobertura:**
- Endpoints completos da API
- Fluxo end-to-end
- Persistência em banco (In-Memory)
- Idempotência

### Executar Todos os Testes

```bash
# Na raiz da solution
dotnet test --logger "console;verbosity=detailed"
```

### Relatório de Cobertura (Opcional)

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Com ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.opencover.xml -targetdir:coverage-report
```

## 📊 Testes de Carga

### Usando NBomber (C#)

```bash
cd tests/FiscalDocuments.LoadTests
dotnet run
```

**Configuração do Teste:**
- **Taxa**: 10 requisições/segundo
- **Duração**: 30 segundos
- **Total**: ~300 requisições

**Métricas Coletadas:**
- Latência (p50, p75, p95, p99)
- Taxa de sucesso/falha
- Throughput (req/s)

### Interpretando Resultados

**Exemplo de saída NBomber:**
```
┌─────────────────────────────┬──────────┬─────────┐
│ Scenario                    │ ok       │ failed  │
├─────────────────────────────┼──────────┼─────────┤
│ upload_fiscal_documents     │ 295      │ 5       │
└─────────────────────────────┴──────────┴─────────┘

Latency:
  Min:  45ms
  Mean: 120ms
  Max:  850ms
  p50:  105ms
  p75:  145ms
  p95:  320ms
  p99:  650ms
```

## 🏛 Testes de Arquitetura

### Executar Testes de Arquitetura

```bash
cd tests/FiscalDocuments.ArchitectureTests
dotnet test
```

### Validações Implementadas

1. **Domain não depende de outras camadas**
```csharp
Domain.ShouldNotHaveDependencyOnOtherLayers()
```

2. **Application não depende de Infrastructure/API**
```csharp
Application.ShouldNotHaveDependencyOnInfrastructureOrApi()
```

3. **Infrastructure não depende de API**
```csharp
Infrastructure.ShouldNotHaveDependencyOnApi()
```

4. **Convenções de Nomenclatura**
- Controllers terminam com "Controller"
- Handlers terminam com "Handler"
- Repositories implementam IRepository

5. **Organização de Namespaces**
- Entidades em Domain.Entities
- Repositórios em Infrastructure.Repositories

### Benefícios

- Previne violações arquiteturais
- Garante manutenibilidade
- Documentação executável da arquitetura
- CI/CD pode falhar se arquitetura for violada

## 🎯 Decisões de Arquitetura

### 1. Banco de Dados: SQL Server vs MongoDB

**Escolha: SQL Server com Entity Framework Core**

**Justificativa:**
- ✅ Estrutura relacional bem definida
- ✅ ACID transactions para consistência
- ✅ Suporte robusto a índices compostos (CNPJ + Data)
- ✅ Queries complexas com filtros múltiplos
- ✅ Familiaridade do time .NET
- ✅ Ferramentas de administração maduras

**Quando MongoDB seria melhor:**
- Estrutura de documentos variável (NFSe varia por município)
- Necessidade de escalabilidade horizontal massiva
- Schemas flexíveis sem migrações frequentes

### 2. Mensageria: RabbitMQ

**Justificativa:**
- ✅ Broker maduro e confiável
- ✅ Suporte a DLQ (Dead Letter Queue)
- ✅ Flexibilidade de routing (exchanges, queues)
- ✅ Client library oficial e bem mantida
- ✅ Management UI útil para debug

**Alternativas consideradas:**
- **Azure Service Bus**: Excelente, mas cloud-specific
- **Apache Kafka**: Overkill para este cenário, melhor para event sourcing

### 3. CQRS com MediatR

**Justificativa:**
- ✅ Separação clara de Commands (escrita) e Queries (leitura)
- ✅ Single Responsibility Principle
- ✅ Facilita testes unitários
- ✅ Extensível com Behaviors (validação, logging, caching)

### 4. Repository Pattern

**Justificativa:**
- ✅ Abstração da camada de dados
- ✅ Facilita testes (mocking)
- ✅ Permite trocar implementação (SQL → MongoDB) sem impacto
- ✅ Encapsula lógica de queries complexas

### 5. DTOs Separados

**Justificativa:**
- ✅ Controle fino sobre o que é exposto na API
- ✅ Evolução independente do modelo de domínio
- ✅ Segurança (mascaramento de dados sensíveis)
- ✅ Versionamento de API facilitado

### 6. Idempotência por Hash

**Justificativa:**
- ✅ Garantia de não duplicação
- ✅ Retry seguro
- ✅ Simples e eficaz
- ✅ SHA256 é rápido e confiável

**Alternativa considerada:**
- **Idempotency Key** no header: Requer coordenação do cliente

### 7. Logging Estruturado (Serilog)

**Justificativa:**
- ✅ Logs estruturados e consultáveis
- ✅ Múltiplos sinks (Console, File, Elasticsearch futuro)
- ✅ Enrichers para contexto adicional
- ✅ Performance superior ao ILogger padrão

## 🚧 Melhorias Futuras

### Se Tivesse Mais Tempo

#### 1. Autenticação e Autorização
- **JWT Bearer Tokens**
- **OAuth 2.0 / OpenID Connect**
- **Políticas de autorização baseadas em claims**
- **Rate limiting por cliente**

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* ... */ });
```

#### 2. Cache Distribuído
- **Redis** para cache de consultas frequentes
- **Output caching** para endpoints de leitura
- **Cache de resultados de parse de XML**

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

#### 3. Observabilidade Avançada
- **OpenTelemetry** para traces distribuídos
- **Application Insights** ou **Jaeger**
- **Métricas customizadas** (Prometheus)
- **Health checks** detalhados

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder.AddAspNetCoreInstrumentation())
    .WithMetrics(builder => builder.AddAspNetCoreInstrumentation());
```

#### 4. Validação de Schema XML
- **XSD Schema validation** antes do parse
- **Validação de assinatura digital** dos XMLs
- **Verificação de validade junto à SEFAZ** (webservices)

#### 5. Armazenamento de XMLs
- **Azure Blob Storage** ou **AWS S3** para XMLs grandes
- **Banco armazena apenas metadados** + referência
- **Compressão** de XMLs antigos

#### 6. Event Sourcing
- **Histórico completo de mudanças** no documento
- **Auditoria granular**
- **Capacidade de replay de eventos**

#### 7. API Gateway
- **Ocelot** ou **YARP**
- **Rate limiting global**
- **Aggregation de múltiplos endpoints**

#### 8. Containerização
- **Dockerfile** multi-stage otimizado
- **Docker Compose** para stack completa
- **Kubernetes** manifests para produção

#### 9. CI/CD Pipeline
- **GitHub Actions** ou **Azure DevOps**
- **Build automatizado**
- **Testes em pipeline**
- **Deploy automatizado**

```yaml
# .github/workflows/ci.yml
name: CI
on: [push]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
      - name: Test
        run: dotnet test
```

#### 10. Documentação Adicional
- **AsyncAPI** para documentar eventos RabbitMQ
- **Architecture Decision Records (ADRs)**
- **Postman Collections**
- **Videos de demonstração**

#### 11. Melhorias de Performance
- **Batch processing** de múltiplos XMLs
- **Async all the way** (revisão de código)
- **Connection pooling** otimizado
- **Índices adicionais** baseados em queries reais

#### 12. Segurança Adicional
- **SQL Injection** - já protegido por EF, mas code review
- **XML External Entity (XXE)** - desabilitar DTD processing
- **Scanning de vulnerabilidades** (Dependabot, Snyk)
- **Secrets scanning** no Git

```csharp
var settings = new XmlReaderSettings
{
    DtdProcessing = DtdProcessing.Prohibit,
    XmlResolver = null
};
```

#### 13. Multi-tenancy
- **Suporte a múltiplas empresas**
- **Isolamento de dados por tenant**
- **Configurações por tenant**

#### 14. Webhooks
- **Notificar clientes** quando documento é processado
- **Retry de webhooks falhados**
- **Assinatura de payload** para segurança

#### 15. GraphQL
- **Alternativa REST** para queries complexas
- **Hot Chocolate** framework
- **Subscriptions** para notificações em tempo real

## 📝 Notas Finais

### Estrutura de Pastas Completa

```
FiscalDocuments/
├── src/
│   ├── FiscalDocuments.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── FiscalDocuments.Application/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Handlers/
│   │   ├── DTOs/
│   │   ├── Services/
│   │   ├── Validators/
│   │   └── Mappers/
│   ├── FiscalDocuments.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Events/
│   │   ├── Interfaces/
│   │   └── Common/
│   ├── FiscalDocuments.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   └── Messaging/
│   └── FiscalDocuments.Worker/
│       ├── Services/
│       └── Program.cs
├── tests/
│   ├── FiscalDocuments.UnitTests/
│   ├── FiscalDocuments.IntegrationTests/
│   ├── FiscalDocuments.ArchitectureTests/
│   └── FiscalDocuments.LoadTests/
├── FiscalDocuments.sln
└── README.md
```

### Comandos Úteis de Troubleshooting

```bash
# Verificar status do RabbitMQ
rabbitmqctl status

# Listar filas
rabbitmqctl list_queues

# Limpar fila
rabbitmqctl purge_queue fiscal-documents-processed

# Verificar conexões SQL Server
sqlcmd -S localhost -U sa -P SuaSenha -Q "SELECT name FROM sys.databases"

# Logs da aplicação
tail -f logs/fiscaldocuments-*.log

# Verificar portas em uso
netstat -ano | findstr :5000
netstat -ano | findstr :5672
```

### Contato e Suporte

Para dúvidas, abra uma issue no repositório ou entre em contato comigo.

---

**Desenvolvido por Yago Alexander usando .NET 8 e Clean Architecture**