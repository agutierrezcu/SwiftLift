using System.Reflection;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SwiftLift.IdentityServerAspNetIdentity.Data;
using SwiftLift.IdentityServerAspNetIdentity.Models;
using SwiftLift.IdentityServerAspNetIdentity.Services;

namespace SwiftLift.IdentityServerAspNetIdentity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddRazorPages();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var identityConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        //services.AddDbContext<ApplicationDbContext>(
        //    options => options.UseSqlite(identityConnectionString));

        services
            .AddIdentityApiEndpoints<ApplicationUser>(
                options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        var identityServerBuilder =
            services
                .AddIdentityServer(
                    opts =>
                    {
                        opts.Events.RaiseErrorEvents = true;
                        opts.Events.RaiseInformationEvents = true;
                        opts.Events.RaiseFailureEvents = true;
                        opts.Events.RaiseSuccessEvents = true;

                        // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                        opts.EmitStaticAudienceClaim = true;
                    })
                .AddConfigurationStore(
                    opts =>
                    {
                        //opts.ConfigureDbContext = builder =>
                        //    builder.UseSqlite(identityConnectionString,
                        //        sql => sql.MigrationsAssembly(migrationsAssembly));
                    })
                 .AddOperationalStore(opts =>
                 {
                     //opts.ConfigureDbContext = builder =>
                     //      builder.UseSqlite(identityConnectionString,
                     //          sql => sql.MigrationsAssembly(migrationsAssembly));

                     // this enables automatic token cleanup. this is optional.
                     opts.EnableTokenCleanup = true;
                     opts.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
                 })
                .AddAspNetIdentity<ApplicationUser>();

        if (!builder.Environment.IsDevelopment())
        {
            identityServerBuilder.AddConfigurationStoreCache();
        }

        builder.Services.AddAuthentication(
            opts =>
            {
                opts.DefaultScheme = IdentityConstants.ApplicationScheme;
                opts.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
        .AddGoogle(
            opts =>
            {
                opts.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                opts.ClientId = "copy client ID from Google here";
                opts.ClientSecret = "copy client secret from Google here";
            });

        services.AddAuthorization();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        app.MapIdentityApi<ApplicationUser>();
        //app.MapAdditionalIdentityEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}
