namespace JobApplicationHelper.Services.Api.Exceptions;

public sealed class DraftNotFoundException(string message) : Exception(message)
{
}