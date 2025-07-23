using System.ComponentModel.DataAnnotations;

namespace BackendTracker.Entities.Message;

public class Conversation
{
    [Key] public Guid Id { get; set; }
    [Required] public Guid InitialSenderId { get; set; }
    [Required] public Guid InitialReceiverId { get; set; }
    public DateTime LastMessageTime { get; set; }
}