using BackendTrackerDomain.Entity.Message;

namespace BackendTrackerApplication.Services.Messaging;

public interface IMessageService
{
    Task ConnectAsync(string url);
    Task DisconnectAsync();
    Task SendAsync(Message message);
}