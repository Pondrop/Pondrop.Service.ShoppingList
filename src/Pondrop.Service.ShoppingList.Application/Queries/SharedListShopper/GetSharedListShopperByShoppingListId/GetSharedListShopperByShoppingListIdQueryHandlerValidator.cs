using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetSharedListShopperByShoppingListIdQueryHandlerValidator : AbstractValidator<GetSharedListShopperByShoppingListIdQuery>
{
    public GetSharedListShopperByShoppingListIdQueryHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
    }
}