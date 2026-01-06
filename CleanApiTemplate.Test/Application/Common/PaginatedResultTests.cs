using CleanApiTemplate.Application.Common;
using FluentAssertions;

namespace CleanApiTemplate.Test.Application.Common;

/// <summary>
/// Unit tests for PaginatedResult<T> class
/// Demonstrates testing pagination logic
/// </summary>
public class PaginatedResultTests
{
    [Fact]
    public void Constructor_WithParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var totalCount = 10;
        var pageNumber = 2;
        var pageSize = 3;

        // Act
        var result = new PaginatedResult<string>(items, totalCount, pageNumber, pageSize);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(totalCount);
        result.PageNumber.Should().Be(pageNumber);
        result.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>(
            new List<string>(),
            25,
            1,
            10);

        // Assert
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_WithExactDivision_ShouldCalculateCorrectly()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>(
            new List<string>(),
            30,
            1,
            10);

        // Assert
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_WithZeroItems_ShouldReturnZero()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>(
            new List<string>(),
            0,
            1,
            10);

        // Assert
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ShouldBeFalse()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>(
            new List<string>(),
            20,
            1,
            10);

        // Assert
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ShouldBeTrue()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>(
            new List<string>(),
            20,
            2,
            10);

        // Assert
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_OnLastPage_ShouldBeFalse()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>(
            new List<string>(),
            20,
            2,
            10);

        // Assert
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_NotOnLastPage_ShouldBeTrue()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>(
            new List<string>(),
            30,
            2,
            10);

        // Assert
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldPaginateSourceList()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var pageNumber = 2;
        var pageSize = 3;

        // Act
        var result = PaginatedResult<int>.Create(source, pageNumber, pageSize);

        // Assert
        result.Items.Should().BeEquivalentTo(new[] { 4, 5, 6 });
        result.TotalCount.Should().Be(10);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(4);
    }

    [Fact]
    public void Create_WithFirstPage_ShouldReturnFirstItems()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = PaginatedResult<int>.Create(source, 1, 3);

        // Assert
        result.Items.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLastPage_ShouldReturnRemainingItems()
    {
        // Arrange
        var source = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var result = PaginatedResult<int>.Create(source, 2, 3);

        // Assert
        result.Items.Should().BeEquivalentTo(new[] { 4, 5 });
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptySource_ShouldReturnEmptyResult()
    {
        // Arrange
        var source = new List<int>();

        // Act
        var result = PaginatedResult<int>.Create(source, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void DefaultConstructor_ShouldInitializeWithEmptyItems()
    {
        // Arrange & Act
        var result = new PaginatedResult<string>();

        // Assert
        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(0);
        result.PageSize.Should().Be(0);
    }
}
