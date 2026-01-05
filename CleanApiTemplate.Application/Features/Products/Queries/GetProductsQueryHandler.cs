using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Core.Interfaces;
using MediatR;

namespace CleanApiTemplate.Application.Features.Products.Queries;

/// <summary>
/// Handler for GetProductsQuery
/// Demonstrates efficient query with NoTracking, projections, and pagination
/// </summary>
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PaginatedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaginatedResult<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Use raw SQL with Dapper for optimized performance
            // This demonstrates avoiding N+1 queries and using projections
            var sql = @"
                SELECT
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Sku,
                    p.Price,
                    p.StockQuantity,
                    p.IsActive,
                    p.CategoryId,
                    c.Name as CategoryName,
                    p.CreatedAt,
                    p.CreatedBy
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.IsDeleted = 0
                    AND c.IsDeleted = 0
                    AND (@IncludeInactive = 1 OR p.IsActive = 1)
                    AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                    AND (@SearchTerm IS NULL OR
                         p.Name LIKE '%' + @SearchTerm + '%' OR
                         p.Description LIKE '%' + @SearchTerm + '%' OR
                         p.Sku LIKE '%' + @SearchTerm + '%')
                ORDER BY p.CreatedAt DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var countSql = @"
                SELECT COUNT(*)
                FROM Products p
                INNER JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.IsDeleted = 0
                    AND c.IsDeleted = 0
                    AND (@IncludeInactive = 1 OR p.IsActive = 1)
                    AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
                    AND (@SearchTerm IS NULL OR
                         p.Name LIKE '%' + @SearchTerm + '%' OR
                         p.Description LIKE '%' + @SearchTerm + '%' OR
                         p.Sku LIKE '%' + @SearchTerm + '%')";

            var offset = (request.PageNumber - 1) * request.PageSize;

            var parameters = new
            {
                IncludeInactive = request.IncludeInactive ? 1 : 0,
                CategoryId = request.CategoryId,
                SearchTerm = request.SearchTerm,
                Offset = offset,
                PageSize = request.PageSize
            };

            // Execute queries in parallel for better performance
            var productsTask = _unitOfWork.ExecuteQueryAsync<ProductDto>(sql, parameters, cancellationToken);
            var countTask = _unitOfWork.ExecuteQueryAsync<int>(countSql, parameters, cancellationToken);

            await Task.WhenAll(productsTask, countTask);

            var products = await productsTask;
            var totalCount = (await countTask).FirstOrDefault();

            var result = new PaginatedResult<ProductDto>(
                products,
                totalCount,
                request.PageNumber,
                request.PageSize);

            return Result<PaginatedResult<ProductDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<ProductDto>>.Failure($"Failed to retrieve products: {ex.Message}");
        }
    }
}
