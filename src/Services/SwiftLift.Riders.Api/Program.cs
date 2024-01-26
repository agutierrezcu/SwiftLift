using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel;
using SwiftLift.SharedKernel.Application;
using SwiftLift.SharedKernel.ApplicationInsight;
using SwiftLift.SharedKernel.Environment;

var applicationInfo = new ApplicationInfo("swiftlift.riders.api", "Riders.Api", "SwiftLift");

var builder = WebApplication.CreateBuilder(args);

var applicationInsightConnectionString = ApplicationInsightResource.Instance
    .GetConnectionStringGuaranteed(EnvironmentService.Instance, builder.Configuration);

var assemblies = AppDomain.CurrentDomain.GetApplicationAssemblies(applicationInfo.Namespace);

builder.AddServiceDefaults(applicationInfo, assemblies);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
