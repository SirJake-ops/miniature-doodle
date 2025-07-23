namespace BackendTrackerInfrastructure.Exceptions;

public class ApplicationUserExceptions : Exception
{
    public ApplicationUserExceptions() : base("Application user not found")
    {
    }

    public ApplicationUserExceptions(string message) : base(message)
    {
    }
}