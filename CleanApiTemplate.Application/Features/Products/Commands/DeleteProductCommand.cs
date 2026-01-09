using CleanApiTemplate.Application.Common;

namespace CleanApiTemplate.Application.Features.Products.Commands;

/// <summary>
/// Command to delete (soft delete) a product
/// Demonstrates CQRS command pattern with soft delete
/// </summary>
public class DeleteProductCommand : CommandBase<Result>
{
    public Guid Id { get; set; }
}
