using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.ShoppingList.Application.Models;
using Pondrop.Service.ShoppingList.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.ShoppingList.Domain.Events.ShoppingList;
using Pondrop.Service.ShoppingList.Domain.Events.ListItem;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateListItemCommandHandler : DirtyCommandHandler<ListItemEntity, UpdateListItemCommand, Result<List<ListItemRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ListItemEntity> _listItemCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateListItemCommand> _validator;
    private readonly ILogger<UpdateListItemCommandHandler> _logger;

    public UpdateListItemCommandHandler(
        IOptions<ListItemUpdateConfiguration> ListItemUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ListItemEntity> listItemCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateListItemCommand> validator,
        ILogger<UpdateListItemCommandHandler> logger) : base(eventRepository, ListItemUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _listItemCheckpointRepository = listItemCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<List<ListItemRecord>>> Handle(UpdateListItemCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update ListItem failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ListItemRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ListItemRecord>>);

        try
        {
            var entities = new List<ListItemEntity>();
            foreach (var listItem in command.ListItems)
            {
                var listItemEntity = await _listItemCheckpointRepository.GetByIdAsync(listItem.Id);
                listItemEntity ??= await GetFromStreamAsync(listItem.Id);

                if (listItemEntity is not null)
                {
                    if (listItemEntity.CreatedBy == _userService.CurrentUserName() || _userService.CurrentUserName() == "admin")
                    {
                        var evtPayload = new UpdateListItem(
                        listItem.Id,
                        listItemEntity.ItemTitle,
                        listItemEntity.AddedBy,
                        listItemEntity.SelectedCategoryId,
                        listItemEntity.Quantity,
                        listItemEntity.ItemNetSize,
                        listItemEntity.ItemUOM,
                        listItemEntity.SelectedPreferenceIds,
                        listItemEntity.SelectedProductId,
                        listItemEntity.StoreId,
                        listItem.SortOrder,
                        listItem.Checked
                        );
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
                        result = Result<List<ListItemRecord>>.Error($"List Item does not belong to '{_userService.CurrentUserId}'");
                    }
                }
                else
                {
                    result = Result<List<ListItemRecord>>.Error($"List Item does not exist '{listItem}'");
                }

            }
            result = entities != null && entities.Count() > 0
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

    private static string FailedToCreateMessage(UpdateListItemCommand command) =>
        $"Failed to update ListItem\nCommand: '{JsonConvert.SerializeObject(command)}'";
}