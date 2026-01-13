using CleanApiTemplate.Application.Features.Products.Commands;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Test.Common;
using FluentAssertions;
using Moq;

namespace CleanApiTemplate.Test.Application.Handlers;

/// <summary>
/// Unit tests for DeleteProductCommandHandler
/// Demonstrates testing soft delete pattern
/// </summary>
public class DeleteProductCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _productRepositoryMock = new Mock<IRepository<Product>>();

        _unitOfWorkMock.Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);

        _currentUserServiceMock.Setup(x => x.Username).Returns("testuser");

        _handler = new DeleteProductCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSoftDeleteProductSuccessfullyAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false,
            DeletedAt = null,
            DeletedBy = null
        };

        var command = new DeleteProductCommand { Id = productId };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        existingProduct.IsDeleted.Should().BeTrue();
        existingProduct.DeletedAt.Should().NotBeNull();
        existingProduct.DeletedBy.Should().Be("testuser");

        _productRepositoryMock.Verify(x => x.Update(existingProduct), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Verify that Remove was NOT called (soft delete, not hard delete)
        _productRepositoryMock.Verify(x => x.Remove(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnFailureAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand { Id = productId };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Error.Should().Contain(productId.ToString());

        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        _productRepositoryMock.Verify(x => x.Remove(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyDeletedProduct_ShouldReturnFailureAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var deletedProduct = new Product
        {
            Id = productId,
            Name = "Deleted Product",
            Sku = "DEL-SKU",
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow.AddDays(-1),
            DeletedBy = "admin"
        };

        var command = new DeleteProductCommand { Id = productId };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");

        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NullUsername_ShouldUseSystemAsDeletedByAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var command = new DeleteProductCommand { Id = productId };

        _currentUserServiceMock.Setup(x => x.Username).Returns((string?)null);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.IsDeleted.Should().BeTrue();
        existingProduct.DeletedBy.Should().Be("System");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetDeletedAtTimestampAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var beforeDelete = DateTime.UtcNow;
        
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false,
            DeletedAt = null
        };

        var command = new DeleteProductCommand { Id = productId };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.DeletedAt.Should().NotBeNull();
        existingProduct.DeletedAt.Should().BeOnOrAfter(beforeDelete);
        existingProduct.DeletedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_SoftDelete_ShouldNotRemoveFromDatabaseAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var command = new DeleteProductCommand { Id = productId };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify Update was called (soft delete modifies the entity)
        _productRepositoryMock.Verify(x => x.Update(existingProduct), Times.Once);
        
        // Verify Remove was NOT called (data is preserved)
        _productRepositoryMock.Verify(x => x.Remove(It.IsAny<Product>()), Times.Never);
        _productRepositoryMock.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<Product>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SoftDelete_ShouldPreserveAllProductDataAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Sku = "TEST-SKU",
            Price = 99.99m,
            StockQuantity = 10,
            CategoryId = categoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            CreatedBy = "creator",
            IsDeleted = false
        };

        var command = new DeleteProductCommand { Id = productId };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify all original data is preserved
        existingProduct.Name.Should().Be("Test Product");
        existingProduct.Description.Should().Be("Test Description");
        existingProduct.Sku.Should().Be("TEST-SKU");
        existingProduct.Price.Should().Be(99.99m);
        existingProduct.StockQuantity.Should().Be(10);
        existingProduct.CategoryId.Should().Be(categoryId);
        existingProduct.IsActive.Should().BeTrue();
        existingProduct.CreatedBy.Should().Be("creator");
        
        // Only deletion flags are changed
        existingProduct.IsDeleted.Should().BeTrue();
        existingProduct.DeletedAt.Should().NotBeNull();
        existingProduct.DeletedBy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldPropagateCancellationAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new DeleteProductCommand { Id = productId };
        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallUpdateExactlyOnceAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var command = new DeleteProductCommand { Id = productId };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p => p.Id == productId)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
