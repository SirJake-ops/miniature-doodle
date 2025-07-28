using BackendTrackerDomain.Entity.Message;

namespace BackendTrackerApplication.Services.Messaging;

public class MessageService : IMessageService
{
    public Task ConnectAsync(string url)
    {
        throw new NotImplementedException();
    }

    public Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }

    public Task SendAsync(Message message)
    {
        throw new NotImplementedException();
    }
}