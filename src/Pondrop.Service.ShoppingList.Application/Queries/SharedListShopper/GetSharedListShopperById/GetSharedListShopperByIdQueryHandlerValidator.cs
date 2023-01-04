using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetSharedListShopperByIdQueryHandlerValidator : AbstractValidator<GetSharedListShopperByIdQuery>
{
    public GetSharedListShopperByIdQueryHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}