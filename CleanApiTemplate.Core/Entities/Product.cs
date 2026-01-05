namespace CleanApiTemplate.Core.Entities;

/// <summary>
/// Sample domain entity representing a product
/// Demonstrates proper entity modeling with validation rules
/// </summary>
public class Product : BaseEntity
{
    /// <summary>
    /// Product name (required, max 200 characters)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description (optional, max 2000 characters)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Product SKU (Stock Keeping Unit) - unique identifier
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product price in decimal format
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Available stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Flag indicating if product is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Category identifier (foreign key)
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Navigation property to Category
    /// </summary>
    public Category? Category { get; set; }
}