using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateStoreCommandHandlerValidator : AbstractValidator<CreateShoppingListCommand>
{
    
    public CreateStoreCommandHandlerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}