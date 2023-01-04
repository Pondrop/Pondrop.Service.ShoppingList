using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateListItemCommandHandlerValidator : AbstractValidator<CreateListItemCommand>
{
    
    public CreateListItemCommandHandlerValidator()
    {
        RuleFor(x => x.ItemTitle).NotEmpty();
    }
}