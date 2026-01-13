using AutoFixture;

namespace CleanApiTemplate.Test.Common;

/// <summary>
/// Base class for all unit tests providing common test utilities
/// </summary>
public abstract class TestBase
{
    protected IFixture Fixture { get; }

    protected TestBase()
    {
        Fixture = new Fixture();
    }

    /// <summary>
    /// Call this method in your test class constructor to customize AutoFixture configuration.
    /// </summary>
    protected void InitializeFixture()
    {
        ConfigureFixture();
    }

    /// <summary>
    /// Override this method to customize AutoFixture configuration for specific test classes
    /// </summary>
    protected virtual void ConfigureFixture()
    {
        // Default configuration can be added here
    }
}
