namespace JobApplicationHelper.Exceptions;

public sealed class BackgroundJobNotificationException : Exception
{
    public BackgroundJobNotificationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}