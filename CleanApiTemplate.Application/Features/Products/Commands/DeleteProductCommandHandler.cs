using CleanApiTemplate.Application.Common;
using CleanApiTemplate.Core.Entities;
using CleanApiTemplate.Core.Interfaces;
using MediatR;

namespace CleanApiTemplate.Application.Features.Products.Commands;

/// <summary>
/// Handler for DeleteProductCommand
/// Demonstrates soft delete pattern - marks entity as deleted instead of removing from database
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteProductCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Product>();

        // Get existing product
        var product = await repository.GetByIdAsync(request.Id, cancellationToken: cancellationToken);

        if (product == null || product.IsDeleted)
        {
            return Result.Failure($"Product with ID '{request.Id}' not found");
        }

        // Soft delete - mark as deleted instead of removing from database
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        product.DeletedBy = _currentUserService.Username ?? "System";

        repository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
