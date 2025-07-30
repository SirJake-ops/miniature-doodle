namespace BackendTrackerApplication.Exceptions;

public class NotificationException : Exception
{
   public NotificationException() : base("An error occurred while processing the notification.") 
   {
   }
   
   public NotificationException(string message) : base(message) 
   {
   }
}