namespace BackendTrackerApplication.Dtos;

public class MessageDto
{
    public  string? Content { get; set; }
    public DateTime SentTime { get; set; }
    public bool IsRead { get; set; }
}