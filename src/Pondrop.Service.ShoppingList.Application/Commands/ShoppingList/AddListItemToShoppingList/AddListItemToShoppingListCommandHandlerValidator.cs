using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class AddListItemToShoppingListCommandHandlerValidator : AbstractValidator<AddListItemToShoppingListCommand>
{
    
    public AddListItemToShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleFor(x => x.ListItemId).NotEmpty();
    }
}