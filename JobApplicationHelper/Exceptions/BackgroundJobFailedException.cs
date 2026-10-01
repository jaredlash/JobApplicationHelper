namespace JobApplicationHelper.Exceptions;

public sealed class BackgroundJobFailedException(string? message) : Exception(message ?? "The background job failed.")
{
}