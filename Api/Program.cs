using JobApplicationHelper.Api.Endpoints;
using JobApplicationHelper.Api.Hubs;
using JobApplicationHelper.Api.Services;
using JobApplicationHelper.Application;
using JobApplicationHelper.Application.Services;
using JobApplicationHelper.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();

builder.Services.AddScoped<IBackgroundJobNotifier, SignalRBackgroundJobNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // adds the /scalar/v1 endpoint for the Scalar API reference
}

app.UseHttpsRedirection();



app.MapJobRequirementsEndpoints();
app.MapBackgroundJobEndpoints();
app.MapCoverLetterEndpoints();
app.MapExperienceBankEndpoints();
app.MapLocationsEndpoints();
app.MapJobApplicationsEndpoints();
app.MapHub<BackgroundJobHub>("/hubs/background-jobs");

app.MapGet("/health", () => Results.Ok());

app.Run();