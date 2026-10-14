namespace JobApplicationHelper.Exceptions;

public class ImportExperiencesException(string? message = null) : Exception(message ?? "Error attempting to import experiences")
{
}
