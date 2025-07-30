using Microsoft.AspNetCore.SignalR;

namespace BackendTrackerApplication.Services.Messaging;

public class MessageHub : Hub
{
   public override async Task OnConnectedAsync()
   {
      await Groups.AddToGroupAsync(Context.ConnectionId, "AllUsers");
      await base.OnConnectedAsync();
   }
   
   public override async Task OnDisconnectedAsync(Exception? exception)
   {
      await base.OnDisconnectedAsync(exception);
   }

   public async Task JoinTicketRoom(Guid ticketId)
   {
      await Groups.AddToGroupAsync(Context.ConnectionId, $"Ticket_{ticketId}");
   }
   
   public async Task SendMessageToTicketRoom(Guid ticketId, string message)
   {
      await Clients.Group($"Ticket_{ticketId}").SendAsync("ReceiveMessage", Context.UserIdentifier, message);
   }
}