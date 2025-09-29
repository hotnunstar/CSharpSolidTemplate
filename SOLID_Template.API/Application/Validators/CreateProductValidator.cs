using FluentValidation;
using SOLID_Template.Application.DTOs;

namespace SOLID_Template.Application.Validators;

/// <summary>
/// Validator for CreateProductDto following business rules
/// </summary>
public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .Length(2, 200)
            .WithMessage("Product name must be between 2 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required")
            .Length(3, 50)
            .WithMessage("SKU must be between 3 and 50 characters")
            .Matches("^[A-Za-z0-9-_]+$")
            .WithMessage("SKU can only contain letters, numbers, hyphens and underscores");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero")
            .PrecisionScale(18, 2, true)
            .WithMessage("Price cannot have more than 2 decimal places");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Category is required")
            .Length(2, 100)
            .WithMessage("Category must be between 2 and 100 characters");
    }
}