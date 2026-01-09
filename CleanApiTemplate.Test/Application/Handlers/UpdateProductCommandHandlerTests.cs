using CleanApiTemplate.Application.Features.Products.Commands;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Test.Common;
using FluentAssertions;
using Moq;

namespace CleanApiTemplate.Test.Application.Handlers;

/// <summary>
/// Unit tests for UpdateProductCommandHandler
/// Demonstrates testing update operations with validation
/// </summary>
public class UpdateProductCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _productRepositoryMock = new Mock<IRepository<Product>>();
        _categoryRepositoryMock = new Mock<IRepository<Category>>();

        _unitOfWorkMock.Setup(x => x.Repository<Product>())
            .Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Repository<Category>())
            .Returns(_categoryRepositoryMock.Object);

        _currentUserServiceMock.Setup(x => x.Username).Returns("testuser");

        _handler = new UpdateProductCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateProductSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Description",
            Sku = "OLD-SKU",
            Price = 50m,
            StockQuantity = 5,
            CategoryId = Guid.NewGuid(),
            IsActive = true,
            IsDeleted = false
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Description = "Updated Description",
            Sku = "NEW-SKU",
            Price = 99.99m,
            StockQuantity = 20,
            CategoryId = categoryId,
            IsActive = false
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        existingProduct.Name.Should().Be(command.Name);
        existingProduct.Description.Should().Be(command.Description);
        existingProduct.Sku.Should().Be(command.Sku);
        existingProduct.Price.Should().Be(command.Price);
        existingProduct.StockQuantity.Should().Be(command.StockQuantity);
        existingProduct.CategoryId.Should().Be(command.CategoryId);
        existingProduct.IsActive.Should().Be(command.IsActive);
        existingProduct.UpdatedBy.Should().Be("testuser");
        existingProduct.UpdatedAt.Should().NotBeNull();

        _productRepositoryMock.Verify(x => x.Update(existingProduct), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "NEW-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

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
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeletedProduct_ShouldReturnFailure()
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
            DeletedAt = DateTime.UtcNow,
            DeletedBy = "admin"
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "NEW-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

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
    }

    [Fact]
    public async Task Handle_DuplicateSku_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Existing Product",
            Sku = "OLD-SKU",
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "DUPLICATE-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // SKU exists

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        result.Error.Should().Contain(command.Sku);

        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SkuNotChanged_ShouldNotCheckDuplicates()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        const string sameSku = "SAME-SKU";
        
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Product",
            Sku = sameSku,
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = sameSku, // Same SKU
            Price = 99.99m,
            CategoryId = categoryId
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify AnyAsync was NOT called since SKU didn't change
        _productRepositoryMock.Verify(
            x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Product",
            Sku = "SKU",
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "NEW-SKU",
            Price = 99.99m,
            CategoryId = categoryId
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        result.Error.Should().Contain(categoryId.ToString());

        _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NullUsername_ShouldUseSystemAsUpdatedBy()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Product",
            Sku = "SKU",
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "SKU",
            Price = 99.99m,
            CategoryId = categoryId
        };

        _currentUserServiceMock.Setup(x => x.Username).Returns((string?)null);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.UpdatedBy.Should().Be("System");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetUpdatedAtTimestamp()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var beforeUpdate = DateTime.UtcNow;
        
        var existingProduct = new Product
        {
            Id = productId,
            Name = "Product",
            Sku = "SKU",
            Price = 50m,
            CategoryId = Guid.NewGuid(),
            IsDeleted = false,
            UpdatedAt = null
        };

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "SKU",
            Price = 99.99m,
            CategoryId = categoryId
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingProduct.UpdatedAt.Should().NotBeNull();
        existingProduct.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
        existingProduct.UpdatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldPropagateCancellation()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "Updated Product",
            Sku = "SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _handler.Handle(command, cancellationTokenSource.Token));
    }
}
