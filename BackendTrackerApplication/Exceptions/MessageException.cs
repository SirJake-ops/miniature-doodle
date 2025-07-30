namespace BackendTrackerApplication.Exceptions;

public class MessageException : Exception
{
   public MessageException() : base("An error occurred while processing the message.") 
   {
   }
   
   public MessageException(string message) : base(message) 
   {
   }
}