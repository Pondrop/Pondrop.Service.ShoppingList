using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateSharedListShopperCommandHandlerValidator : AbstractValidator<CreateSharedListShopperCommand>
{
    
    public CreateSharedListShopperCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty(); 
        RuleForEach(x => x.SharedListShoppers).ChildRules(shareListShopper =>
        {
            shareListShopper.RuleFor(x => x.UserId).NotEmpty();
        });
    }
}