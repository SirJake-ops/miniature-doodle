using BackendTracker.Entities;

namespace BackendTrackerDomain.Entity.Ticket.FileUpload;

public class TicketFile : BaseEntity {
    public int Id { get; set; }
    public Guid TicketId { get; set; }
    public BackendTrackerDomain.Entity.Ticket.Ticket Ticket { get; set; } = null!;
    public required byte[] Content { get; set; }
}