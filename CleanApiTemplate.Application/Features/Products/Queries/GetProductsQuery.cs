using CleanApiTemplate.Application.Common;

namespace CleanApiTemplate.Application.Features.Products.Queries;

/// <summary>
/// Query to get paginated list of products
/// Demonstrates CQRS query pattern with pagination
/// </summary>
public class GetProductsQuery : QueryBase<Result<PaginatedResult<ProductDto>>>
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Optional search term for filtering
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional category filter
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Include inactive products
    /// </summary>
    public bool IncludeInactive { get; set; }
}
