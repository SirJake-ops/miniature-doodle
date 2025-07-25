namespace BackendTrackerInfrastructure.Exceptions;

public class AuthenticationExceptions : Exception
{
   public AuthenticationExceptions() : base("Exception thrown in Authentication") 
   {
   }
   
   public AuthenticationExceptions(string message) : base(message) 
   {
   }
}