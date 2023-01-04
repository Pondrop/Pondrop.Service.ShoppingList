using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RemoveSharedListShopperToShoppingListCommandHandlerValidator : AbstractValidator<RemoveSharedListShopperToShoppingListCommand>
{
    
    public RemoveSharedListShopperToShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleFor(x => x.SharedListShopperId).NotEmpty();
    }
}