﻿using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class RemoveListItemToShoppingListCommandHandlerValidator : AbstractValidator<RemoveListItemToShoppingListCommand>
{
    
    public RemoveListItemToShoppingListCommandHandlerValidator()
    {
        RuleFor(x => x.ShoppingListId).NotEmpty();
        RuleForEach(x => x.ListItemIds).ChildRules(listItem =>
        {
            listItem.RuleFor(x => x).NotEmpty();
        });
    }
}