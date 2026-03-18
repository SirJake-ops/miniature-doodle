using System.Diagnostics;
using BackendTracker.Entities.Message;
using BackendTrackerDomain.Entity.ApplicationUser;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Entity.Ticket.FileUpload;
using Microsoft.EntityFrameworkCore;

namespace BackendTrackerInfrastructure.Persistence.Context;


public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
#pragma warning restore IL3050

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
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
                var username = FirstEnv("POSTGRES_USERNAME_DEV", "POSTGRES_USER_DEV", "POSTGRES_USER") ?? "postgres";
                var password = FirstEnv("POSTGRES_PASSWORD_DEV", "POSTGRES_PASSWORD");
                optionsBuilder.UseNpgsql(
                    $"Host={pgHost};Port={pgPort};Database={pgDb};Username={username};Password={password}");
            }
        }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BackendTrackerDomain.Entity.Ticket.Ticket>()
            .HasOne(t => t.Submitter)
            .WithMany(u => u.SubmittedTickets)
            .HasForeignKey(t => t.SubmitterId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<BackendTrackerDomain.Entity.Ticket.Ticket>()
            .HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<BackendTrackerDomain.Entity.Ticket.Ticket> Tickets { get; set; }
    public DbSet<TicketFile> TicketFiles { get; set; }
}
