using BackendTrackerDomain.Entity.Ticket;

namespace BackendTrackerApplication.Services.Messaging;

public class NotificationService : INotificationService
{
    public Task NotifyTicketCreated(Ticket ticket)
    {
        throw new NotImplementedException();
    }

    public Task NotifyTicketUpdated(Ticket ticket)
    {
        throw new NotImplementedException();
    }

    public Task NotifyTicketAssigned(Ticket ticket, Guid assigneeId)
    {
        throw new NotImplementedException();
    }
}