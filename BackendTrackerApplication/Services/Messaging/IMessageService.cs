using BackendTrackerApplication.Dtos;

namespace BackendTrackerApplication.Services.Messaging;

public interface IMessageService
{
    Task NotifyTicketCreated(BackendTrackerDomain.Entity.Ticket.Ticket ticket);
    Task NotifyTicketUpdated(BackendTrackerDomain.Entity.Ticket.Ticket ticket);
    Task NotifyTicketAssigned(BackendTrackerDomain.Entity.Ticket.Ticket ticket, Guid assigneeId);
    Task SendAsync(MessageDto message);
}