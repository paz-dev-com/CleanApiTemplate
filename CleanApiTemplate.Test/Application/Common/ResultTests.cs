using CleanApiTemplate.Application.Common;
using FluentAssertions;

namespace CleanApiTemplate.Test.Application.Common;

/// <summary>
/// Unit tests for Result<T> class
/// Demonstrates testing result pattern implementation
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var data = "test data";

        // Act
        var result = Result<string>.Success(data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Data.Should().Be(data);
        result.Error.Should().BeNull();
        result.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Error.Should().Be(errorMessage);
        result.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void ValidationFailure_ShouldCreateFailedResultWithValidationErrors()
    {
        // Arrange
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required" } },
            { "Email", new[] { "Email is invalid", "Email is required" } }
        };

        // Act
        var result = Result<string>.ValidationFailure(validationErrors);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Error.Should().Be("Validation failed");
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors.Should().ContainKey("Name");
        result.ValidationErrors.Should().ContainKey("Email");
        result.ValidationErrors!["Email"].Should().HaveCount(2);
    }

    [Fact]
    public void Success_WithComplexType_ShouldStoreData()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var result = Result<object>.Success(data);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(data);
    }

    [Fact]
    public void Success_WithNull_ShouldAcceptNull()
    {
        // Arrange & Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
    }
}

/// <summary>
/// Unit tests for Result class (non-generic)
/// </summary>
public class ResultNonGenericTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
        result.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.ValidationErrors.Should().BeNull();
    }

    [Fact]
    public void ValidationFailure_ShouldCreateFailedResultWithValidationErrors()
    {
        // Arrange
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1" } }
        };

        // Act
        var result = Result.ValidationFailure(validationErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Validation failed");
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors.Should().ContainKey("Field1");
    }
}
