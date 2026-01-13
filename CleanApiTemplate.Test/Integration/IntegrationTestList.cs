namespace CleanApiTemplate.Test.Integration;

/// <summary>
/// Collection definition to prevent parallel execution of integration tests
/// All integration tests will run sequentially to avoid database conflicts
/// </summary>
[CollectionDefinition("Integration Tests", DisableParallelization = true)]
public class IntegrationTestList
{
    // This class is never instantiated.
    // It's just a marker for xUnit to group tests.
}
