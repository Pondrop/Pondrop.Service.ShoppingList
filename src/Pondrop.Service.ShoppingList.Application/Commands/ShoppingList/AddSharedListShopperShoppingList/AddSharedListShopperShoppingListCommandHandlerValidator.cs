using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class AddSharedListShopperShoppingListCommandHandlerValidator : AbstractValidator<AddSharedListShopperShoppingListCommand>
{
    
    public AddSharedListShopperShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleFor(x => x.SharedListShopperId).NotEmpty();
    }
}