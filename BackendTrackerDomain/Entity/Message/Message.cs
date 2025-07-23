using System.ComponentModel.DataAnnotations;

namespace BackendTracker.Entities.Message;

public class Message : BaseEntity
{
   [Key] public Guid Id { get; set; }
   [Required] public Guid SenderId { get; set; }
   [Required] public Guid ReceiverId { get; set; }
   [Required][MaxLength(250)] public required string Content { get; set; }
   [Required] public DateTime SentTime { get; set; }
   [Required] public bool IsRead { get; set; }
}