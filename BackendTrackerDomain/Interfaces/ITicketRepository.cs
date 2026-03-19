using BackendTrackerDomain.Entity.Ticket;

namespace BackendTrackerDomain.Interfaces;

public interface ITicketRepository
{
    Task<IEnumerable<Ticket>> GetAllTickets();
    Task<IEnumerable<Ticket>> GetTickets(Guid submitterId);
    Task<IEnumerable<Ticket>> GetTicketsForUser(Guid userId);
    Task<Ticket?> GetTicketById(Guid ticketId);
    Task<Ticket> CreateTicket(Ticket ticket);
    Task<Ticket> UpdateTicket(Ticket ticket);
    Task<Ticket> DeleteAsync(Guid ticketId);
    Task<bool> UserExistsAsync(Guid userId);
    Task<Ticket> AssignTicketToUser(Guid userId, Guid ticketId);
}
