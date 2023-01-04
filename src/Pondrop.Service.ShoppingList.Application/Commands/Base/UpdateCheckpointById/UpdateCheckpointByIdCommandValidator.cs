using FluentValidation;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateCheckpointByIdCommandValidator : AbstractValidator<UpdateCheckpointByIdCommand>
{
    public UpdateCheckpointByIdCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}