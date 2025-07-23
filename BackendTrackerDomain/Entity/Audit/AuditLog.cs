using System.ComponentModel.DataAnnotations;

namespace BackendTracker.Entities.Audit;

public class AuditLog : BaseEntity
{
    [Key] public Guid Id { get; set; }
    
    [Required] public Guid UserId { get; set; }
    [Required] public required string Action { get; set; }
    [Required] public DateTime TimeStamp { get; set; }
}