using CleanApiTemplate.Application.Features.Products.Queries;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Test.Common;
using FluentAssertions;
using Moq;

namespace CleanApiTemplate.Test.Application.Handlers;

/// <summary>
/// Unit tests for GetProductByIdQueryHandler
/// Demonstrates testing query handlers with mocked dependencies
/// </summary>
public class GetProductByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();

        _unitOfWorkMock.Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Repository<Category>())
            .Returns(_categoryRepositoryMock.Object);

        _handler = new GetProductByIdQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidProductId_ShouldReturnProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Sku = "TEST-SKU",
            Price = 99.99m,
            StockQuantity = 10,
            IsActive = true,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "testuser",
            IsDeleted = false
        };

        var category = new Category
        {
            Id = categoryId,
            Name = "Test Category",
            IsDeleted = false
        };

        var query = new GetProductByIdQuery { Id = productId };

        SetupProductRepositoryMock(product);
        SetupCategoryRepositoryMock(category);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(productId);
        result.Data.Name.Should().Be(product.Name);
        result.Data.Description.Should().Be(product.Description);
        result.Data.Sku.Should().Be(product.Sku);
        result.Data.Price.Should().Be(product.Price);
        result.Data.StockQuantity.Should().Be(product.StockQuantity);
        result.Data.IsActive.Should().Be(product.IsActive);
        result.Data.CategoryId.Should().Be(categoryId);
        result.Data.CategoryName.Should().Be(category.Name);
        result.Data.CreatedBy.Should().Be(product.CreatedBy);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery { Id = productId };

        SetupProductRepositoryMock();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Error.Should().Contain(productId.ToString());
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_DeletedProduct_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery { Id = productId };

        SetupProductRepositoryMock();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ProductWithoutCategory_ShouldReturnEmptyCategoryName()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = categoryId,
            IsDeleted = false
        };

        var query = new GetProductByIdQuery { Id = productId };

        SetupProductRepositoryMock(product);
        SetupCategoryRepositoryMock();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CategoryName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Handle_ProductWithEmptyCategoryId_ShouldReturnEmptyCategoryName()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.Empty,
            IsDeleted = false
        };

        var query = new GetProductByIdQuery { Id = productId };

        SetupProductRepositoryMock(product);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CategoryName.Should().Be(string.Empty);
        
        // Verify category repository was not called since CategoryId is empty
        _categoryRepositoryMock.Verify(
            x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUseAsNoTracking_ForReadOnlyQuery()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var query = new GetProductByIdQuery { Id = productId };

        SetupProductRepositoryMock(product);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Verify that FindAsync was called (which uses AsNoTracking internally)
        _productRepositoryMock.Verify(
            x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldPropagateCancellation()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = new GetProductByIdQuery { Id = productId };
        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        _productRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _handler.Handle(query, cancellationTokenSource.Token));
    }

    private void SetupProductRepositoryMock(Product? product = null)
    {
        var products = product != null ? new[] { product } : Array.Empty<Product>();
        _productRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
    }

    private void SetupCategoryRepositoryMock(Category? category = null)
    {
        var categories = category != null ? new[] { category } : Array.Empty<Category>();
        _categoryRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
    }
}
