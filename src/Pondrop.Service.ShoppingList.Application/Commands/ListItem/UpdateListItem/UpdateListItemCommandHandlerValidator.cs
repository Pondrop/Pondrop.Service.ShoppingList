using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateListItemCommandHandlerValidator : AbstractValidator<UpdateListItemCommand>
{
    public UpdateListItemCommandHandlerValidator()
    {
           RuleFor(x => x.ShoppingListId).NotEmpty();
            RuleForEach(x => x.ListItems).ChildRules(listItem =>
            {
                listItem.RuleFor(x => x.Id).NotNull();
                listItem.RuleFor(x => x.Id).NotEmpty();
            });
    }
}