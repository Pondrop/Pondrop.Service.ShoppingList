using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RemoveListItemToShoppingListCommandHandlerValidator : AbstractValidator<RemoveListItemToShoppingListCommand>
{
    
    public RemoveListItemToShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleFor(x => x.ListItemId).NotEmpty();
    }
}