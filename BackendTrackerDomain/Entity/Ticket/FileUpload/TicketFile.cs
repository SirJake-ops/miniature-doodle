namespace BackendTrackerDomain.Entity.Ticket.FileUpload;

public class TicketFile : BaseEntity {
    public int Id { get; set; }
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public required byte[] Content { get; set; }
}