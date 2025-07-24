namespace BackendTrackerPresentation.Exceptions;

public class UserNotFoundGraphql : Exception
{
   public UserNotFoundGraphql() : base("User not found")
   {
   }
   
   public UserNotFoundGraphql(string message) : base(message)
   {
   } 
}