📚 EduGraph Scheduler — Backend API

API backend desenvolvida em .NET 8 para gerenciamento de usuários e eventos acadêmicos, com integração ao Microsoft Graph e processamento assíncrono com Hangfire.

🚀 Tecnologias Utilizadas

.NET 8 (ASP.NET Core)

Entity Framework Core (ORM)

SQL Server / LocalDB

JWT Bearer Authentication

Microsoft Graph API

Hangfire (jobs e sincronização)

xUnit (testes unitários)

Swagger / OpenAPI (documentação)

📌 Pré-requisitos

Instale antes de começar:

.NET 8 SDK

SQL Server Express ou LocalDB

Visual Studio 2022+
 ou VS Code

🛠️ Instalação e Setup do Projeto
1️⃣ Clonar o repositório
git clone <url-do-repositorio>
cd EduGraphScheduler

2️⃣ Restaurar pacotes NuGet
dotnet restore

3️⃣ Configurar o banco de dados
Opção A — SQL Server LocalDB (recomendado)

Verifique as instâncias instaladas:

sqllocaldb info


Inicie a instância:

sqllocaldb start MSSQLLocalDB

Opção B — SQL Server Express

Atualize sua connection string no arquivo:
src/EduGraphScheduler.API/appsettings.json

4️⃣ Aplicar as migrations
cd src/EduGraphScheduler.Infrastructure
dotnet ef database update --startup-project ../EduGraphScheduler.API

5️⃣ Configurar Microsoft Graph

No arquivo src/EduGraphScheduler.API/appsettings.json:

"MicrosoftGraph": {
  "ClientId": "",
  "ClientSecret": "",
  "TenantId": "",
  "Scope": "https://graph.microsoft.com/.default"
}


⚠️ Nunca exponha secrets em um repositório público.
Coloque-os em variáveis de ambiente ou User Secrets.

▶️ Executando a Aplicação
Ambiente de desenvolvimento
cd src/EduGraphScheduler.API
dotnet run


A API ficará disponível em:

Swagger UI: http://localhost:5000/swagger

API Base: http://localhost:5000/api

Produção
dotnet publish -c Release -o ./publish
cd publish
dotnet EduGraphScheduler.API.dll

🧪 Executando Testes
Todos os testes
dotnet test

Com detalhes
dotnet test --verbosity normal

Testes da pasta Tests
cd src/EduGraphScheduler.Tests
dotnet test

Com cobertura
dotnet test --collect:"XPlat Code Coverage"

📁 Estrutura do Projeto
EduGraphScheduler/
├── src/
│   ├── EduGraphScheduler.API/           # Camada de Apresentação (Controllers)
│   ├── EduGraphScheduler.Application/   # Casos de uso / serviços / DTOs
│   ├── EduGraphScheduler.Domain/        # Entidades, interfaces e regras de negócio
│   ├── EduGraphScheduler.Infrastructure/# EF Core, repositórios, migrations
│   ├── EduGraphScheduler.Worker/        # Serviços em background (Hangfire)
│   └── EduGraphScheduler.Tests/         # Testes unitários
└── README.md

🔐 Autenticação (JWT)
Fluxo:

Registrar usuário

Fazer login

Utilizar Bearer token nos endpoints protegidos

Exemplos
Registrar
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@edu.com","password":"password123","displayName":"Test User"}'

Login
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password123"}'

Acessar endpoint protegido
curl -X GET "http://localhost:5000/api/users" \
  -H "Authorization: Bearer {token}"

📡 Endpoints Principais
🔹 Autenticação

POST /api/auth/register

POST /api/auth/login

POST /api/auth/validate

🔹 Usuários

GET /api/users

GET /api/users/{id}

POST /api/users/sync

🔹 Eventos

GET /api/events/user/{userId}

POST /api/events/sync/user/{userId}

POST /api/events/sync/all

🔹 Sincronização Geral

POST /api/sync/start

POST /api/sync/schedule

POST /api/sync/users

🔄 Sincronização Microsoft Graph

Ocorrência automática: A cada 6 horas (configurável)

Executada via Hangfire

Sincronização manual disponível via API

📍 Dashboard Hangfire (dev): /hangfire

🗄️ Migrations

Criar migration:

dotnet ef migrations add NomeDaMigration --startup-project ../EduGraphScheduler.API


Aplicar:

dotnet ef database update --startup-project ../EduGraphScheduler.API


Remover:

dotnet ef migrations remove --startup-project ../EduGraphScheduler.API

🐳 Docker (Opcional)

Build da imagem:

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EduGraphScheduler.API/EduGraphScheduler.API.csproj", "src/EduGraphScheduler.API/"]
COPY ["src/EduGraphScheduler.Application/EduGraphScheduler.Application.csproj", "src/EduGraphScheduler.Application/"]
COPY ["src/EduGraphScheduler.Domain/EduGraphScheduler.Domain.csproj", "src/EduGraphScheduler.Domain/"]
COPY ["src/EduGraphScheduler.Infrastructure/EduGraphScheduler.Infrastructure.csproj", "src/EduGraphScheduler.Infrastructure/"]
RUN dotnet restore "src/EduGraphScheduler.API/EduGraphScheduler.API.csproj"
COPY . .
WORKDIR "/src/src/EduGraphScheduler.API"
RUN dotnet build "EduGraphScheduler.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EduGraphScheduler.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EduGraphScheduler.API.dll"]

🩺 Solução de Problemas (FAQ)
❗ Erro: Conexão com SQL Server

Certifique-se de que o LocalDB está ativo:

sqllocaldb start MSSQLLocalDB

❗ Erro em migrations

Execute:

dotnet ef database drop
dotnet ef database update

❗ JWT inválido

Verifique o secret no appsettings.json

❗ Erro Microsoft Graph

Verifique:

ClientId

ClientSecret

TenantId

Permissões no Azure AD