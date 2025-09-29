using FluentValidation;
using SOLID_Template.Application.DTOs.Order;
using SOLID_Template.Domain.Interfaces;

namespace SOLID_Template.Application.Validators;

/// <summary>
/// Validator for CreateOrderDto using FluentValidation
/// </summary>
public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderValidator(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Order number is required")
            .MinimumLength(3).WithMessage("Number must have at least 3 characters")
            .MaximumLength(20).WithMessage("Number must have at most 20 characters")
            .MustAsync(BeUniqueNumber).WithMessage("This order number already exists");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MinimumLength(10).WithMessage("Description must have at least 10 characters")
            .MaximumLength(500).WithMessage("Description must have at most 500 characters");

        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("At least one product must be associated with the order");

        RuleForEach(x => x.Products).ChildRules(product =>
        {
            product.RuleFor(p => p.ProductId)
                .NotEmpty().WithMessage("Product ID is required");
            
            product.RuleFor(p => p.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");
        });
    }

    private async Task<bool> BeUniqueNumber(string number, CancellationToken cancellationToken)
    {
        return !await _orderRepository.NumberExistsAsync(number);
    }
}