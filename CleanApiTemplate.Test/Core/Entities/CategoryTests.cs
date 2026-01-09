using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Test.Common;
using FluentAssertions;

namespace CleanApiTemplate.Test.Core.Entities;

/// <summary>
/// Unit tests for Category entity
/// </summary>
public class CategoryTests : TestBase
{
    [Fact]
    public void Category_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var category = new Category();

        // Assert
        category.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Category_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var category = new Category();

        // Assert
        category.Id.Should().BeEmpty();
        category.Name.Should().BeEmpty();
        category.Description.Should().BeNull();
        category.Products.Should().NotBeNull();
        category.Products.Should().BeEmpty();
    }

    [Fact]
    public void Category_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1" },
            new() { Id = Guid.NewGuid(), Name = "Product 2" }
        };

        // Act
        var category = new Category
        {
            Id = categoryId,
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            Products = products,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin"
        };

        // Assert
        category.Id.Should().Be(categoryId);
        category.Name.Should().Be("Electronics");
        category.Description.Should().Be("Electronic devices and accessories");
        category.Products.Should().HaveCount(2);
        category.Products.Should().Contain(p => p.Name == "Product 1");
    }

    [Fact]
    public void Category_ShouldSupportNullDescription()
    {
        // Arrange & Act
        var category = new Category
        {
            Name = "Test Category",
            Description = null
        };

        // Assert
        category.Description.Should().BeNull();
    }

    [Fact]
    public void Category_ProductsCollection_ShouldBeModifiable()
    {
        // Arrange
        var category = new Category { Name = "Test" };
        var product = new Product { Name = "Test Product" };

        // Act
        category.Products.Add(product);

        // Assert
        category.Products.Should().HaveCount(1);
        category.Products.Should().Contain(product);
    }

    [Fact]
    public void Category_WithTestDataGenerator_ShouldHaveValidData()
    {
        // Arrange & Act
        var category = TestDataGenerator.GenerateCategory();

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().NotBeEmpty();
        category.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Category_GenerateMultiple_ShouldCreateUniqueInstances()
    {
        // Arrange & Act
        var categories = TestDataGenerator.GenerateCategories(5);

        // Assert
        categories.Should().HaveCount(5);
        categories.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        // Note: Category names may not be unique as Bogus generates from a limited set
    }
}
