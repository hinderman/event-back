using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Common.Security;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(ApplicationAssembly.Reference));
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IUserAccessService, UserAccessService>();

        return services;
    }
}
