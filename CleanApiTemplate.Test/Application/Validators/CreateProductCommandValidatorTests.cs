using CleanApiTemplate.Application.Features.Products.Commands;
using FluentAssertions;

namespace CleanApiTemplate.Test.Application.Validators;

/// <summary>
/// Unit tests for CreateProductCommandValidator
/// Demonstrates testing FluentValidation validators
/// </summary>
public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _validator = new CreateProductCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Sku = "TEST-SKU-123",
            Price = 99.99m,
            StockQuantity = 10,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyName_ShouldHaveValidationError(string name)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = name,
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = new string('a', 201),
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateProductCommand.Name) &&
            e.ErrorMessage.Contains("200 characters"));
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = new string('a', 2001),
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateProductCommand.Description) &&
            e.ErrorMessage.Contains("2000 characters"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptySku_ShouldHaveValidationError(string sku)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = sku,
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Sku));
    }

    [Theory]
    [InlineData("test-sku")]
    [InlineData("TEST SKU")]
    [InlineData("TEST@SKU")]
    public void Validate_InvalidSkuFormat_ShouldHaveValidationError(string sku)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = sku,
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateProductCommand.Sku) &&
            e.ErrorMessage.Contains("uppercase"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_InvalidPrice_ShouldHaveValidationError(decimal price)
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = price,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Price));
    }

    [Fact]
    public void Validate_PriceTooHigh_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 1000000m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == nameof(CreateProductCommand.Price) &&
            e.ErrorMessage.Contains("1,000,000"));
    }

    [Fact]
    public void Validate_NegativeStockQuantity_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            StockQuantity = -1,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.StockQuantity));
    }

    [Fact]
    public void Validate_EmptyCategoryId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.CategoryId));
    }
}
