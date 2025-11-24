using Microsoft.Extensions.DependencyInjection;

namespace EduGraphScheduler.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services will be registered in Program.cs
        // This method is for future extension if needed
        return services;
    }
}