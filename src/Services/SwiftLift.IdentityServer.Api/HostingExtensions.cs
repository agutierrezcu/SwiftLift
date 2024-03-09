using Ardalis.GuardClauses;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Serilog;
using SwiftLift.IdentityServer.Api.Data;
using SwiftLift.IdentityServer.Api.Models;
using SwiftLift.Infrastructure;
using SwiftLift.Infrastructure.Logging;
using SwiftLift.ServiceDefaults;
using SwiftLift.SharedKernel.Application;

namespace SwiftLift.IdentityServer.Api;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder,
        ApplicationInfo applicationInfo)
    {
        Guard.Against.Null(builder);
        Guard.Against.Null(applicationInfo);

        var applicationAssemblies = AppDomain.CurrentDomain
            .GetApplicationAssemblies(applicationInfo.Namespace);

        ServiceDefaultsOptions serviceDefaultsOptions = new()
        {
            ApplicationInfo = applicationInfo,
            ApplicationAssemblies = applicationAssemblies
        };

        builder.AddServiceDefaults(serviceDefaultsOptions);

        var services = builder.Services;

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var identityServerBuilder = services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;
                options.EmitScopesAsSpaceDelimitedStringInJwt = true;
            })
            .AddConfigurationStore(
                opts => opts.DefaultSchema = IdentityServerSchema.Configuration.ToStringFast())
            .AddOperationalStore(
                opts =>
                {
                    opts.DefaultSchema = IdentityServerSchema.Operational.ToStringFast();
                    opts.EnableTokenCleanup = true;
                    opts.TokenCleanupInterval = 3600;
                })
            .AddAspNetIdentity<ApplicationUser>();

        if (!builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddConfigurationStoreCache();
        }

        var identityServerAssembly = typeof(Program).Assembly;

        builder.AddIdentityServerDbContext<ApplicationDbContext>(
            IdentityServerConnectionString.Name,
            identityServerAssembly,
            IdentityServerSchema.Users);

        builder.AddIdentityServerDbContext<ConfigurationDbContext>(
            IdentityServerConnectionString.Name,
            identityServerAssembly,
            IdentityServerSchema.Configuration);

        builder.AddIdentityServerDbContext<PersistedGrantDbContext>(
           IdentityServerConnectionString.Name,
           identityServerAssembly,
           IdentityServerSchema.Operational);

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

        services.AddRazorPages();

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

        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        app.MapDefaultEndpoints();

        return app;
    }
}
