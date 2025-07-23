using BackendTrackerDomain.Entity.Ticket.FileUpload;
using Environment = BackendTracker.Ticket.Enums.Environment;

namespace BackendTrackerApplication.DTOs;

public class TicketRequestBody
{
    public required Guid SubmitterId { get; set; }
    public required Environment Environment { get; set; } = Environment.Browser;

    public required string Title { get; set; } = string.Empty;

    public required string Description { get; set; } = string.Empty;
    public required string StepsToReproduce { get; set; } = string.Empty;

    public required string ExpectedResult { get; set; } = string.Empty;
    public required List<TicketFile> Files { get; set; } = new();
    public bool IsResolved { get; set; }
}