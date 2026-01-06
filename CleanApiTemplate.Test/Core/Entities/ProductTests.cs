using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Test.Common;
using FluentAssertions;

namespace CleanApiTemplate.Test.Core.Entities;

/// <summary>
/// Unit tests for Product entity
/// Demonstrates testing domain entities and business rules
/// </summary>
public class ProductTests : TestBase
{
    [Fact]
    public void Product_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Product_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        product.Id.Should().BeEmpty();
        product.Name.Should().BeEmpty();
        product.Description.Should().BeNull();
        product.Sku.Should().BeEmpty();
        product.Price.Should().Be(0);
        product.StockQuantity.Should().Be(0);
        product.IsActive.Should().BeFalse();
        product.CategoryId.Should().BeEmpty();
        product.Category.Should().BeNull();
    }

    [Fact]
    public void Product_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };
        var createdDate = DateTime.UtcNow;

        // Act
        var product = new Product
        {
            Id = productId,
            Name = "Laptop",
            Description = "High-performance laptop",
            Sku = "LAP-001",
            Price = 1299.99m,
            StockQuantity = 50,
            IsActive = true,
            CategoryId = categoryId,
            Category = category,
            CreatedAt = createdDate,
            CreatedBy = "admin"
        };

        // Assert
        product.Id.Should().Be(productId);
        product.Name.Should().Be("Laptop");
        product.Description.Should().Be("High-performance laptop");
        product.Sku.Should().Be("LAP-001");
        product.Price.Should().Be(1299.99m);
        product.StockQuantity.Should().Be(50);
        product.IsActive.Should().BeTrue();
        product.CategoryId.Should().Be(categoryId);
        product.Category.Should().NotBeNull();
        product.Category!.Id.Should().Be(categoryId);
        product.CreatedAt.Should().Be(createdDate);
        product.CreatedBy.Should().Be("admin");
    }

    [Fact]
    public void Product_ShouldSupportNullDescription()
    {
        // Arrange & Act
        var product = new Product
        {
            Name = "Test Product",
            Sku = "TEST-001",
            Price = 10.00m,
            Description = null
        };

        // Assert
        product.Description.Should().BeNull();
    }

    [Fact]
    public void Product_CategoryNavigation_ShouldBeNullableReference()
    {
        // Arrange & Act
        var product = new Product
        {
            CategoryId = Guid.NewGuid()
        };

        // Assert
        product.Category.Should().BeNull();
        product.CategoryId.Should().NotBeEmpty();
    }

    [Fact]
    public void Product_WithTestDataGenerator_ShouldHaveValidData()
    {
        // Arrange & Act
        var product = TestDataGenerator.GenerateProduct();

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().NotBeNullOrEmpty();
        product.Sku.Should().NotBeNullOrEmpty();
        product.Price.Should().BeGreaterThan(0);
        product.CategoryId.Should().NotBeEmpty();
    }

    [Fact]
    public void Product_GenerateMultiple_ShouldCreateUniqueInstances()
    {
        // Arrange & Act
        var products = TestDataGenerator.GenerateProducts(5);

        // Assert
        products.Should().HaveCount(5);
        products.Select(p => p.Id).Should().OnlyHaveUniqueItems();
        products.Select(p => p.Sku).Should().OnlyHaveUniqueItems();
    }
}
