\# Sistema de Gerenciamento de Documentos Fiscais - .NET 8



API REST profissional para upload, armazenamento, processamento e gerenciamento de documentos fiscais XML (NFe, CTe, NFSe) seguindo Clean Architecture e boas práticas de desenvolvimento.



\## 📋 Índice



\- \[Características](#características)

\- \[Arquitetura](#arquitetura)

\- \[Tecnologias Utilizadas](#tecnologias-utilizadas)

\- \[Pré-requisitos](#pré-requisitos)

\- \[Como Executar Localmente (SEM Docker)](#como-executar-localmente-sem-docker)

\- \[Endpoints da API](#endpoints-da-api)

\- \[Tratamento de Dados Sensíveis](#tratamento-de-dados-sensíveis)

\- \[Idempotência](#idempotência)

\- \[Resiliência](#resiliência)

\- \[Como Executar os Testes](#como-executar-os-testes)

\- \[Testes de Carga](#testes-de-carga)

\- \[Testes de Arquitetura](#testes-de-arquitetura)

\- \[Decisões de Arquitetura](#decisões-de-arquitetura)

\- \[Melhorias Futuras](#melhorias-futuras)



\## 🚀 Características



\### Funcionalidades Principais



\- ✅ \*\*Upload de XMLs Fiscais\*\*: Suporte para NFe, CTe e NFSe

\- ✅ \*\*Listagem Paginada\*\*: Com filtros por data, CNPJ e UF

\- ✅ \*\*Consulta de Detalhes\*\*: Recuperação completa de documento específico

\- ✅ \*\*Atualização de Documentos\*\*: Substituição de XML existente

\- ✅ \*\*Exclusão de Documentos\*\*: Remoção segura

\- ✅ \*\*Publicação em RabbitMQ\*\*: Eventos de documentos processados

\- ✅ \*\*Consumidor Resiliente\*\*: Worker com retry, backoff exponencial e DLQ



\### Qualidade e Segurança



\- ✅ \*\*Clean Architecture\*\*: Separação clara de responsabilidades

\- ✅ \*\*CQRS com MediatR\*\*: Commands e Queries separados

\- ✅ \*\*Repository Pattern\*\*: Abstração da camada de dados

\- ✅ \*\*Idempotência\*\*: Proteção contra duplicação via hash SHA256

\- ✅ \*\*Validação com FluentValidation\*\*: Validação robusta de entrada

\- ✅ \*\*Logging Estruturado com Serilog\*\*: Logs detalhados e mascarados

\- ✅ \*\*Tratamento Global de Erros\*\*: Middleware customizado

\- ✅ \*\*Documentação Swagger/OpenAPI\*\*: Documentação interativa

\- ✅ \*\*Testes Abrangentes\*\*: Unitários, Integração, Arquitetura e Carga



\## 🏗 Arquitetura



\### Clean Architecture - Camadas



```

┌─────────────────────────────────────────────┐

│           FiscalDocuments.API               │

│         (Controllers, Middleware)           │

└─────────────────┬───────────────────────────┘

&nbsp;                 │

┌─────────────────▼───────────────────────────┐

│       FiscalDocuments.Application           │

│  (Commands, Queries, Handlers, DTOs)        │

└─────────────────┬───────────────────────────┘

&nbsp;                 │

┌─────────────────▼───────────────────────────┐

│         FiscalDocuments.Domain              │

│    (Entities, Interfaces, Events)           │

└─────────────────△───────────────────────────┘

&nbsp;                 │

┌─────────────────┴───────────────────────────┐

│      FiscalDocuments.Infrastructure         │

│  (Repositories, DbContext, RabbitMQ)        │

└─────────────────────────────────────────────┘

```



\### Fluxo de Processamento



```

1\. Cliente → API Controller

2\. Controller → MediatR Command/Query

3\. MediatR → Handler

4\. Handler → Domain + Repository

5\. Handler → Event Publisher (RabbitMQ)

6\. Worker → Consome RabbitMQ → Processa Evento

```



\## 🛠 Tecnologias Utilizadas



\### Core

\- \*\*.NET 8\*\*: Framework principal

\- \*\*ASP.NET Core\*\*: API REST

\- \*\*Entity Framework Core 8\*\*: ORM

\- \*\*SQL Server\*\*: Banco de dados relacional



\### Bibliotecas e Frameworks

\- \*\*MediatR\*\*: Implementação de CQRS e Mediator Pattern

\- \*\*FluentValidation\*\*: Validação de dados

\- \*\*Serilog\*\*: Logging estruturado

\- \*\*RabbitMQ.Client\*\*: Mensageria

\- \*\*Polly\*\*: Resiliência e retry policies

\- \*\*Swashbuckle (Swagger)\*\*: Documentação OpenAPI



\### Testes

\- \*\*NUnit\*\*: Framework de testes

\- \*\*FluentAssertions\*\*: Assertions fluentes

\- \*\*Moq\*\*: Mocking

\- \*\*NBomber\*\*: Testes de carga

\- \*\*NetArchTest\*\*: Validação de arquitetura



\## 📦 Pré-requisitos



\### Obrigatórios



1\. \*\*.NET 8 SDK\*\*

&nbsp;  ```bash

&nbsp;  # Verificar instalação

&nbsp;  dotnet --version

&nbsp;  # Deve retornar 8.0.x ou superior

&nbsp;  ```



2\. \*\*SQL Server\*\* (LocalDB, Express ou completo)

&nbsp;  ```bash

&nbsp;  # LocalDB (incluído no Visual Studio)

&nbsp;  # OU SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads

&nbsp;  ```



3\. \*\*RabbitMQ\*\*

&nbsp;  ```bash

&nbsp;  # Windows (Chocolatey)

&nbsp;  choco install rabbitmq

&nbsp;  

&nbsp;  # Windows (Installer)

&nbsp;  # Download: https://www.rabbitmq.com/install-windows.html

&nbsp;  

&nbsp;  # Verificar se está rodando

&nbsp;  # Acessar: http://localhost:15672

&nbsp;  # Usuário padrão: guest/guest

&nbsp;  ```




\## 🚀 Como Executar Localmente (SEM Docker)



\### Passo 1: Clonar o Repositório



```bash

git clone https://github.com/yAlexanderz/siegchallenge.git

cd FiscalDocuments

```



\### Passo 2: Configurar Connection Strings



Edite o arquivo `src/FiscalDocuments.API/appsettings.json`:



```json

{

&nbsp; "ConnectionStrings": {

&nbsp;   "DefaultConnection": "Server=(localdb)\\\\mssqllocaldb;Database=FiscalDocumentsDb;Trusted\_Connection=True;TrustServerCertificate=True;"

&nbsp; },

&nbsp; "RabbitMQ": {

&nbsp;   "ConnectionString": "amqp://guest:guest@localhost:5672"

&nbsp; }

}

```



\*\*Alternativas de Connection String:\*\*



\- \*\*SQL Server Express\*\*: `Server=localhost\\\\SQLEXPRESS;Database=FiscalDocumentsDb;Trusted\_Connection=True;TrustServerCertificate=True;`

\- \*\*SQL Server com autenticação\*\*: `Server=localhost;Database=FiscalDocumentsDb;User Id=sa;Password=SuaSenha123;TrustServerCertificate=True;`



\### Passo 3: Restaurar Pacotes



```bash

dotnet restore

```



\### Passo 4: Aplicar Migrations e Criar Banco de Dados



```bash

cd src/FiscalDocuments.API

dotnet ef database update



\# Se não tiver dotnet ef instalado:

dotnet tool install --global dotnet-ef

```



\*\*Alternativa (migrations são aplicadas automaticamente no startup):\*\*

As migrations serão aplicadas automaticamente quando a API iniciar pela primeira vez.



\### Passo 5: Iniciar RabbitMQ



```bash

\# Windows - Iniciar serviço

net start RabbitMQ



\# Verificar Management UI

\# http://localhost:15672

\# Usuário: guest / Senha: guest

```



\### Passo 6: Executar a API



```bash

\# Na pasta src/FiscalDocuments.API

dotnet run



\# A API estará disponível em:

\# http://localhost:5000


```



\*\*Swagger UI\*\*: Acesse `http://localhost:5000/index.html`



\### Passo 7: Executar o Worker (Consumidor RabbitMQ)



\*\*Em outro terminal:\*\*



```bash

cd src/FiscalDocuments.Worker

dotnet run

```



O Worker ficará escutando a fila do RabbitMQ e processando eventos.



\### Passo 8: Testar a API



\#### Usando Swagger UI

1\. Acesse `http://localhost:5000/index.html`

2\. Navegue até o endpoint `POST /api/v1/fiscaldocuments/upload`

3\. Faça upload de um arquivo XML de teste



\#### Usando cURL



```bash

curl -X POST "http://localhost:5000/api/v1/fiscaldocuments/upload" \\

&nbsp; -H "accept: application/json" \\

&nbsp; -H "Content-Type: multipart/form-data" \\

&nbsp; -F "XmlFile=@caminho/para/seu/arquivo.xml"

```



\#### Arquivo XML de Teste (NFe)



Crie um arquivo `nfe-teste.xml`:



```xml

<?xml version="1.0" encoding="UTF-8"?>

<nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">

&nbsp;   <NFe>

&nbsp;       <infNFe Id="NFe35210112345678901234550010000000011234567890">

&nbsp;           <ide>

&nbsp;               <dhEmi>2025-01-15T10:30:00-03:00</dhEmi>

&nbsp;           </ide>

&nbsp;           <emit>

&nbsp;               <CNPJ>12345678901234</CNPJ>

&nbsp;               <xNome>Empresa Teste LTDA</xNome>

&nbsp;               <enderEmit>

&nbsp;                   <UF>PE</UF>

&nbsp;               </enderEmit>

&nbsp;           </emit>

&nbsp;           <dest>

&nbsp;               <CNPJ>98765432109876</CNPJ>

&nbsp;               <xNome>Cliente Teste SA</xNome>

&nbsp;           </dest>

&nbsp;           <total>

&nbsp;               <ICMSTot>

&nbsp;                   <vNF>1500.00</vNF>

&nbsp;               </ICMSTot>

&nbsp;           </total>

&nbsp;       </infNFe>

&nbsp;   </NFe>

</nfeProc>

```



\## 📡 Endpoints da API



\### Base URL

```

http://localhost:5000/api/v1/fiscaldocuments

```



\### 1. Upload de Documento Fiscal



\*\*POST\*\* `/upload`



Faz upload de um arquivo XML fiscal (NFe, CTe, NFSe).



\*\*Request:\*\*

```

Content-Type: multipart/form-data

Body: XmlFile (arquivo .xml)

```



\*\*Response 200 OK:\*\*

```json

{

&nbsp; "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",

&nbsp; "documentKey": "35210112345678901234550010000000011234567890",

&nbsp; "message": "Documento processado com sucesso",

&nbsp; "isNewDocument": true

}

```



\### 2. Listar Documentos (Paginado)



\*\*GET\*\* `/?pageNumber=1\&pageSize=10\&startDate=2025-01-01\&cnpj=12345678901234\&uf=SP`



\*\*Query Parameters:\*\*

\- `pageNumber` (int, opcional): Número da página (padrão: 1)

\- `pageSize` (int, opcional): Itens por página (padrão: 10, máx: 100)

\- `startDate` (datetime, opcional): Data inicial do filtro

\- `endDate` (datetime, opcional): Data final do filtro

\- `cnpj` (string, opcional): CNPJ para filtro (14 dígitos)

\- `uf` (string, opcional): UF para filtro (2 letras)



\*\*Response 200 OK:\*\*

```json

{

&nbsp; "data": \[

&nbsp;   {

&nbsp;     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",

&nbsp;     "documentKey": "35210112345678901234550010000000011234567890",

&nbsp;     "type": 1,

&nbsp;     "typeDescription": "NFe",

&nbsp;     "cnpjMasked": "12.\*\*\*.\*\*\*/\*\*34",

&nbsp;     "uf": "PE",

&nbsp;     "issueDate": "2025-01-15T10:30:00Z",

&nbsp;     "totalValue": 1500.00,

&nbsp;     "issuerName": "Empresa Teste LTDA",

&nbsp;     "recipientName": "Cliente Teste SA",

&nbsp;     "recipientCnpjMasked": "98.\*\*\*.\*\*\*/\*\*76",

&nbsp;     "status": 3,

&nbsp;     "statusDescription": "Processed",

&nbsp;     "createdAt": "2025-01-15T13:45:00Z",

&nbsp;     "updatedAt": "2025-01-15T13:45:00Z"

&nbsp;   }

&nbsp; ],

&nbsp; "pageNumber": 1,

&nbsp; "pageSize": 10,

&nbsp; "totalCount": 45,

&nbsp; "totalPages": 5,

&nbsp; "hasPreviousPage": false,

&nbsp; "hasNextPage": true

}

```



\### 3. Consultar Documento por ID



\*\*GET\*\* `/{id}`



\*\*Response 200 OK:\*\*

```json

{

&nbsp; "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",

&nbsp; "documentKey": "35210112345678901234550010000000011234567890",

&nbsp; "type": 1,

&nbsp; "typeDescription": "NFe",

&nbsp; "cnpjMasked": "12.\*\*\*.\*\*\*/\*\*34",

&nbsp; "uf": "PE",

&nbsp; "issueDate": "2025-01-15T10:30:00Z",

&nbsp; "totalValue": 1500.00,

&nbsp; "issuerName": "Empresa Teste LTDA",

&nbsp; "recipientName": "Cliente Teste SA",

&nbsp; "recipientCnpjMasked": "98.\*\*\*.\*\*\*/\*\*76",

&nbsp; "status": 3,

&nbsp; "statusDescription": "Processed",

&nbsp; "xmlContent": "<?xml version=\\"1.0\\"...",

&nbsp; "processingNotes": null,

&nbsp; "createdAt": "2025-01-15T13:45:00Z",

&nbsp; "updatedAt": "2025-01-15T13:45:00Z"

}

```



\*\*Response 404 Not Found:\*\*

```json

{

&nbsp; "message": "Documento com ID {id} não encontrado"

}

```



\### 4. Atualizar Documento



\*\*PUT\*\* `/{id}`



\*\*Request:\*\*

```

Content-Type: multipart/form-data

Body: XmlFile (arquivo .xml)

```



\*\*Response 200 OK:\*\* Retorna o documento atualizado (mesma estrutura do GET por ID)



\### 5. Excluir Documento



\*\*DELETE\*\* `/{id}`



\*\*Response 204 No Content:\*\* Documento excluído com sucesso



\*\*Response 404 Not Found:\*\* Documento não encontrado



\## 🔒 Tratamento de Dados Sensíveis



\### Estratégias Implementadas



1\. \*\*Mascaramento de CNPJ em Logs e Respostas\*\*

&nbsp;  ```csharp

&nbsp;  // CNPJ original: 12345678901234

&nbsp;  // CNPJ mascarado: 12.\*\*\*.\*\*\*/\*\*34

&nbsp;  ```

&nbsp;  

&nbsp;  - Logs estruturados sempre usam CNPJ mascarado

&nbsp;  - DTOs de resposta retornam `cnpjMasked` e `recipientCnpjMasked`

&nbsp;  - CNPJ completo armazenado apenas no banco de dados



2\. \*\*Logging Estruturado com Serilog\*\*

&nbsp;  ```csharp

&nbsp;  \_logger.LogInformation(

&nbsp;      "Documento criado. DocumentId: {DocumentId}, CNPJ: {Cnpj}",

&nbsp;      document.Id,

&nbsp;      MaskCnpj(document.Cnpj) // Sempre mascarado

&nbsp;  );

&nbsp;  ```



3\. \*\*Configurações Sensíveis no appsettings.json\*\*

&nbsp;  - Connection strings nunca commitadas com credenciais reais

&nbsp;  - Uso de User Secrets em Development

&nbsp;  - Variáveis de ambiente em Production



4\. \*\*Conteúdo XML Completo\*\*

&nbsp;  - Disponível apenas no endpoint de detalhes (GET /{id})

&nbsp;  - Não exposto em listagens

&nbsp;  - Logs nunca incluem conteúdo XML completo



5\. \*\*HTTPS Obrigatório em Produção\*\*

&nbsp;  - Certificados SSL/TLS

&nbsp;  - Redirecionamento HTTP → HTTPS



\### Exemplo de Configuração de User Secrets



```bash

\# Inicializar User Secrets

cd src/FiscalDocuments.API

dotnet user-secrets init



\# Adicionar secrets

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=SenhaSegura;"

dotnet user-secrets set "RabbitMQ:ConnectionString" "amqp://user:pass@host:5672"

```



\## 🔄 Idempotência



\### Estratégia Implementada



A API garante idempotência através de \*\*hash SHA256 do conteúdo XML\*\*:



1\. \*\*Cálculo do Hash\*\*

&nbsp;  ```csharp

&nbsp;  using var sha256 = SHA256.Create();

&nbsp;  var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xmlContent));

&nbsp;  var xmlHash = Convert.ToBase64String(hashBytes);

&nbsp;  ```



2\. \*\*Verificação de Duplicidade\*\*

&nbsp;  - Antes de criar um documento, verifica se já existe um com o mesmo hash

&nbsp;  - Se existir, retorna o documento existente com `isNewDocument: false`

&nbsp;  - Garante que o mesmo XML nunca seja processado duas vezes



3\. \*\*Dupla Proteção\*\*

&nbsp;  - \*\*Por Hash (XmlHash)\*\*: Conteúdo idêntico

&nbsp;  - \*\*Por Chave de Documento (DocumentKey)\*\*: Mesma chave de acesso fiscal



4\. \*\*Benefícios\*\*

&nbsp;  - Retry seguro em caso de timeout

&nbsp;  - Proteção contra upload acidental duplicado

&nbsp;  - Consistência de dados



\### Exemplo de Resposta Idempotente



```json

{

&nbsp; "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",

&nbsp; "documentKey": "35210112345678901234550010000000011234567890",

&nbsp; "message": "Documento já processado anteriormente (idempotência garantida)",

&nbsp; "isNewDocument": false

}

```



\## 🛡 Resiliência



\### Worker RabbitMQ Resiliente



O consumidor RabbitMQ implementa múltiplas camadas de resiliência:



1\. \*\*Retry com Backoff Exponencial (Polly)\*\*

&nbsp;  ```csharp

&nbsp;  var retryPolicy = Policy

&nbsp;      .Handle<BrokerUnreachableException>()

&nbsp;      .WaitAndRetryAsync(

&nbsp;          5, // Máximo de 5 tentativas

&nbsp;          retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))

&nbsp;          // 2s, 4s, 8s, 16s, 32s

&nbsp;      );

&nbsp;  ```



2\. \*\*Reconexão Automática\*\*

&nbsp;  ```csharp

&nbsp;  var factory = new ConnectionFactory

&nbsp;  {

&nbsp;      AutomaticRecoveryEnabled = true,

&nbsp;      NetworkRecoveryInterval = TimeSpan.FromSeconds(10)

&nbsp;  };

&nbsp;  ```



3\. \*\*Prefetch e Manual ACK\*\*

&nbsp;  ```csharp

&nbsp;  \_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

&nbsp;  // Processa uma mensagem por vez

&nbsp;  

&nbsp;  \_channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

&nbsp;  // ACK manual após processamento bem-sucedido

&nbsp;  ```



4\. \*\*Dead Letter Queue (DLQ)\*\*

&nbsp;  ```csharp

&nbsp;  // Após 3 falhas, rejeita e envia para DLQ

&nbsp;  if (retryCount >= maxRetries)

&nbsp;  {

&nbsp;      \_channel.BasicNack(deliveryTag: ea.DeliveryTag, 

&nbsp;                        multiple: false, 

&nbsp;                        requeue: false);

&nbsp;  }

&nbsp;  ```



5\. \*\*Circuit Breaker (Futuro)\*\*

&nbsp;  - Planejado para versões futuras usando Polly



\### Entity Framework - Resiliência de Conexão



```csharp

options.UseSqlServer(connectionString, sqlOptions =>

{

&nbsp;   sqlOptions.EnableRetryOnFailure(

&nbsp;       maxRetryCount: 5,

&nbsp;       maxRetryDelay: TimeSpan.FromSeconds(30),

&nbsp;       errorNumbersToAdd: null);

});

```



\## 🧪 Como Executar os Testes



\### Testes Unitários



```bash

cd tests/FiscalDocuments.UnitTests

dotnet test --logger "console;verbosity=detailed"

```



\*\*Cobertura:\*\*

\- Handlers (Commands e Queries)

\- Serviços (XmlParserService)

\- Lógica de domínio

\- Edge cases e validações



\### Testes de Integração



```bash

cd tests/FiscalDocuments.IntegrationTests

dotnet test --logger "console;verbosity=detailed"

```



\*\*Cobertura:\*\*

\- Endpoints completos da API

\- Fluxo end-to-end

\- Persistência em banco (In-Memory)

\- Idempotência



\### Executar Todos os Testes



```bash

\# Na raiz da solution

dotnet test --logger "console;verbosity=detailed"

```



\### Relatório de Cobertura (Opcional)



```bash

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover



\# Com ReportGenerator

dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator -reports:\*\*/coverage.opencover.xml -targetdir:coverage-report

```



\## 📊 Testes de Carga



\### Usando NBomber (C#)



```bash

cd tests/FiscalDocuments.LoadTests

dotnet run

```



\*\*Configuração do Teste:\*\*

\- \*\*Taxa\*\*: 10 requisições/segundo

\- \*\*Duração\*\*: 30 segundos

\- \*\*Total\*\*: ~300 requisições



\*\*Métricas Coletadas:\*\*

\- Latência (p50, p75, p95, p99)

\- Taxa de sucesso/falha

\- Throughput (req/s)



\### Interpretando Resultados



\*\*Exemplo de saída NBomber:\*\*

```

┌─────────────────────────────┬──────────┬─────────┐

│ Scenario                    │ ok       │ failed  │

├─────────────────────────────┼──────────┼─────────┤

│ upload\_fiscal\_documents     │ 295      │ 5       │

└─────────────────────────────┴──────────┴─────────┘



Latency:

&nbsp; Min:  45ms

&nbsp; Mean: 120ms

&nbsp; Max:  850ms

&nbsp; p50:  105ms

&nbsp; p75:  145ms

&nbsp; p95:  320ms

&nbsp; p99:  650ms

```



\## 🏛 Testes de Arquitetura



\### Executar Testes de Arquitetura



```bash

cd tests/FiscalDocuments.ArchitectureTests

dotnet test

```



\### Validações Implementadas



1\. \*\*Domain não depende de outras camadas\*\*

&nbsp;  ```csharp

&nbsp;  Domain.ShouldNotHaveDependencyOnOtherLayers()

&nbsp;  ```



2\. \*\*Application não depende de Infrastructure/API\*\*

&nbsp;  ```csharp

&nbsp;  Application.ShouldNotHaveDependencyOnInfrastructureOrApi()

&nbsp;  ```



3\. \*\*Infrastructure não depende de API\*\*

&nbsp;  ```csharp

&nbsp;  Infrastructure.ShouldNotHaveDependencyOnApi()

&nbsp;  ```



4\. \*\*Convenções de Nomenclatura\*\*

&nbsp;  - Controllers terminam com "Controller"

&nbsp;  - Handlers terminam com "Handler"

&nbsp;  - Repositories implementam IRepository



5\. \*\*Organização de Namespaces\*\*

&nbsp;  - Entidades em Domain.Entities

&nbsp;  - Repositórios em Infrastructure.Repositories



\### Benefícios



\- Previne violações arquiteturais

\- Garante manutenibilidade

\- Documentação executável da arquitetura

\- CI/CD pode falhar se arquitetura for violada



\## 🎯 Decisões de Arquitetura



\### 1. Banco de Dados: SQL Server vs MongoDB



\*\*Escolha: SQL Server com Entity Framework Core\*\*



\*\*Justificativa:\*\*

\- ✅ Estrutura relacional bem definida

\- ✅ ACID transactions para consistência

\- ✅ Suporte robusto a índices compostos (CNPJ + Data)

\- ✅ Queries complexas com filtros múltiplos

\- ✅ Familiaridade do time .NET

\- ✅ Ferramentas de administração maduras



\*\*Quando MongoDB seria melhor:\*\*

\- Estrutura de documentos variável (NFSe varia por município)

\- Necessidade de escalabilidade horizontal massiva

\- Schemas flexíveis sem migrações frequentes



\### 2. Mensageria: RabbitMQ



\*\*Justificativa:\*\*

\- ✅ Broker maduro e confiável

\- ✅ Suporte a DLQ (Dead Letter Queue)

\- ✅ Flexibilidade de routing (exchanges, queues)

\- ✅ Client library oficial e bem mantida

\- ✅ Management UI útil para debug



\*\*Alternativas consideradas:\*\*

\- \*\*Azure Service Bus\*\*: Excelente, mas cloud-specific

\- \*\*Apache Kafka\*\*: Overkill para este cenário, melhor para event sourcing



\### 3. CQRS com MediatR



\*\*Justificativa:\*\*

\- ✅ Separação clara de Commands (escrita) e Queries (leitura)

\- ✅ Single Responsibility Principle

\- ✅ Facilita testes unitários

\- ✅ Extensível com Behaviors (validação, logging, caching)



\### 4. Repository Pattern



\*\*Justificativa:\*\*

\- ✅ Abstração da camada de dados

\- ✅ Facilita testes (mocking)

\- ✅ Permite trocar implementação (SQL → MongoDB) sem impacto

\- ✅ Encapsula lógica de queries complexas



\### 5. DTOs Separados



\*\*Justificativa:\*\*

\- ✅ Controle fino sobre o que é exposto na API

\- ✅ Evolução independente do modelo de domínio

\- ✅ Segurança (mascaramento de dados sensíveis)

\- ✅ Versionamento de API facilitado



\### 6. Idempotência por Hash



\*\*Justificativa:\*\*

\- ✅ Garantia de não duplicação

\- ✅ Retry seguro

\- ✅ Simples e eficaz

\- ✅ SHA256 é rápido e confiável



\*\*Alternativa considerada:\*\*

\- \*\*Idempotency Key\*\* no header: Requer coordenação do cliente



\### 7. Logging Estruturado (Serilog)



\*\*Justificativa:\*\*

\- ✅ Logs estruturados e consultáveis

\- ✅ Múltiplos sinks (Console, File, Elasticsearch futuro)

\- ✅ Enrichers para contexto adicional

\- ✅ Performance superior ao ILogger padrão



\## 🚧 Melhorias Futuras



\### Se Tivesse Mais Tempo



\#### 1. Autenticação e Autorização

\- \*\*JWT Bearer Tokens\*\*

\- \*\*OAuth 2.0 / OpenID Connect\*\*

\- \*\*Políticas de autorização baseadas em claims\*\*

\- \*\*Rate limiting por cliente\*\*



```csharp

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

&nbsp;   .AddJwtBearer(options => { /\* ... \*/ });

```



\#### 2. Cache Distribuído

\- \*\*Redis\*\* para cache de consultas frequentes

\- \*\*Output caching\*\* para endpoints de leitura

\- \*\*Cache de resultados de parse de XML\*\*



```csharp

builder.Services.AddStackExchangeRedisCache(options =>

{

&nbsp;   options.Configuration = "localhost:6379";

});

```



\#### 3. Observabilidade Avançada

\- \*\*OpenTelemetry\*\* para traces distribuídos

\- \*\*Application Insights\*\* ou \*\*Jaeger\*\*

\- \*\*Métricas customizadas\*\* (Prometheus)

\- \*\*Health checks\*\* detalhados



```csharp

builder.Services.AddOpenTelemetry()

&nbsp;   .WithTracing(builder => builder.AddAspNetCoreInstrumentation())

&nbsp;   .WithMetrics(builder => builder.AddAspNetCoreInstrumentation());

```



\#### 4. Validação de Schema XML

\- \*\*XSD Schema validation\*\* antes do parse

\- \*\*Validação de assinatura digital\*\* dos XMLs

\- \*\*Verificação de validade junto à SEFAZ\*\* (webservices)



\#### 5. Armazenamento de XMLs

\- \*\*Azure Blob Storage\*\* ou \*\*AWS S3\*\* para XMLs grandes

\- \*\*Banco armazena apenas metadados\*\* + referência

\- \*\*Compressão\*\* de XMLs antigos



\#### 6. Event Sourcing

\- \*\*Histórico completo de mudanças\*\* no documento

\- \*\*Auditoria granular\*\*

\- \*\*Capacidade de replay de eventos\*\*



\#### 7. API Gateway

\- \*\*Ocelot\*\* ou \*\*YARP\*\*

\- \*\*Rate limiting global\*\*

\- \*\*Aggregation de múltiplos endpoints\*\*



\#### 8. Containerização

\- \*\*Dockerfile\*\* multi-stage otimizado

\- \*\*Docker Compose\*\* para stack completa

\- \*\*Kubernetes\*\* manifests para produção



\#### 9. CI/CD Pipeline

\- \*\*GitHub Actions\*\* ou \*\*Azure DevOps\*\*

\- \*\*Build automatizado\*\*

\- \*\*Testes em pipeline\*\*

\- \*\*Deploy automatizado\*\*



```yaml

\# .github/workflows/ci.yml

name: CI

on: \[push]

jobs:

&nbsp; build:

&nbsp;   runs-on: ubuntu-latest

&nbsp;   steps:

&nbsp;     - uses: actions/checkout@v2

&nbsp;     - name: Setup .NET

&nbsp;       uses: actions/setup-dotnet@v1

&nbsp;     - name: Test

&nbsp;       run: dotnet test

```



\#### 10. Documentação Adicional

\- \*\*AsyncAPI\*\* para documentar eventos RabbitMQ

\- \*\*Architecture Decision Records (ADRs)\*\*

\- \*\*Postman Collections\*\*

\- \*\*Videos de demonstração\*\*



\#### 11. Melhorias de Performance

\- \*\*Batch processing\*\* de múltiplos XMLs

\- \*\*Async all the way\*\* (revisão de código)

\- \*\*Connection pooling\*\* otimizado

\- \*\*Índices adicionais\*\* baseados em queries reais



\#### 12. Segurança Adicional

\- \*\*SQL Injection\*\* - já protegido por EF, mas code review

\- \*\*XML External Entity (XXE)\*\* - desabilitar DTD processing

\- \*\*Scanning de vulnerabilidades\*\* (Dependabot, Snyk)

\- \*\*Secrets scanning\*\* no Git



```csharp

var settings = new XmlReaderSettings

{

&nbsp;   DtdProcessing = DtdProcessing.Prohibit,

&nbsp;   XmlResolver = null

};

```



\#### 13. Multi-tenancy

\- \*\*Suporte a múltiplas empresas\*\*

\- \*\*Isolamento de dados por tenant\*\*

\- \*\*Configurações por tenant\*\*



\#### 14. Webhooks

\- \*\*Notificar clientes\*\* quando documento é processado

\- \*\*Retry de webhooks falhados\*\*

\- \*\*Assinatura de payload\*\* para segurança



\#### 15. GraphQL

\- \*\*Alternativa REST\*\* para queries complexas

\- \*\*Hot Chocolate\*\* framework

\- \*\*Subscriptions\*\* para notificações em tempo real



\## 📝 Notas Finais



\### Estrutura de Pastas Completa



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



\### Comandos Úteis de Troubleshooting



```bash

\# Verificar status do RabbitMQ

rabbitmqctl status



\# Listar filas

rabbitmqctl list\_queues



\# Limpar fila

rabbitmqctl purge\_queue fiscal-documents-processed



\# Verificar conexões SQL Server

sqlcmd -S localhost -U sa -P SuaSenha -Q "SELECT name FROM sys.databases"



\# Logs da aplicação

tail -f logs/fiscaldocuments-\*.log



\# Verificar portas em uso

netstat -ano | findstr :5000

netstat -ano | findstr :5672

```



\### Contato e Suporte



Para dúvidas, abra uma issue no repositório ou entre em contato comigo.



---



\*\*Desenvolvido por Yago Alexander usando .NET 8 e Clean Architecture\*\*

