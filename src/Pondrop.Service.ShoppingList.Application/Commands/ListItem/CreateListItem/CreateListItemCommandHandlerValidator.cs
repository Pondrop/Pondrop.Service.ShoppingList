using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateListItemCommandHandlerValidator : AbstractValidator<CreateListItemCommand>
{

    public CreateListItemCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleForEach(x => x.ListItems).ChildRules(listItem =>
        {
            listItem.RuleFor(x => x.ItemTitle).NotEmpty();
        });
    }
}