using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.ShoppingList.Application.Models;

using Pondrop.Service.ShoppingList.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.ShoppingList.Domain.Events.ListItem;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class DeleteListItemCommandHandler : DirtyCommandHandler<ListItemEntity, DeleteListItemCommand, Result<List<ListItemRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ListItemEntity> _listItemCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<DeleteListItemCommand> _validator;
    private readonly ILogger<DeleteListItemCommandHandler> _logger;

    public DeleteListItemCommandHandler(
        IOptions<ShoppingListUpdateConfiguration> shoppingListUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ListItemEntity> listItemCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<DeleteListItemCommand> validator,
        ILogger<DeleteListItemCommandHandler> logger) : base(eventRepository, shoppingListUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _listItemCheckpointRepository = listItemCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<List<ListItemRecord>>> Handle(DeleteListItemCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update list item failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ListItemRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ListItemRecord>>);

        try
        {
            var entities = new List<ListItemEntity>();
            foreach (var listItem in command.ListItemIds)
            {
                var listItemEntity = await _listItemCheckpointRepository.GetByIdAsync(listItem);
                listItemEntity ??= await GetFromStreamAsync(listItem);

                if (listItemEntity is not null)
                {
                    var evtPayload = new DeleteListItem(
                        listItem);
                    var createdBy = _userService.CurrentUserName();

                    var success = await UpdateStreamAsync(listItemEntity, evtPayload, createdBy);

                    if (!success)
                    {
                        await _listItemCheckpointRepository.FastForwardAsync(listItemEntity);
                        success = await UpdateStreamAsync(listItemEntity, evtPayload, createdBy);
                    }

                    entities.Add(listItemEntity);

                    await Task.WhenAll(
                        InvokeDaprMethods(listItemEntity.Id, listItemEntity.GetEvents(listItemEntity.AtSequence)));
                }
                else
                {
                    result = Result<List<ListItemRecord>>.Error($"List Item does not exist '{listItem}'");
                }

            }
            result = entities != null
                      ? Result<List<ListItemRecord>>.Success(_mapper.Map<List<ListItemRecord>>(entities))
                      : Result<List<ListItemRecord>>.Error(FailedToCreateMessage(command));


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<List<ListItemRecord>>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(DeleteListItemCommand command) =>
        $"Failed to update shoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}