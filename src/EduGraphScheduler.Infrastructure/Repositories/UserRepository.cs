using EduGraphScheduler.Domain.Entities;
using EduGraphScheduler.Domain.Interfaces;
using EduGraphScheduler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EduGraphScheduler.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByMicrosoftGraphIdAsync(string microsoftGraphId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.MicrosoftGraphId == microsoftGraphId);
    }

    public async Task<IEnumerable<User>> GetUsersWithEventsAsync()
    {
        return await _context.Users
            .Include(u => u.CalendarEvents)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<User?> GetUserWithEventsAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.CalendarEvents
                .OrderBy(e => e.Start)
                .ThenBy(e => e.End))
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> UserExistsAsync(string microsoftGraphId)
    {
        return await _context.Users
            .AnyAsync(u => u.MicrosoftGraphId == microsoftGraphId);
    }

    public async Task BulkUpsertAsync(IEnumerable<User> users)
    {
        var userList = users.ToList();

        if (!userList.Any())
            return;

        // Para poucos registros, use EF Core
        if (userList.Count <= 100)
        {
            await BulkUpsertEfCoreAsync(userList);
        }
        else
        {
            // Para muitos registros, use MERGE SQL
            await BulkUpsertMergeAsync(userList);
        }
    }

    private async Task BulkUpsertEfCoreAsync(List<User> users)
    {
        foreach (var user in users)
        {
            var existingUser = await GetByMicrosoftGraphIdAsync(user.MicrosoftGraphId);

            if (existingUser != null)
            {
                // Update existing user
                existingUser.DisplayName = user.DisplayName;
                existingUser.GivenName = user.GivenName;
                existingUser.Surname = user.Surname;
                existingUser.Mail = user.Mail;
                existingUser.JobTitle = user.JobTitle;
                existingUser.Department = user.Department;
                existingUser.OfficeLocation = user.OfficeLocation;
                existingUser.LastSyncedAt = DateTime.UtcNow;

                _context.Users.Update(existingUser);
            }
            else
            {
                // Add new user
                user.CreatedAt = DateTime.UtcNow;
                await _context.Users.AddAsync(user);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task BulkUpsertMergeAsync(List<User> users)
    {
        var connection = _context.Database.GetDbConnection() as SqlConnection;

        if (connection == null)
            throw new InvalidOperationException("SQL Connection not available");

        try
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            // Criar DataTable
            var dataTable = CreateUserDataTable(users);

            // Criar tabela temporária
            await CreateTempTableAsync(connection);

            // Bulk copy para tabela temporária
            await BulkCopyToTempTableAsync(connection, dataTable);

            // Executar MERGE
            await ExecuteMergeStatementAsync(connection);

            // Limpar tabela temporária
            //await DropTempTableAsync(connection);
        }
        catch (Exception ex)
        {
            // Fallback para EF Core em caso de erro
            Console.WriteLine($"MERGE failed, falling back to EF Core: {ex.Message}");
            await BulkUpsertEfCoreAsync(users);
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    private DataTable CreateUserDataTable(List<User> users)
    {
        var table = new DataTable();

        // Definir colunas (mesmo schema da tabela Users)
        table.Columns.Add("Id", typeof(Guid));
        table.Columns.Add("MicrosoftGraphId", typeof(string));
        table.Columns.Add("DisplayName", typeof(string));
        table.Columns.Add("GivenName", typeof(string));
        table.Columns.Add("Surname", typeof(string));
        table.Columns.Add("Mail", typeof(string));
        table.Columns.Add("UserPrincipalName", typeof(string));
        table.Columns.Add("JobTitle", typeof(string));
        table.Columns.Add("Department", typeof(string));
        table.Columns.Add("OfficeLocation", typeof(string));
        table.Columns.Add("LastSyncedAt", typeof(DateTime));
        table.Columns.Add("CreatedAt", typeof(DateTime));
        table.Columns.Add("PasswordHash", typeof(string));

        // Preencher dados
        foreach (var user in users)
        {
            table.Rows.Add(
                user.Id != Guid.Empty ? user.Id : Guid.NewGuid(),
                user.MicrosoftGraphId ?? string.Empty,
                user.DisplayName ?? string.Empty,
                user.GivenName ?? string.Empty,
                user.Surname ?? string.Empty,
                user.Mail ?? string.Empty,
                user.UserPrincipalName ?? string.Empty,
                user.JobTitle ?? string.Empty,
                user.Department ?? string.Empty,
                user.OfficeLocation ?? string.Empty,
                user.LastSyncedAt,
                user.CreatedAt != DateTime.MinValue ? user.CreatedAt : DateTime.UtcNow,
                user.PasswordHash ?? string.Empty
            );
        }

        return table;
    }

    private async Task CreateTempTableAsync(SqlConnection connection)
    {
        var createTempTableSql = @"
            CREATE TABLE #TempUsers (
                Id UNIQUEIDENTIFIER,
                MicrosoftGraphId NVARCHAR(200),
                DisplayName NVARCHAR(255),
                GivenName NVARCHAR(100),
                Surname NVARCHAR(200),
                Mail NVARCHAR(255),
                UserPrincipalName NVARCHAR(200),
                JobTitle NVARCHAR(100),
                Department NVARCHAR(100),
                OfficeLocation NVARCHAR(100),
                LastSyncedAt DATETIME2,
                CreatedAt DATETIME2,
                PasswordHash NVARCHAR(200)
            )";

        using var command = new SqlCommand(createTempTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task BulkCopyToTempTableAsync(SqlConnection connection, DataTable dataTable)
    {
        using var bulkCopy = new SqlBulkCopy(connection);

        bulkCopy.DestinationTableName = "#TempUsers";
        bulkCopy.BatchSize = 5000; // Otimizado para grandes volumes
        bulkCopy.BulkCopyTimeout = 120; // 2 minutos

        // Mapear colunas
        bulkCopy.ColumnMappings.Add("Id", "Id");
        bulkCopy.ColumnMappings.Add("MicrosoftGraphId", "MicrosoftGraphId");
        bulkCopy.ColumnMappings.Add("DisplayName", "DisplayName");
        bulkCopy.ColumnMappings.Add("GivenName", "GivenName");
        bulkCopy.ColumnMappings.Add("Surname", "Surname");
        bulkCopy.ColumnMappings.Add("Mail", "Mail");
        bulkCopy.ColumnMappings.Add("UserPrincipalName", "UserPrincipalName");
        bulkCopy.ColumnMappings.Add("JobTitle", "JobTitle");
        bulkCopy.ColumnMappings.Add("Department", "Department");
        bulkCopy.ColumnMappings.Add("OfficeLocation", "OfficeLocation");
        bulkCopy.ColumnMappings.Add("LastSyncedAt", "LastSyncedAt");
        bulkCopy.ColumnMappings.Add("CreatedAt", "CreatedAt");
        bulkCopy.ColumnMappings.Add("PasswordHash", "PasswordHash");

        await bulkCopy.WriteToServerAsync(dataTable);
    }

    private async Task ExecuteMergeStatementAsync(SqlConnection connection)
    {
        // 🔥 SOLUÇÃO COMPLETA: Criar temp table sem duplicatas desde o início
        var createCleanTempTableSql = @"
        DROP TABLE IF EXISTS #CleanTempUsers;
        
        SELECT 
            Id, MicrosoftGraphId, DisplayName, GivenName, Surname, 
            Mail, UserPrincipalName, JobTitle, Department, OfficeLocation, 
            LastSyncedAt, CreatedAt, PasswordHash
        INTO #CleanTempUsers
        FROM (
            SELECT *,
                ROW_NUMBER() OVER (PARTITION BY MicrosoftGraphId ORDER BY LastSyncedAt DESC) as rn
            FROM #TempUsers
        ) AS Ranked
        WHERE rn = 1";

        using var cleanupCommand = new SqlCommand(createCleanTempTableSql, connection);
        await cleanupCommand.ExecuteNonQueryAsync();
        Console.WriteLine("✅ Created clean temp table without duplicates");

        // 🔥 MERGE usando a temp table limpa
        var mergeSql = @"
        MERGE INTO Users AS Target
        USING #CleanTempUsers AS Source
        ON Target.MicrosoftGraphId = Source.MicrosoftGraphId
        
        WHEN MATCHED THEN
            UPDATE SET 
                Target.DisplayName = Source.DisplayName,
                Target.GivenName = Source.GivenName,
                Target.Surname = Source.Surname,
                Target.Mail = Source.Mail,
                Target.UserPrincipalName = Source.UserPrincipalName,
                Target.JobTitle = Source.JobTitle,
                Target.Department = Source.Department,
                Target.OfficeLocation = Source.OfficeLocation,
                Target.LastSyncedAt = Source.LastSyncedAt,
                Target.PasswordHash = Source.PasswordHash
        
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                Id, MicrosoftGraphId, DisplayName, GivenName, Surname, 
                Mail, UserPrincipalName, JobTitle, Department, OfficeLocation, 
                LastSyncedAt, CreatedAt, PasswordHash
            )
            VALUES (
                Source.Id, Source.MicrosoftGraphId, Source.DisplayName, Source.GivenName, Source.Surname,
                Source.Mail, Source.UserPrincipalName, Source.JobTitle, Source.Department, Source.OfficeLocation,
                Source.LastSyncedAt, Source.CreatedAt, Source.PasswordHash
            );";

        using var command = new SqlCommand(mergeSql, connection);

        try
        {
            var affectedRows = await command.ExecuteNonQueryAsync();
            Console.WriteLine($"✅ MERGE completed successfully. Affected rows: {affectedRows}");

            // Limpar temp tables
            await DropTempTableAsync(connection, "#TempUsers");
            await DropTempTableAsync(connection, "#CleanTempUsers");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MERGE failed: {ex.Message}");

            // Debug detalhado
            await DebugDetailed(connection);
            throw;
        }
    }

    private async Task DebugDetailed(SqlConnection connection)
    {
        Console.WriteLine("🔍 DETAILED DEBUG:");

        // Verificar temp tables
        var checkTempSql = "SELECT COUNT(*) as Count FROM #TempUsers";
        using var tempCmd = new SqlCommand(checkTempSql, connection);
        var tempCount = await tempCmd.ExecuteScalarAsync();
        Console.WriteLine($"Temp table records: {tempCount}");

        // Verificar duplicatas específicas
        var dupSql = @"
        SELECT MicrosoftGraphId, COUNT(*) as Count 
        FROM #TempUsers 
        WHERE MicrosoftGraphId = '0550ef3f-fcba-435b-bb0c-631265cbe206'
        GROUP BY MicrosoftGraphId";

        using var dupCmd = new SqlCommand(dupSql, connection);
        using var reader = await dupCmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            Console.WriteLine($"🚨 PROBLEMATIC ID: {reader["MicrosoftGraphId"]} appears {reader["Count"]} times");
        }
        await reader.CloseAsync();
    }

    private async Task DropTempTableAsync(SqlConnection connection, string tableName)
    {
        try
        {
            var dropSql = $"DROP TABLE IF EXISTS {tableName}";
            using var command = new SqlCommand(dropSql, connection);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not drop {tableName}: {ex.Message}");
        }
    }

    private async Task DropTempTableAsync(SqlConnection connection)
    {
        var dropSql = "DROP TABLE #TempUsers";
        using var command = new SqlCommand(dropSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<User>> GetUsersWhoHaveEventsAsync()
    {
        return await _context.Users
            .Where(u => u.CalendarEvents.Any())
            .AsNoTracking()
            .ToListAsync();
    }
}