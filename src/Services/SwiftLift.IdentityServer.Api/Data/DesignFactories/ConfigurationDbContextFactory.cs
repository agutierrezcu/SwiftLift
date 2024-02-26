using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftLift.IdentityServer.Api.Data.DesignFactories;

public sealed class ConfigurationDbContextFactory
    : IDesignTimeDbContextFactory<ConfigurationDbContext>, IDisposable
{
    private bool _disposedValue;

    private IServiceScope? _scope;

    public ConfigurationDbContext CreateDbContext(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Environment.EnvironmentName = "Development";

        var services = builder.Services;

        services
            .AddIdentityServer()
            .AddConfigurationStore(
                opts => opts.DefaultSchema = IdentityServerSchema.Configuration.ToStringFast());

        builder.AddIdentityServerDbContext<ConfigurationDbContext>(
           IdentityServerConnectionString.Name,
           typeof(Program).Assembly,
           IdentityServerSchema.Configuration);

        var serviceProvider = services.BuildServiceProvider();

        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        _scope = serviceScopeFactory.CreateScope();

        return _scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _scope?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
