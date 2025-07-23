using System.Diagnostics;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendTrackerInfrastructure.Persistence.Context;

public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args)
    {
        Env.Load();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

        // var username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME_DEV");
        var username = "postgres";
        // var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD_DEV");
        var password = "postgres";
        optionsBuilder.UseNpgsql($"Host=localhost;Port=5432;Database=tracker;Username={username};Password={password}");

        return new ApplicationContext(optionsBuilder.Options);
    }
}