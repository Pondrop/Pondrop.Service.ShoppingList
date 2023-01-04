using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetListItemByIdQueryHandlerValidator : AbstractValidator<GetListItemByIdQuery>
{
    public GetListItemByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}