using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetAllShoppingListsQueryHandlerValidator : AbstractValidator<GetAllShoppingListsQuery>
{
    public GetAllShoppingListsQueryHandlerValidator()
    {
    }
}