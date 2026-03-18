using System.Text;
using BackendTracker.Entities.Message;
using BackendTrackerApplication.Interfaces;
using BackendTrackerApplication.Mapping.MappingProfiles;
using BackendTrackerApplication.Services;
using BackendTrackerApplication.Services.Messaging;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Interfaces;
using BackendTrackerInfrastructure.Authentication;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerInfrastructure.Repositories;
using BackendTrackerPresentation.Graphql;
using BackendTrackerPresentation.Graphql.Subscriptions;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Environment = System.Environment;
using BackendTrackerDomain.Entity.ApplicationUser;

namespace BackendTrackerPresentation;

public class Program
{
    public static async Task Main(string[] args)
    {
        Env.TraversePath().Load();
        var builder = WebApplication.CreateSlimBuilder(args);

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
        if (string.IsNullOrWhiteSpace(pgPass))
            throw new InvalidOperationException(
                "Missing Postgres password. Set POSTGRES_PASSWORD_DEV (or POSTGRES_PASSWORD) in your .env / environment.");

        var connectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";

        builder.Services.AddPooledDbContextFactory<ApplicationContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddType<Message>()
            .AddType<Conversation>()
            .AddSubscriptionType<Subscription>();

        builder.Services.AddScoped<ITicketRepository, TicketRepository>();
        builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
        builder.Services.AddScoped<TicketService>();
        builder.Services.AddScoped<Mutation>();
        builder.Services.AddScoped<Query>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "AllowedOrigins",
                policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:1420",
                            "http://127.0.0.1:1420",
                            "https://localhost:1420",
                            "https://127.0.0.1:1420"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddControllers().AddNewtonsoftJson();
        builder.Services.AddAuthorization();
        builder.Services.AddAutoMapper(config =>
        {
            config.AddProfile(new ApplicationUserMappingProfile());
            config.AddProfile(new TicketMappingProfile());
        });


        var app = builder.Build();

        app.UseCors("AllowedOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        await EnsureDatabaseSeeded(app.Services);
        app.MapControllers();
        app.MapGraphQL();
        app.MapHub<MessageHub>("/messageHub");

        app.Run();
    }

    private static async Task EnsureDatabaseSeeded(IServiceProvider services)
    {
        try
        {
            using var scope = services.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationContext>>();
            await using var context = await factory.CreateDbContextAsync();

            await context.Database.EnsureCreatedAsync();

            if (await context.ApplicationUsers.AnyAsync()) return;

            var hasher = new PasswordHasher<ApplicationUser>();

            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = "admin@demo.local",
                Role = "Admin",
                Messages = new List<Message>(),
                Conversations = new List<Conversation>(),
            };
            admin.Password = hasher.HashPassword(admin, "Password123!");

            var doctor = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "doctor",
                Email = "doctor@demo.local",
                Role = "Doctor",
                Messages = new List<Message>(),
                Conversations = new List<Conversation>(),
            };
            doctor.Password = hasher.HashPassword(doctor, "Password123!");

            var nurse = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "nurse",
                Email = "nurse@demo.local",
                Role = "Nurse",
                Messages = new List<Message>(),
                Conversations = new List<Conversation>(),
            };
            nurse.Password = hasher.HashPassword(nurse, "Password123!");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user",
                Email = "user@demo.local",
                Role = "User",
                Messages = new List<Message>(),
                Conversations = new List<Conversation>(),
            };
            user.Password = hasher.HashPassword(user, "Password123!");

            context.ApplicationUsers.AddRange(admin, doctor, nurse, user);

            var t1 = new BackendTrackerDomain.Entity.Ticket.Ticket
            {
                TicketId = Guid.NewGuid(),
                SubmitterId = user.Id,
                AssigneeId = admin.Id,
                Environment = BackendTracker.Ticket.Enums.Environment.Browser,
                Title = "Login page CORS error",
                Description = "Browser blocks request due to missing CORS headers.",
                StepsToReproduce = "Open app and submit login form.",
                ExpectedResult = "Login succeeds and navigates to dashboard.",
                Files = new List<BackendTrackerDomain.Entity.Ticket.FileUpload.TicketFile>(),
                IsResolved = false,
            };

            var t2 = new BackendTrackerDomain.Entity.Ticket.Ticket
            {
                TicketId = Guid.NewGuid(),
                SubmitterId = nurse.Id,
                AssigneeId = doctor.Id,
                Environment = BackendTracker.Ticket.Enums.Environment.OperatingSystem,
                Title = "Patient ticket workflow mock",
                Description = "Seeded ticket representing a clinician workflow item.",
                StepsToReproduce = "Open clinician dashboard and click a row.",
                ExpectedResult = "Modal opens and View Details redirects.",
                Files = new List<BackendTrackerDomain.Entity.Ticket.FileUpload.TicketFile>(),
                IsResolved = false,
            };

            context.Tickets.AddRange(t1, t2);

            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }
}
