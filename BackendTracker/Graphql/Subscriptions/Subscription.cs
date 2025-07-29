using BackendTracker.Entities.Message;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Entity.Ticket;

namespace BackendTrackerPresentation.Graphql.Subscriptions;

public abstract class Subscription
{
    [Subscribe]
    public Message MessageAdded([EventMessage] Message message) => message;
    
    [Subscribe]
    public Conversation ConversationAdded([EventMessage] Conversation conversation) => conversation;
    
    [Subscribe]
    public Ticket TicketCreated([EventMessage] Ticket ticket) => ticket;
}