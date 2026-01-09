using CleanApiTemplate.Application.Features.Products.Commands;
using CleanApiTemplate.Test.Common;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CleanApiTemplate.Test.Application.Validators;

/// <summary>
/// Unit tests for DeleteProductCommandValidator
/// Demonstrates simple validation testing
/// </summary>
public class DeleteProductCommandValidatorTests : TestBase
{
    private readonly DeleteProductCommandValidator _validator;

    public DeleteProductCommandValidatorTests()
    {
        _validator = new DeleteProductCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new DeleteProductCommand
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new DeleteProductCommand
        {
            Id = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Product ID is required");
    }

    [Fact]
    public void Validate_ValidGuid_ShouldNotHaveValidationError()
    {
        // Arrange
        var validGuids = new[]
        {
            Guid.NewGuid(),
            Guid.Parse("12345678-1234-1234-1234-123456789012"),
            Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")
        };

        foreach (var guid in validGuids)
        {
            var command = new DeleteProductCommand { Id = guid };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }

    [Fact]
    public void Validate_DefaultGuid_ShouldHaveValidationError()
    {
        // Arrange
        var command = new DeleteProductCommand
        {
            Id = default(Guid) // Same as Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validator_ShouldOnlyValidateId()
    {
        // Arrange
        var command = new DeleteProductCommand
        {
            Id = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().OnlyContain(e => e.PropertyName == nameof(DeleteProductCommand.Id));
    }
}
