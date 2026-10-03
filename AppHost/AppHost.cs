var builder = DistributedApplication.CreateBuilder(args);


var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("jobapplicationhelper");

var api = builder.AddProject<Projects.JobApplicationHelper_Api>("api")
    .WithReference(database)
    .WaitFor(database);

var migrations = api
    .AddEFMigrations(
        "api-migrations",
        "JobApplicationHelper.Infrastructure.Persistence.JobApplicationHelperDbContext")
    .WithMigrationsProject<Projects.JobApplicationHelper_Infrastructure>()
    .WaitFor(database)
    .RunDatabaseUpdateOnStart();

api.WaitForCompletion(migrations);

builder.Build().Run();
