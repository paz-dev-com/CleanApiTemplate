using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Application.Features.Products.Queries;
using CleanApiTemplate.Core.Interfaces;
using CleanApiTemplate.Test.Common;
using FluentAssertions;
using Moq;

namespace CleanApiTemplate.Test.Application.Handlers;

/// <summary>
/// Unit tests for GetProductsQueryHandler
/// Demonstrates testing query handlers with pagination
/// </summary>
public class GetProductsQueryHandlerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new GetProductsQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnPaginatedProducts()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var products = new List<ProductDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Product 1", Sku = "SKU-1", Price = 10.99m },
            new() { Id = Guid.NewGuid(), Name = "Product 2", Sku = "SKU-2", Price = 20.99m },
            new() { Id = Guid.NewGuid(), Name = "Product 3", Sku = "SKU-3", Price = 30.99m }
        };

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<ProductDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { 3 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(3);
        result.Data.TotalCount.Should().Be(3);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassSearchParameter()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "Test"
        };

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<ProductDto>(
                It.IsAny<string>(),
                It.Is<object>(p => p.GetType().GetProperty("SearchTerm")!.GetValue(p)!.Equals("Test")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { 0 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.ExecuteQueryAsync<ProductDto>(
            It.IsAny<string>(),
            It.Is<object>(p => p.GetType().GetProperty("SearchTerm")!.GetValue(p)!.Equals("Test")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldPassCategoryParameter()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            CategoryId = categoryId
        };

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<ProductDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { 0 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.ExecuteQueryAsync<ProductDto>(
            It.IsAny<string>(),
            It.Is<object>(p => 
                p.GetType().GetProperty("CategoryId")!.GetValue(p)!.Equals(categoryId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SecondPage_ShouldCalculateCorrectOffset()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 2,
            PageSize = 10
        };

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<ProductDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { 0 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.ExecuteQueryAsync<ProductDto>(
            It.IsAny<string>(),
            It.Is<object>(p => 
                p.GetType().GetProperty("Offset")!.GetValue(p)!.Equals(10)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_IncludeInactiveTrue_ShouldPassCorrectFlag()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            IncludeInactive = true
        };

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<ProductDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { 0 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.ExecuteQueryAsync<ProductDto>(
            It.IsAny<string>(),
            It.Is<object>(p => 
                p.GetType().GetProperty("IncludeInactive")!.GetValue(p)!.Equals(1)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<ProductDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to retrieve products");
        result.Error.Should().Contain("Database connection error");
    }

    [Fact]
    public async Task Handle_EmptyResults_ShouldReturnEmptyPaginatedResult()
    {
        // Arrange
        var query = new GetProductsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<ProductDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>());

        _unitOfWorkMock
            .Setup(x => x.ExecuteQueryAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { 0 });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
        result.Data.TotalPages.Should().Be(0);
    }
}
