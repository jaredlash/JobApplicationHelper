namespace JobApplicationHelper.IntegrationTests;

internal static class TestDatabase
{
    public static string ConnectionString => Environment.GetEnvironmentVariable("JOBAPPLICATIONHELPER_TEST_CONNECTION_STRING")
        ?? throw new InvalidOperationException("The JOBAPPLICATIONHELPER_TEST_CONNECTION_STRING " +
            "environment variable is not set.");
}