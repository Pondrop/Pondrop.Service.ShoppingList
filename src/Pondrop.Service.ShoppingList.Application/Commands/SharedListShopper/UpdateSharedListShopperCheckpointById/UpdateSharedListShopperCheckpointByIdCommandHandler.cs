using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Application.Commands;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateSharedListShopperCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateSharedListShopperCheckpointByIdCommand, SharedListShopperEntity, SharedListShopperRecord>
{
    public UpdateSharedListShopperCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<SharedListShopperEntity> SharedListShopperCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateSharedListShopperCheckpointByIdCommandHandler> logger) : base(eventRepository, SharedListShopperCheckpointRepository, mapper, validator, logger)
    {
    }
}