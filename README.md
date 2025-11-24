📚 EduGraph Scheduler - Backend API
🎯 Sobre o Projeto
API backend desenvolvida em .NET 8 para gerenciamento de usuários e eventos acadêmicos de instituições de ensino, com integração completa ao Microsoft Graph e processamento assíncrono com Hangfire.

⚡ Funcionalidades Principais
👥 Gestão de Usuários
Listagem completa de 253.207+ usuários

Busca e filtros por nome, email e departamento

Paginação otimizada para alta volumetria

Detalhes completos do usuário

📅 Gestão de Eventos
Visualização de agendas individuais

Sincronização inteligente com Microsoft Graph

Verificação prévia de existência de eventos

Atualização em background

🔄 Sincronização Automática
Jobs recorrentes via Hangfire (a cada 6 horas)

Processamento inteligente - só sincroniza usuários com eventos

Rate limiting automático para Microsoft Graph

Sincronização manual via endpoints da API

🔐 Segurança
Autenticação JWT Bearer

Ambiente protegido para dados confidenciais

CORS configurado para frontend

Validação de tokens

🏗️ Arquitetura
EduGraphScheduler/
├── src/
│ ├── EduGraphScheduler.API/
│ ├── EduGraphScheduler.Application/
│ ├── EduGraphScheduler.Domain/
│ ├── EduGraphScheduler.Infrastructure/
│ ├── EduGraphScheduler.Worker/ 
  └── EduGraphScheduler.Tests/ 

🛠️ Tecnologias Utilizadas
.NET 8 - ASP.NET Core Web API

Entity Framework Core - ORM com SQL Server

Microsoft Graph SDK - Integração com Office 365

Hangfire - Agendamento de jobs em background

JWT Bearer Authentication - Autenticação segura

Swagger/OpenAPI - Documentação interativa

xUnit - Testes unitários

Azure Identity - Autenticação com Azure AD

📦 Estrutura do Projeto
Camadas da Aplicação
Camada	Responsabilidade
API	Controllers, Middleware, Configuração
Application	Casos de uso, Serviços, DTOs
Domain	Entidades, Regras de negócio
Infrastructure	EF Core, Repositórios, Serviços externos
Entidades Principais
csharp
public class User
{
    public Guid Id { get; set; }
    public string MicrosoftGraphId { get; set; }
    public string DisplayName { get; set; }
    public string UserPrincipalName { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public int EventCount { get; set; }
    public DateTime? LastEventCheckAt { get; set; }
}

public class CalendarEvent
{
    public Guid Id { get; set; }
    public string MicrosoftGraphEventId { get; set; }
    public string Subject { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
}
🚀 Configuração e Instalação
Pré-requisitos
.NET 8 SDK

SQL Server 2019+ (LocalDB, Express ou Full)

Conta Azure AD com permissões Microsoft Graph

1. Clone o repositório
bash
git clone https://github.com/Marestoni/Desafio_FullStack_Back_end.git
cd edugraph-scheduler
2. Restaure as dependências
bash
dotnet restore
3. Configure o banco de dados
Opção A - LocalDB (Desenvolvimento)

bash
sqllocaldb start MSSQLLocalDB
cd src/EduGraphScheduler.Infrastructure
dotnet ef database update --startup-project ../EduGraphScheduler.API
Opção B - SQL Server
Atualize a connection string em appsettings.json:

json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EduGraphScheduler;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
4. Configure o Microsoft Graph
Edite src/EduGraphScheduler.API/appsettings.json:

json
{
  "MicrosoftGraph": {
    "ClientId": "SEU CLIENTE ID",
    "ClientSecret": "SEU CLIENTE SECRET",
    "TenantId": "SEU TENANT ID",
    "Scope": "https://graph.microsoft.com/.default"
  },
  "JwtSettings": {
    "Secret": "sua-chave-secreta-super-segura-aqui",
    "Issuer": "EduGraphScheduler",
    "Audience": "EduGraphUsers",
    "ExpiresInMinutes": 60
  }
}
5. Execute a aplicação
bash
cd src/EduGraphScheduler.API
dotnet run
A API estará disponível em:

API: https://localhost:5000

Swagger UI: https://localhost:5000/swagger

Hangfire Dashboard: https://localhost:5000/hangfire

📡 Endpoints da API
🔐 Autenticação
Método	Endpoint	Descrição
POST	/api/auth/login	Autenticar usuário
POST	/api/auth/register	Registrar novo usuário
POST	/api/auth/validate	Validar token JWT
👥 Usuários
Método	Endpoint	Descrição
GET	/api/users	Listar todos os usuários
GET	/api/users/{id}	Obter usuário por ID
GET	/api/users/search?query=...	Buscar usuários
📅 Eventos
Método	Endpoint	Descrição
GET	/api/events/user/{userId}	Obter eventos do usuário
POST	/api/events/sync/user/{userId}	Sincronizar eventos do usuário
POST	/api/events/sync/all	Sincronizar todos os eventos
🔄 Sincronização
Método	Endpoint	Descrição
POST	/api/sync/start	Iniciar sincronização completa
POST	/api/sync/users	Sincronizar apenas usuários
POST	/api/sync/schedule	Agendar sincronização recorrente
🔧 Configuração Avançada
Hangfire Jobs
Jobs recorrentes configurados automaticamente:

sync-users-recurring: Sincroniza usuários a cada 6 horas

sync-events-recurring: Sincroniza eventos a cada 12 horas

maintenance-cleanup: Limpeza diária à meia-noite

Microsoft Graph Integration
csharp
public class MicrosoftGraphService : IMicrosoftGraphService
{
    public async Task<IEnumerable<MicrosoftGraphUser>> GetUsersAsync()
    public async Task<IEnumerable<MicrosoftGraphEvent>> GetUserEventsAsync(string userPrincipalName)
    public async Task<bool> UserHasEventsAsync(string userPrincipalName) // Verificação inteligente
}
Sincronização Inteligente
csharp
public async Task SyncAllUsersEventsAsync()
{
    // Verifica primeiro se o usuário tem eventos
    var hasEvents = await _microsoftGraphService.UserHasEventsAsync(userPrincipalName);
    if (hasEvents)
    {
        await SyncUserEventsAsync(userId); // Só sincroniza se tiver eventos
    }
}
🧪 Executando Testes
bash
# Executar todos os testes
dotnet test

# Executar testes com detalhes
dotnet test --verbosity normal

# Executar testes específicos
cd src/EduGraphScheduler.Tests
dotnet test

# Testes com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
📊 Migrations
bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration --startup-project ../EduGraphScheduler.API

# Aplicar migrations
dotnet ef database update --startup-project ../EduGraphScheduler.API

# Remover última migration
dotnet ef migrations remove --startup-project ../EduGraphScheduler.API
🐳 Docker (Opcional)
dockerfile
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
🔍 Monitoramento
Hangfire Dashboard
Acesse /hangfire para monitorar jobs em execução:

Status dos jobs recorrentes

Histórico de execuções

Logs de erro

Estatísticas de performance


🚨 Solução de Problemas
Erro: "Cannot connect to SQL Server"
bash
# Iniciar LocalDB
sqllocaldb start MSSQLLocalDB

# Verificar instâncias
sqllocaldb info
Erro: "Microsoft Graph authentication failed"
Verifique ClientId, ClientSecret e TenantId

Confirme as permissões no Azure AD

Valide o scope "https://graph.microsoft.com/.default"

Erro: "JWT token invalid"
Verifique o secret no JwtSettings

Confirme issuer e audience

Valide o tempo de expiração

Rate Limiting do Microsoft Graph
A aplicação inclui tratamento automático para rate limits:

Pausas entre requisições

Retry automático

Processamento em lotes

📞 Suporte
Desenvolvedor: André Marestoni
Email: m.marestoni@gmail.com
