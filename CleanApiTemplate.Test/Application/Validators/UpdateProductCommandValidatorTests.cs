using CleanApiTemplate.Application.Features.Products.Commands;
using CleanApiTemplate.Test.Common;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CleanApiTemplate.Test.Application.Validators;

/// <summary>
/// Unit tests for UpdateProductCommandValidator
/// Demonstrates FluentValidation testing patterns
/// </summary>
public class UpdateProductCommandValidatorTests : TestBase
{
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductCommandValidatorTests()
    {
        _validator = new UpdateProductCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Product Name",
            Description = "Valid product description",
            Sku = "VALID-SKU-123",
            Price = 99.99m,
            StockQuantity = 10,
            CategoryId = Guid.NewGuid(),
            IsActive = true
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
        var command = new UpdateProductCommand
        {
            Id = Guid.Empty,
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Product ID is required");
    }

    [Fact]
    public void Validate_EmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Sku = "SKU-123",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 201), // 201 characters
            Sku = "SKU-123",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_NameExactly200Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = new string('A', 200), // Exactly 200 characters
            Sku = "SKU-123",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptySku_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = string.Empty,
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage("SKU is required");
    }

    [Fact]
    public void Validate_SkuTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = new string('A', 51), // 51 characters
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage("SKU cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_SkuWithLowercase_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "sku-123", // Contains lowercase
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku)
            .WithErrorMessage("SKU must contain only uppercase letters, numbers, and hyphens");
    }

    [Fact]
    public void Validate_SkuWithSpecialCharacters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU@123!", // Contains special characters
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sku);
    }

    [Fact]
    public void Validate_ValidSkuFormats_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validSkus = new[]
        {
            "SKU123",
            "SKU-123",
            "PRODUCT-ABC-123",
            "12345",
            "ABC"
        };

        foreach (var sku in validSkus)
        {
            var command = new UpdateProductCommand
            {
                Id = Guid.NewGuid(),
                Name = "Product",
                Sku = sku,
                Price = 99.99m,
                CategoryId = Guid.NewGuid()
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Sku);
        }
    }

    [Fact]
    public void Validate_PriceZero_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 0m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than zero");
    }

    [Fact]
    public void Validate_NegativePrice_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = -10m,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_PriceTooHigh_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 1000001m, // Over limit
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be less than 1,000,000");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(99.99)]
    [InlineData(999999.99)]
    public void Validate_ValidPriceRanges_ShouldNotHaveValidationError(decimal price)
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = price,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_NegativeStockQuantity_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            StockQuantity = -1,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity)
            .WithErrorMessage("Stock quantity cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000000)]
    public void Validate_ValidStockQuantity_ShouldNotHaveValidationError(int stockQuantity)
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            StockQuantity = stockQuantity,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StockQuantity);
    }

    [Fact]
    public void Validate_EmptyCategoryId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            CategoryId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category ID is required");
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            Description = new string('A', 2001), // 2001 characters
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters");
    }

    [Fact]
    public void Validate_DescriptionExactly2000Characters_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            Description = new string('A', 2000), // Exactly 2000 characters
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_NullDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            Description = null,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_EmptyDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Product",
            Sku = "SKU-123",
            Price = 99.99m,
            Description = string.Empty,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_MultipleErrors_ShouldReturnAllValidationErrors()
    {
        // Arrange
        var command = new UpdateProductCommand
        {
            Id = Guid.Empty, // Invalid
            Name = string.Empty, // Invalid
            Sku = string.Empty, // Invalid
            Price = 0m, // Invalid
            StockQuantity = -1, // Invalid
            CategoryId = Guid.Empty // Invalid
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Sku);
        result.ShouldHaveValidationErrorFor(x => x.Price);
        result.ShouldHaveValidationErrorFor(x => x.StockQuantity);
        result.ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}
