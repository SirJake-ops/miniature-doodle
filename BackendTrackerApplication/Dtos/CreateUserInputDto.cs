using BackendTracker.Entities.Message;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Entity.Ticket;

namespace BackendTrackerApplication.Dtos;

public class CreateUserInput
{
    
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; } = "User"; 
    public List<Message> Messages { get; set; } = new List<Message>();
    public List<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Ticket> SubmittedTickets { get; set; } = new List<Ticket>();
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();
}