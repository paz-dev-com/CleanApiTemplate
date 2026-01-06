using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Application.Features.Products.Commands;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Test.Common;
using FluentAssertions;
using Moq;

namespace CleanApiTemplate.Test.Application.Handlers;

/// <summary>
/// Unit tests for CreateProductCommandHandler
/// Demonstrates mocking dependencies and testing business logic
/// </summary>
public class CreateProductCommandHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IRepository<Product>> _productRepositoryMock;
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
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

        _handler = new CreateProductCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Sku = "TEST-SKU-123",
            Price = 99.99m,
            StockQuantity = 10,
            CategoryId = categoryId
        };

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
        result.Data.Should().NotBeEmpty();
        
        _productRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Product>(p => 
                p.Name == command.Name &&
                p.Sku == command.Sku &&
                p.Price == command.Price &&
                p.CategoryId == categoryId &&
                p.IsActive == true), 
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "EXISTING-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        result.Error.Should().Contain(command.Sku);

        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = categoryId
        };

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

        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetCreatedByToCurrentUser()
    {
        // Arrange
        const string expectedUsername = "john.doe";
        _currentUserServiceMock.Setup(x => x.Username).Returns(expectedUsername);

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _productRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Product>(p => p.CreatedBy == expectedUsername), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullUsername_ShouldUseSystemAsCreatedBy()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.Username).Returns((string?)null);

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _productRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Product>(p => p.CreatedBy == "System"), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetProductAsActive()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Sku = "TEST-SKU",
            Price = 99.99m,
            CategoryId = Guid.NewGuid()
        };

        _productRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _productRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Product>(p => p.IsActive == true), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
