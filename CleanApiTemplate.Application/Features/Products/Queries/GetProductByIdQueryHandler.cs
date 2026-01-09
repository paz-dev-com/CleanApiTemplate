using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using MediatR;

namespace CleanApiTemplate.Application.Features.Products.Queries;

/// <summary>
/// Handler for GetProductByIdQuery
/// Demonstrates repository pattern usage and entity-to-DTO mapping
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var productRepository = _unitOfWork.Repository<Product>();
        var categoryRepository = _unitOfWork.Repository<Category>();

        // Get product (read-only query with AsNoTracking)
        var products = await productRepository.FindAsync(
            p => p.Id == request.Id && !p.IsDeleted,
            cancellationToken);

        var product = products.FirstOrDefault();

        if (product == null)
        {
            return Result<ProductDto>.Failure($"Product with ID '{request.Id}' not found");
        }

        // Get category separately
        Category? category = null;
        if (product.CategoryId != Guid.Empty)
        {
            var categories = await categoryRepository.FindAsync(
                c => c.Id == product.CategoryId && !c.IsDeleted,
                cancellationToken);
            category = categories.FirstOrDefault();
        }

        // Map to DTO
        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt,
            CreatedBy = product.CreatedBy
        };

        return Result<ProductDto>.Success(productDto);
    }
}
