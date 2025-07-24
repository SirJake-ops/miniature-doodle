using System.Text;
using BackendTracker.Entities.Message;
using BackendTrackerApplication.Services;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Interfaces;
using BackendTrackerInfrastructure.Persistence.Context;
using BackendTrackerInfrastructure.Repositories;
using BackendTrackerPresentation.Graphql;
using BackendTrackerPresentation.Graphql.Subscriptions;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Environment = System.Environment;

namespace BackendTrackerPresentation;

public class Program
{
    public static void Main(string[] args)
    {
        Env.Load();
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.AddPooledDbContextFactory<ApplicationContext>(options =>
            options.UseNpgsql(
                $"Host=localhost;Port=5432;Database={Environment.GetEnvironmentVariable("POSTGRES_DB_NAME_DEV")};Username={Environment.GetEnvironmentVariable("POSTGRES_USERNAME_DEV")};Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD_DEV")}"));

        builder.Services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddType<Message>()
            .AddType<Conversation>()
            .AddSubscriptionType<Subscription>();

        builder.Services.AddScoped<ITicketRepository, TicketRepository>();
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

        builder.Services.AddControllers();
        builder.Services.AddControllers().AddNewtonsoftJson();
        builder.Services.AddAuthorization();


        var app = builder.Build();

        app.MapControllers();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGraphQL();

        app.Run();
    }
}