using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateShoppingListCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateShoppingListCheckpointByIdCommand, ShoppingListEntity, ShoppingListRecord>
{
    public UpdateShoppingListCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateShoppingListCheckpointByIdCommandHandler> logger) : base(eventRepository, shoppingListCheckpointRepository, mapper, validator, logger)
    {
    }
}