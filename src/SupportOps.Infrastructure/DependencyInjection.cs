using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Application.Abstractions.Security;
using SupportOps.Infrastructure.Persistence;
using SupportOps.Infrastructure.Persistence.Repositories;
using SupportOps.Infrastructure.Security;

namespace SupportOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found."
            );

        services.AddDbContext<SupportOpsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<
            ITicketRepository,
            TicketRepository>();

        services.AddScoped<
            ITicketHistoryRepository,
            TicketHistoryRepository>();
        services.AddScoped<
                ITicketCommentRepository,
                TicketCommentRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<SupportOpsDbContext>());

        services.AddScoped<
            IPasswordHasher,
            IdentityPasswordHasher>();
        services.AddScoped<
            IJwtTokenGenerator,
            JwtTokenGenerator>();

        return services;
    }
}