namespace BackendTrackerApplication.Dtos;

public class DeleteMessageDto
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public DateTime SentTime { get; set; }
}