using Microsoft.Extensions.DependencyInjection;

namespace EduGraphScheduler.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}