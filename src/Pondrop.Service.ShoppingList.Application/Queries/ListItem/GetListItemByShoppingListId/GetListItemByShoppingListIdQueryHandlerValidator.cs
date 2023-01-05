using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetListItemByShoppingListIdQueryHandlerValidator : AbstractValidator<GetListItemByShoppingListIdQuery>
{
    public GetListItemByShoppingListIdQueryHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
    }
}