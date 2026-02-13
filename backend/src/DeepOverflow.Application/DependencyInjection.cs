using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DeepOverflow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR (CQRS handlers)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}

