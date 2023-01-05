using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class AddListItemsToShoppingListCommandHandlerValidator : AbstractValidator<AddListItemsToShoppingListCommand>
{
    
    public AddListItemsToShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleForEach(x => x.ListItemIds).ChildRules(listItem =>
        {
            listItem.RuleFor(x => x).NotEmpty();
        });
    }
}