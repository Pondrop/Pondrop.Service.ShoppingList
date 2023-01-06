using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateShoppingListCommandHandlerValidator : AbstractValidator<UpdateShoppingListCommand>
{
    public UpdateShoppingListCommandHandlerValidator()
    {
        RuleForEach(x => x.ShoppingLists).ChildRules(shoppingList =>
        {
            shoppingList.RuleFor(x => x.Id).NotNull();
            shoppingList.RuleFor(x => x.Id).NotEmpty();
            shoppingList.RuleFor(x => x.Name).NotEmpty();
        });
    }
}