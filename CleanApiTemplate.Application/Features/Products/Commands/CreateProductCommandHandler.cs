using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using MediatR;

namespace CleanApiTemplate.Application.Features.Products.Commands;

/// <summary>
/// Handler for CreateProductCommand
/// Demonstrates async/await pattern and dependency injection
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var repository = _unitOfWork.Repository<Product>();
        var skuExists = await repository.AnyAsync(p => p.Sku == request.Sku, cancellationToken);

        if (skuExists)
        {
            return Result<Guid>.Failure($"Product with SKU '{request.Sku}' already exists");
        }

        // Check if category exists
        var categoryRepository = _unitOfWork.Repository<Category>();
        var categoryExists = await categoryRepository.AnyAsync(
            c => c.Id == request.CategoryId && !c.IsDeleted,
            cancellationToken);

        if (!categoryExists)
        {
            return Result<Guid>.Failure($"Category with ID '{request.CategoryId}' not found");
        }

        // Create product entity
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Sku = request.Sku,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = _currentUserService.Username ?? "System"
        };

        await repository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }
}
