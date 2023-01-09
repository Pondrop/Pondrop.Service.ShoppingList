using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class DeleteShoppingListCommandHandlerValidator : AbstractValidator<DeleteShoppingListCommand>
{
    public DeleteShoppingListCommandHandlerValidator()
    {
        
        RuleFor(x => x.Id).NotNull();
       RuleFor(x => x.Id).NotEmpty();
    }
}