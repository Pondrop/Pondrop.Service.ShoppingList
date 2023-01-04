using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Queries;

public class GetAllSharedListShoppersQueryHandlerValidator : AbstractValidator<GetAllSharedListShoppersQuery>
{
    public GetAllSharedListShoppersQueryHandlerValidator()
    {
    }
}