using CleanApiTemplate.Application.Common;

namespace CleanApiTemplate.Application.Features.Products.Commands;

/// <summary>
/// Command to create a new product
/// Demonstrates CQRS command pattern with validation
/// </summary>
public class CreateProductCommand : CommandBase<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid CategoryId { get; set; }
}
