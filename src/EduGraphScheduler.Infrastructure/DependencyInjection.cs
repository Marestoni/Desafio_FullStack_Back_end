using EduGraphScheduler.Application;
using EduGraphScheduler.Application.Interfaces;
using EduGraphScheduler.Domain.Interfaces;
using EduGraphScheduler.Infrastructure.Data;
using EduGraphScheduler.Infrastructure.Repositories;
using EduGraphScheduler.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduGraphScheduler.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();

        services.AddScoped<IMicrosoftGraphService, MicrosoftGraphService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.Configure<MicrosoftGraphSettings>(configuration.GetSection("MicrosoftGraph"));
        services.Configure<SyncSettings>(configuration.GetSection("SyncSettings"));

        return services;
    }
}