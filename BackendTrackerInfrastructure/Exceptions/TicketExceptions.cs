namespace BackendTrackerInfrastructure.Exceptions;

public class TicketExceptions : Exception
{
    public TicketExceptions() : base("Ticket not found")
    {
    }

    public TicketExceptions(string message) : base(message)
    {
    }
}