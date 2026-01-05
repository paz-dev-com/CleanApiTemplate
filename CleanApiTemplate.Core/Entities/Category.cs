namespace CleanApiTemplate.Core.Entities;

/// <summary>
/// Sample domain entity representing a product category
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Category name (required, max 100 characters)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description (optional)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to products in this category
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}