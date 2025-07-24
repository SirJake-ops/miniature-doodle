namespace BackendTrackerPresentation.Exceptions;

public class MessageNotSentException : Exception
{
   public MessageNotSentException() : base("Message could not be sent.")
   {
   } 
   
   public MessageNotSentException(string message) : base(message)
   {
   }
}