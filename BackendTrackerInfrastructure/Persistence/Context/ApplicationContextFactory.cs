using System.Diagnostics;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendTrackerInfrastructure.Persistence.Context;

    public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            Env.TraversePath().Load();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

            static string? FirstEnv(params string[] keys)
            {
                foreach (var k in keys)
                {
                    var v = Environment.GetEnvironmentVariable(k);
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
                return null;
            }

            var pgHost = FirstEnv("POSTGRES_HOST_DEV", "POSTGRES_HOST") ?? "localhost";
            var pgPort = FirstEnv("POSTGRES_PORT_DEV", "POSTGRES_PORT") ?? "5432";
            var pgDb = FirstEnv("POSTGRES_DB_NAME_DEV", "POSTGRES_DB_NAME", "POSTGRES_DB") ?? "tracker";
            var pgUser = FirstEnv("POSTGRES_USERNAME_DEV", "POSTGRES_USER_DEV", "POSTGRES_USER") ?? "postgres";
            var pgPass = FirstEnv("POSTGRES_PASSWORD_DEV", "POSTGRES_PASSWORD");

            optionsBuilder.UseNpgsql(
                $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}");

            return new ApplicationContext(optionsBuilder.Options);
        }
    }
