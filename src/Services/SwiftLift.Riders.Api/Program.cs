using SwiftLift.ServiceDefaults;
using SwiftLift.Infrastructure;
using SwiftLift.SharedKernel.Application;
using SwiftLift.Infrastructure.ApplicationInsight;
using SwiftLift.Infrastructure.Environment;

var applicationInfo = new ApplicationInfo("swiftlift.riders.api", "Riders.Api", "SwiftLift");

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

var assemblies = AppDomain.CurrentDomain
    .GetApplicationAssemblies(applicationInfo.Namespace);

builder.AddServiceDefaults(applicationInfo, assemblies);

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await app.RunAppAsync(args)
    .ConfigureAwait(false);
