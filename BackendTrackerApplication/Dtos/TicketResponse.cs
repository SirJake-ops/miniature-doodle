namespace BackendTracker.Ticket.NewFolder;

public class TicketResponse
{
    public Guid TicketId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StepsToReproduce { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public Guid SubmitterId { get; set; }
    public Guid AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set;}
}