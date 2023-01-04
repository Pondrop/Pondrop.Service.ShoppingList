using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetShoppingListByIdQueryHandlerValidator : AbstractValidator<GetShoppingListByIdQuery>
{
    public GetShoppingListByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}