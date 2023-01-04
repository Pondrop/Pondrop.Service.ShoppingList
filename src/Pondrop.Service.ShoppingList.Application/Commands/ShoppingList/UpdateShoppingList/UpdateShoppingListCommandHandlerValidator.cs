using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateShoppingListCommandHandlerValidator : AbstractValidator<UpdateShoppingListCommand>
{
    public UpdateShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}