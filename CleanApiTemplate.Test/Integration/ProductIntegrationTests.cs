using CleanApiTemplate.Application.Features.Products.Commands;
using CleanApiTemplate.Application.Features.Products.Queries;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Test.Common;
using CleanApiTemplate.Test.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CleanApiTemplate.Test.Integration;

/// <summary>
/// Integration tests for Product features
/// Tests complete database operations with real SQL Server connection
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration Tests")]
public class ProductIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateProduct_EndToEnd_ShouldPersistToDatabase()
    {
        // Arrange - Create a category first
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        var handler = new CreateProductCommandHandler(
            UnitOfWork,
            MockCurrentUserService.Object);

        var command = new CreateProductCommand
        {
            Name = "Integration Test Product",
            Description = "Created via integration test",
            Sku = "INT-TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            CategoryId = category.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        // Verify in database
        using var verifyContext = CreateNewContext();
        var savedProduct = await verifyContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == result.Data);

        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be(command.Name);
        savedProduct.Sku.Should().Be(command.Sku);
        savedProduct.Price.Should().Be(command.Price);
        savedProduct.StockQuantity.Should().Be(command.StockQuantity);
        savedProduct.CategoryId.Should().Be(category.Id);
        savedProduct.IsActive.Should().BeTrue();
        savedProduct.CreatedBy.Should().Be("test-user");
        savedProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ShouldFail()
    {
        // Arrange - Create existing product
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        var existingProduct = TestDataGenerator.GenerateProduct();
        existingProduct.CategoryId = category.Id;
        existingProduct.Sku = "DUPLICATE-SKU";
        existingProduct.CreatedBy = "test-user";

        await DbFactory.SeedEntitiesAsync(category, existingProduct);

        var handler = new CreateProductCommandHandler(
            UnitOfWork,
            MockCurrentUserService.Object);

        var command = new CreateProductCommand
        {
            Name = "New Product",
            Sku = "DUPLICATE-SKU", // Same SKU
            Price = 50.00m,
            StockQuantity = 10,
            CategoryId = category.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        result.Error.Should().Contain("DUPLICATE-SKU");
    }

    [Fact]
    public async Task CreateProduct_WithNonExistentCategory_ShouldFail()
    {
        // Arrange
        var handler = new CreateProductCommandHandler(
            UnitOfWork,
            MockCurrentUserService.Object);

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 10.00m,
            StockQuantity = 5,
            CategoryId = Guid.NewGuid() // Non-existent category
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetProducts_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange - Seed test data
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        var products = Enumerable.Range(1, 25)
            .Select(i =>
            {
                var product = TestDataGenerator.GenerateProduct();
                product.Name = $"Product {i:D2}";
                product.CategoryId = category.Id;
                product.IsActive = true;
                product.CreatedBy = "test-user";
                return product;
            })
            .ToArray();

        await DbFactory.SeedAsync(products);

        var handler = new GetProductsQueryHandler(UnitOfWork);

        var query = new GetProductsQuery
        {
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(10);
        result.Data.PageNumber.Should().Be(2);
        result.Data.PageSize.Should().Be(10);
        result.Data.TotalCount.Should().Be(25);
        result.Data.TotalPages.Should().Be(3);
        result.Data.HasPreviousPage.Should().BeTrue();
        result.Data.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetProducts_WithSearchTerm_ShouldFilterResults()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        var laptop1 = TestDataGenerator.GenerateProduct();
        laptop1.Name = "Gaming Laptop";
        laptop1.Description = "High performance laptop";
        laptop1.CategoryId = category.Id;
        laptop1.IsActive = true;
        laptop1.CreatedBy = "test-user";

        var laptop2 = TestDataGenerator.GenerateProduct();
        laptop2.Name = "Business Laptop";
        laptop2.Description = "Professional laptop";
        laptop2.CategoryId = category.Id;
        laptop2.IsActive = true;
        laptop2.CreatedBy = "test-user";

        var phone = TestDataGenerator.GenerateProduct();
        phone.Name = "Smartphone";
        phone.Description = "Latest phone model";
        phone.CategoryId = category.Id;
        phone.IsActive = true;
        phone.CreatedBy = "test-user";

        await DbFactory.SeedAsync(laptop1, laptop2, phone);

        var handler = new GetProductsQueryHandler(UnitOfWork);

        var query = new GetProductsQuery
        {
            SearchTerm = "laptop",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
        result.Data.Items.Should().OnlyContain(p => 
            p.Name.Contains("Laptop", StringComparison.OrdinalIgnoreCase) ||
            p.Description!.Contains("laptop", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ShouldReturnOnlyMatchingCategory()
    {
        // Arrange
        var category1 = TestDataGenerator.GenerateCategory();
        category1.Name = "Electronics";
        category1.CreatedBy = "test-user";

        var category2 = TestDataGenerator.GenerateCategory();
        category2.Name = "Books";
        category2.CreatedBy = "test-user";

        await DbFactory.SeedAsync(category1, category2);

        var electronicsProducts = Enumerable.Range(1, 5)
            .Select(i =>
            {
                var product = TestDataGenerator.GenerateProduct();
                product.CategoryId = category1.Id;
                product.IsActive = true;
                product.CreatedBy = "test-user";
                return product;
            })
            .ToArray();

        var bookProducts = Enumerable.Range(1, 3)
            .Select(i =>
            {
                var product = TestDataGenerator.GenerateProduct();
                product.CategoryId = category2.Id;
                product.IsActive = true;
                product.CreatedBy = "test-user";
                return product;
            })
            .ToArray();

        await DbFactory.SeedAsync(electronicsProducts.Concat(bookProducts).ToArray());

        var handler = new GetProductsQueryHandler(UnitOfWork);

        var query = new GetProductsQuery
        {
            CategoryId = category1.Id,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(5);
        result.Data.TotalCount.Should().Be(5);
        result.Data!.Items.Should().OnlyContain(p => p.CategoryId == category1.Id);
    }

    [Fact]
    public async Task GetProducts_IncludeInactiveFalse_ShouldReturnOnlyActiveProducts()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        var activeProduct = TestDataGenerator.GenerateProduct();
        activeProduct.CategoryId = category.Id;
        activeProduct.IsActive = true;
        activeProduct.CreatedBy = "test-user";

        var inactiveProduct = TestDataGenerator.GenerateProduct();
        inactiveProduct.CategoryId = category.Id;
        inactiveProduct.IsActive = false;
        inactiveProduct.CreatedBy = "test-user";

        await DbFactory.SeedAsync(activeProduct, inactiveProduct);

        var handler = new GetProductsQueryHandler(UnitOfWork);

        var query = new GetProductsQuery
        {
            IncludeInactive = false,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data!.Items.First().IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetProducts_IncludeInactiveTrue_ShouldReturnAllProducts()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        await DbFactory.SeedAsync(category);

        var activeProduct = TestDataGenerator.GenerateProduct();
        activeProduct.CategoryId = category.Id;
        activeProduct.IsActive = true;
        activeProduct.CreatedBy = "test-user";

        var inactiveProduct = TestDataGenerator.GenerateProduct();
        inactiveProduct.CategoryId = category.Id;
        inactiveProduct.IsActive = false;
        inactiveProduct.CreatedBy = "test-user";

        await DbFactory.SeedAsync(activeProduct, inactiveProduct);

        var handler = new GetProductsQueryHandler(UnitOfWork);

        var query = new GetProductsQuery
        {
            IncludeInactive = true,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Product_SoftDelete_ShouldNotAppearInQueries()
    {
        // Arrange
        var category = TestDataGenerator.GenerateCategory();
        category.CreatedBy = "test-user";
        var product = TestDataGenerator.GenerateProduct();
        product.CategoryId = category.Id;
        product.IsActive = true;
        product.CreatedBy = "test-user";

        await DbFactory.SeedEntitiesAsync(category, product);

        // Act - Soft delete the product
        using var deleteContext = CreateNewContext();
        var productToDelete = await deleteContext.Products.FindAsync(product.Id);
        deleteContext.Products.Remove(productToDelete!);
        await deleteContext.SaveChangesAsync();

        // Assert - Product should not appear in query (global query filter)
        var handler = new GetProductsQueryHandler(CreateNewUnitOfWork());
        var query = new GetProductsQuery
        {
            IncludeInactive = true,
            PageNumber = 1,
            PageSize = 10
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotContain(p => p.Id == product.Id);

        // Verify it's still in database but marked as deleted
        using var verifyContext = CreateNewContext();
        var deletedProduct = await verifyContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();
        deletedProduct.DeletedAt.Should().NotBeNull();
    }
}
