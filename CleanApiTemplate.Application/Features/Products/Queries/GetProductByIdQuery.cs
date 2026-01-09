using CleanApiTemplate.Application.Common;

namespace CleanApiTemplate.Application.Features.Products.Queries;

/// <summary>
/// Query to get a single product by ID
/// Demonstrates CQRS query pattern
/// </summary>
public class GetProductByIdQuery : QueryBase<Result<ProductDto>>
{
    public Guid Id { get; set; }
}
