using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using MediatR;

namespace CleanApiTemplate.Application.Features.Products.Commands;

/// <summary>
/// Handler for UpdateProductCommand
/// Demonstrates updating entities with validation and concurrency handling
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Product>();

        // Get existing product
        var product = await repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);

        if (product == null || product.IsDeleted)
        {
            return Result.Failure($"Product with ID '{request.Id}' not found");
        }

        // Check if SKU is being changed and if new SKU already exists
        if (product.Sku != request.Sku)
        {
            var skuExists = await repository.AnyAsync(
                p => p.Sku == request.Sku && p.Id != request.Id,
                cancellationToken);

            if (skuExists)
            {
                return Result.Failure($"Product with SKU '{request.Sku}' already exists");
            }
        }

        // Check if category exists
        var categoryRepository = _unitOfWork.Repository<Category>();
        var categoryExists = await categoryRepository.AnyAsync(
            c => c.Id == request.CategoryId && !c.IsDeleted,
            cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure($"Category with ID '{request.CategoryId}' not found");
        }

        // Update product properties
        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = _currentUserService.Username ?? "System";

        repository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
