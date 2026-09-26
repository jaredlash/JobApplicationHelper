var builder = DistributedApplication.CreateBuilder(args);


var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("jobapplicationhelper");

var api = builder.AddProject<Projects.JobApplicationHelper_Api>("api")
    .WithReference(database)
    .WaitFor(database);


builder.Build().Run();
