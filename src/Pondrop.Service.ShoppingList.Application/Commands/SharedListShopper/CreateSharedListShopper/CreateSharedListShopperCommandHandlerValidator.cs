using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateSharedListShopperCommandHandlerValidator : AbstractValidator<CreateSharedListShopperCommand>
{
    
    public CreateSharedListShopperCommandHandlerValidator()
    {
        RuleFor(x => x.ShopperId).NotEmpty();
    }
}