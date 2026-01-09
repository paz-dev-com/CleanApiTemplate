using FluentValidation;

namespace CleanApiTemplate.Application.Features.Products.Commands;

/// <summary>
/// Validator for DeleteProductCommand
/// Demonstrates simple validation for delete operations
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product ID is required");
    }
}
