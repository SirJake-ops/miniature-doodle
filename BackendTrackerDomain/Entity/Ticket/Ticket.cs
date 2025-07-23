using BackendTracker.Entities;
using BackendTrackerDomain.Entity.Ticket.FileUpload;
using System.ComponentModel.DataAnnotations.Schema;
using Environment = BackendTracker.Ticket.Enums.Environment;

namespace BackendTrackerDomain.Entity.Ticket;

public class Ticket : BaseEntity
{
    public required Guid TicketId { get; set; }
    [ForeignKey(nameof(Submitter))] public required Guid SubmitterId { get; set; }
    public required ApplicationUser.ApplicationUser Submitter { get; set; } = null!;

    public required ApplicationUser.ApplicationUser? Assignee { get; set; } = null!;
    [ForeignKey(nameof(Assignee))] public required Guid? AssigneeId { get; set; }

    public required Environment Environment { get; set; }

    public required string Title { get; set; } = string.Empty;

    public required string Description { get; set; } = string.Empty;
    public required string StepsToReproduce { get; set; } = string.Empty;

    public required string ExpectedResult { get; set; } = string.Empty;
    public required List<TicketFile> Files { get; set; } = new();
    public required bool IsResolved { get; set; } = false;
}