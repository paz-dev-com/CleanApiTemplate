using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Data.Persistence;
using CleanApiTemplate.Data.Repositories;
using Moq;

namespace CleanApiTemplate.Test.Integration.Infrastructure;

/// <summary>
/// Base class for integration tests
/// Provides database setup and common test infrastructure
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationTestDbFactory DbFactory { get; private set; } = null!;
    protected ApplicationDbContext DbContext { get; private set; } = null!;
    protected IUnitOfWork UnitOfWork { get; private set; } = null!;
    protected Mock<ICurrentUserService> MockCurrentUserService { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        DbFactory = new IntegrationTestDbFactory();
        await DbFactory.InitializeAsync();
        
        DbContext = DbFactory.CreateDbContext();
        UnitOfWork = new UnitOfWork(DbContext);
        
        // Setup mock current user service
        MockCurrentUserService = new Mock<ICurrentUserService>();
        MockCurrentUserService.Setup(x => x.UserId).Returns("test-user-id");
        MockCurrentUserService.Setup(x => x.Username).Returns("test-user");
        MockCurrentUserService.Setup(x => x.Email).Returns("test@example.com");
        MockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
    }

    public virtual async Task DisposeAsync()
    {
        await DbFactory.ResetDatabaseAsync();
        
        if (UnitOfWork is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        await DbContext.DisposeAsync();
        await DbFactory.DisposeAsync();
    }

    /// <summary>
    /// Create a new DbContext instance for parallel test execution
    /// </summary>
    protected ApplicationDbContext CreateNewContext()
    {
        return DbFactory.CreateDbContext();
    }

    /// <summary>
    /// Create a new UnitOfWork instance for parallel test execution
    /// </summary>
    protected IUnitOfWork CreateNewUnitOfWork()
    {
        return new UnitOfWork(CreateNewContext());
    }
}
