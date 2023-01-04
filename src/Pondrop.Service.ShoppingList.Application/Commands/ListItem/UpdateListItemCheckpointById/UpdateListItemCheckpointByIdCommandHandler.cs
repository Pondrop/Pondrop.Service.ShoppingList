using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateListItemCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateListItemCheckpointByIdCommand, ListItemEntity, ListItemRecord>
{
    public UpdateListItemCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<ListItemEntity> ListItemCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateListItemCheckpointByIdCommandHandler> logger) : base(eventRepository, ListItemCheckpointRepository, mapper, validator, logger)
    {
    }
}