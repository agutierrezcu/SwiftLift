using Ardalis.GuardClauses;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SwiftLift.IdentityServer.Api.Data;
using SwiftLift.IdentityServer.Api.Models;
using SwiftLift.Infrastructure;
using SwiftLift.Infrastructure.ConnectionString;
using SwiftLift.Infrastructure.EfDbContext;
using SwiftLift.Infrastructure.Logging;
using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.IdentityServer.Api;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder,
        ApplicationInfo applicationInfo,
        ConnectionStringResource applicationInsightConnectionString)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInfo);
        Guard.Against.Null(applicationInsightConnectionString);

        var applicationAssemblies = AppDomain.CurrentDomain
            .GetApplicationAssemblies(applicationInfo.Namespace);

        ServiceDefaultsOptions serviceDefaultsOptions = new()
        {
            ApplicationInfo = applicationInfo,
            ApplicationInsightConnectionString = applicationInsightConnectionString,
            ApplicationAssemblies = applicationAssemblies,
            UseFastEndpoints = false
        };

        builder.AddServiceDefaults(serviceDefaultsOptions);

        builder.AddNpgsqlDbContext<ApplicationDbContext>("identityserverdb", null,
            opts =>
                opts.EnableDetailedErrors()
                    .UseNpgsql(
                        builder =>
                        {
                            var identityServerApiAssembly = typeof(Program).Assembly;
                            builder
                                .MigrationsAssembly(identityServerApiAssembly.GetName().Name)
                                .MigrationsHistoryTable("__EFMigrationsHistory", ApplicationDbContext.Schema);
                        }));

        var services = builder.Services;

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(DbContextMigrationsActivity.SourceName));

        if (builder.Environment.IsDevelopment())
        {
            services.AddSingleton<DbContextMigrationsRunner<ApplicationDbContext>>();
            services.AddHostedService(sp => sp.GetRequiredService<DbContextMigrationsRunner<ApplicationDbContext>>());
        }

        services.AddRazorPages();

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>();

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        Guard.Against.Null(app);

        app.UseSerilogRequestLogging(
            SerilogRequestLoggingOptions.Configure);

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseHeaderPropagation();

        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        app.MapDefaultEndpoints();

        return app;
    }
}
