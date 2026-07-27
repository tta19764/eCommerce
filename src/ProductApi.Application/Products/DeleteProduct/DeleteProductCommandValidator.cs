using FluentValidation;

namespace ProductApi.Application.Products.DeleteProduct;

/// <summary>
/// Validates product deletion commands.
/// </summary>
public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    /// Initializes validation rules for product deletion.
    /// </summary>
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty();
    }
}
