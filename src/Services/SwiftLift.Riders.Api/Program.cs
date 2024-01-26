using SimpleInjector;
using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel;
using SwiftLift.SharedKernel.Application;
using SwiftLift.SharedKernel.ApplicationInsight;
using SwiftLift.SharedKernel.Environment;

var applicationInfo = new ApplicationInfo("swiftlift.riders.api", "Riders.Api", "SwiftLift");

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

var assemblies = AppDomain.CurrentDomain
    .GetApplicationAssemblies(applicationInfo.Namespace);

var container = new Container();

services.AddSimpleInjector(
    container, opts => opts.AddAspNetCore());

builder.AddServiceDefaults(applicationInfo, assemblies, container);

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.Services.UseSimpleInjector(container);

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

container.Verify();

await app.RunAppAsync(args, container)
    .ConfigureAwait(false);
