using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetAllListItemsQueryHandlerValidator : AbstractValidator<GetAllListItemsQuery>
{
    public GetAllListItemsQueryHandlerValidator()
    {
    }
}