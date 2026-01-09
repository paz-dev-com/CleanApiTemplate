using CleanApiTemplate.Application.Common;

namespace CleanApiTemplate.Application.Features.Products.Commands;

/// <summary>
/// Command to update an existing product
/// Demonstrates CQRS command pattern with validation
/// </summary>
public class UpdateProductCommand : CommandBase<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; }
}
