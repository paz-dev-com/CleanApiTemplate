using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Test.Common;
using CleanApiTemplate.Test.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CleanApiTemplate.Test.Integration;

/// <summary>
/// Integration tests for Category entity operations
/// Demonstrates testing with related entities and navigation properties
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration Tests")]
public class CategoryIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Category_WithProducts_ShouldLoadNavigationProperty()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        var products = Enumerable.Range(1, 3)
            .Select(i =>
            {
                var product = TestDataGenerator.GenerateProduct();
                product.CategoryId = category.Id;
                product.Name = $"Product {i}";
                product.CreatedBy = "test-user";
                return product;
            })
            .ToArray();

        await DbFactory.SeedAsync(products);

        // Act
        using var context = CreateNewContext();
        var loadedCategory = await context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == category.Id);

        // Assert
        loadedCategory.Should().NotBeNull();
        loadedCategory!.Products.Should().HaveCount(3);
        loadedCategory.Products.Should().OnlyContain(p => p.CategoryId == category.Id);
    }

    [Fact]
    public async Task Category_Delete_ShouldSoftDeleteCategory()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        var product = TestDataGenerator.GenerateProduct();
        product.CategoryId = category.Id;
        product.CreatedBy = "test-user";
        await DbFactory.SeedAsync(product);

        // Act - Soft delete category
        using var context = CreateNewContext();
        var categoryToDelete = await context.Categories.FindAsync(category.Id);
        context.Categories.Remove(categoryToDelete!);
        await context.SaveChangesAsync();

        // Assert - Category should be soft deleted (not appear in normal queries)
        using var verifyContext = CreateNewContext();
        var deletedCategory = await verifyContext.Categories.FindAsync(category.Id);
        deletedCategory.Should().BeNull("soft deleted categories should not appear in normal queries");

        // But category still exists with IsDeleted = true
        var actualCategory = await verifyContext.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        actualCategory.Should().NotBeNull();
        actualCategory!.IsDeleted.Should().BeTrue();

        // Product still exists and references the soft-deleted category
        var existingProduct = await verifyContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        existingProduct.Should().NotBeNull();
        existingProduct!.CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public async Task Category_Update_ShouldPersistChanges()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.Name = "Original Name";
        category.Description = "Original Description";
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        // Act
        using var context = CreateNewContext();
        var categoryToUpdate = await context.Categories.FindAsync(category.Id);
        categoryToUpdate!.Name = "Updated Name";
        categoryToUpdate.Description = "Updated Description";
        categoryToUpdate.UpdatedBy = "test-user";
        categoryToUpdate.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Assert
        using var verifyContext = CreateNewContext();
        var updatedCategory = await verifyContext.Categories.FindAsync(category.Id);
        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be("Updated Name");
        updatedCategory.Description.Should().Be("Updated Description");
        updatedCategory.UpdatedBy.Should().Be("test-user");
        updatedCategory.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Category_CreateMultiple_ShouldAllPersist()
    {
        // Arrange
        var categories = TestDataGenerator.GenerateCategories(5);
        foreach (var category in categories)
        {
            category.CreatedBy = "test-user";
        }

        // Act
        await DbFactory.SeedAsync(categories.ToArray());

        // Assert
        using var context = CreateNewContext();
        var savedCategories = await context.Categories.ToListAsync();
        savedCategories.Should().HaveCount(5);
        savedCategories.Should().OnlyContain(c => c.CreatedBy == "test-user");
    }

    [Fact]
    public async Task Category_SoftDelete_ShouldSetDeletedFlags()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        // Act - Soft delete
        using var context = CreateNewContext();
        var categoryToDelete = await context.Categories.FindAsync(category.Id);
        context.Categories.Remove(categoryToDelete!);
        await context.SaveChangesAsync();

        // Assert - Should not appear in normal queries
        using var verifyContext = CreateNewContext();
        var deletedCategory = await verifyContext.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        deletedCategory.Should().BeNull();

        // But should exist when ignoring filters
        var actualCategory = await verifyContext.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        actualCategory.Should().NotBeNull();
        actualCategory!.IsDeleted.Should().BeTrue();
        actualCategory.DeletedAt.Should().NotBeNull();
    }
}
