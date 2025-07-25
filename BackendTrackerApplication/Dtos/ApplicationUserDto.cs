namespace BackendTrackerApplication.Dtos;

public class ApplicationUserDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
}