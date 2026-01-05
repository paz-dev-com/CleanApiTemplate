using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CleanApiTemplate.Application;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR for CQRS pattern
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Add Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.TransactionBehavior<,>));

        return services;
    }
}
