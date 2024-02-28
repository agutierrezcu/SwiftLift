using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore.Design;

namespace SwiftLift.IdentityServer.Api.Data.DesignFactories;

public sealed class PersistedGrantDbContextFactory
    : IDesignTimeDbContextFactory<PersistedGrantDbContext>, IDisposable
{
    private bool _disposedValue;

    private IServiceScope? _scope;

    public PersistedGrantDbContext CreateDbContext(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Environment.EnvironmentName = "Development";

        var services = builder.Services;

        var options = new ConfigurationStoreOptions
        {
            DefaultSchema = IdentityServerSchema.Operational.ToStringFast()
        };

        services.AddSingleton(options);

        builder.AddIdentityServerDbContext<PersistedGrantDbContext>(
            IdentityServerConnectionString.Name,
            typeof(Program).Assembly,
            IdentityServerSchema.Operational);

        var serviceProvider = services.BuildServiceProvider();

        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        _scope = serviceScopeFactory.CreateScope();

        return _scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
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
