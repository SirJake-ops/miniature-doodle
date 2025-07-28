using BackendTrackerDomain.Entity.Ticket;

namespace BackendTrackerApplication.Services.Messaging;

public interface INotificationService
{
    Task NotifyTicketCreated(Ticket ticket);
    Task NotifyTicketUpdated(Ticket ticket);
    Task NotifyTicketAssigned(Ticket ticket, Guid assigneeId);
}