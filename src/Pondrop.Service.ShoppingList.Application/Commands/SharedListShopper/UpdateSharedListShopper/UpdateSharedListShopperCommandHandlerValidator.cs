using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateSharedListShopperCommandHandlerValidator : AbstractValidator<UpdateSharedListShopperCommand>
{
    public UpdateSharedListShopperCommandHandlerValidator()
    {
           RuleFor(x => x.ShoppingListId).NotEmpty();
            RuleForEach(x => x.SharedListShoppers).ChildRules(sharedListShopper =>
            {
                sharedListShopper.RuleFor(x => x.Id).NotNull();
                sharedListShopper.RuleFor(x => x.Id).NotEmpty();
                sharedListShopper.RuleFor(x => x.UserId).NotNull();
                sharedListShopper.RuleFor(x => x.UserId).NotEmpty();
            });
    }
}