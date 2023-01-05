using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class DeleteSharedListShopperCommandHandlerValidator : AbstractValidator<DeleteSharedListShopperCommand>
{

    public DeleteSharedListShopperCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleForEach(x => x.SharedListShopperIds).ChildRules(SharedListShopper =>
        {
            SharedListShopper.RuleFor(x => x).NotEmpty();
        });
    }
}