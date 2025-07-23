using System.ComponentModel.DataAnnotations;
using BackendTracker.Entities;
using BackendTracker.Entities.Message;

namespace BackendTrackerDomain.Entity.ApplicationUser;

public class ApplicationUser : BaseEntity
{
    [Key] public Guid Id { get; set; }

    [Required] [MaxLength(50)] public string? UserName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [MaxLength(50)] public string? Role { get; set; }

    public required List<BackendTracker.Entities.Message.Message> Messages { get; set; } = new List<Message>();

    public required List<Conversation> Conversations { get; set; } = new List<Conversation>();

    public ICollection<Ticket.Ticket> SubmittedTickets { get; set; } = new List<Ticket.Ticket>();
    public ICollection<Ticket.Ticket> AssignedTickets { get; set; } = new List<Ticket.Ticket>();


    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    public DateTime LastLoginTime { get; set; }
    public bool IsOnline { get; set; }
}