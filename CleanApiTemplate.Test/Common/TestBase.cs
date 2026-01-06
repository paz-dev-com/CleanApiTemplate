using AutoFixture;

namespace CleanApiTemplate.Test.Common;

/// <summary>
/// Base class for all unit tests providing common test utilities
/// </summary>
public abstract class TestBase
{
    protected readonly IFixture Fixture;

    protected TestBase()
    {
        Fixture = new Fixture();
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
