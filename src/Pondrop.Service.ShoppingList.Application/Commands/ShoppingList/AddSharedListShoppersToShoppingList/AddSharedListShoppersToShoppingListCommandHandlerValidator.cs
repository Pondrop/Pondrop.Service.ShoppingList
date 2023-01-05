using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class AddSharedListShoppersToShoppingListCommandHandlerValidator : AbstractValidator<AddSharedListShoppersToShoppingListCommand>
{
    
    public AddSharedListShoppersToShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleForEach(x => x.SharedListShopperIds).ChildRules(sharedListShopper =>
        {
            sharedListShopper.RuleFor(x => x).NotEmpty();
        });
    }
}