var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.JobApplicationHelper_Api>("api");

builder.Build().Run();
