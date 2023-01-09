using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class DeleteShoppingListCommandHandlerValidator : AbstractValidator<DeleteShoppingListCommand>
{
    public DeleteShoppingListCommandHandlerValidator()
    {

        RuleForEach(x => x.Ids).ChildRules(id =>
        {
            id.RuleFor(x => x).NotEmpty();
        });
    }
}