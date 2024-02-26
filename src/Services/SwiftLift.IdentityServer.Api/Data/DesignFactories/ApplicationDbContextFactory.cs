using Microsoft.EntityFrameworkCore.Design;

namespace SwiftLift.IdentityServer.Api.Data.DesignFactories;

public sealed class ApplicationDbContextFactory :
    IDesignTimeDbContextFactory<ApplicationDbContext>, IDisposable
{
    private bool _disposedValue;

    private IServiceScope? _scope;

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Environment.EnvironmentName = "Development";

        builder.AddIdentityServerDbContext<ApplicationDbContext>(
           IdentityServerConnectionString.Name,
           typeof(Program).Assembly,
           IdentityServerSchema.Users);

        var serviceProvider = builder.Services.BuildServiceProvider();

        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        _scope = serviceScopeFactory.CreateScope();

        return _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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
