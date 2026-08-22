using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SupportOps.Infrastructure.Persistence;

public sealed class SupportOpsDbContextFactory
    : IDesignTimeDbContextFactory<SupportOpsDbContext>
{
    public SupportOpsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<SupportOpsDbContext>();

        const string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;" +
            "Database=SupportOpsDb;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new SupportOpsDbContext(
            optionsBuilder.Options
        );
    }
}