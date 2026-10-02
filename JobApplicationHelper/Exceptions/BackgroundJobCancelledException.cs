namespace JobApplicationHelper.Exceptions;

public sealed class BackgroundJobCancelledException(string? message = null) : Exception(message ?? "The background job was cancelled.")
{
}