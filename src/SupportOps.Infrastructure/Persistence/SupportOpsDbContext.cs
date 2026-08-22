using Microsoft.EntityFrameworkCore;
using SupportOps.Domain.Entities;
using SupportOps.Application.Abstractions.Persistence;

namespace SupportOps.Infrastructure.Persistence;

public class SupportOpsDbContext : DbContext, IUnitOfWork
{
    public SupportOpsDbContext(
        DbContextOptions<SupportOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    public DbSet<TicketHistory> TicketHistory => Set<TicketHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SupportOpsDbContext).Assembly
        );
    }
}