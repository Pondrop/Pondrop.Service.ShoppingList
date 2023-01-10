using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.ShoppingList.Application.Models;

using Pondrop.Service.ShoppingList.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class CreateListItemCommandHandler : DirtyCommandHandler<ListItemEntity, CreateListItemCommand, Result<List<ListItemRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateListItemCommand> _validator;
    private readonly ILogger<CreateListItemCommandHandler> _logger;

    public CreateListItemCommandHandler(
        IOptions<ListItemUpdateConfiguration> ListItemUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateListItemCommand> validator,
        ILogger<CreateListItemCommandHandler> logger) : base(eventRepository, ListItemUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<List<ListItemRecord>>> Handle(CreateListItemCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create ListItem failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ListItemRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ListItemRecord>>);

        try
        {
            var entities = new List<ListItemEntity>();

            foreach (var listItem in command.ListItems)
            {
                var ListItemEntity = new ListItemEntity(
                listItem.ItemTitle,
                listItem.SelectedCategoryId,
                listItem.Quantity,
                listItem.ItemNetSize,
                listItem.ItemUOM,
                listItem.SelectedPreferenceIds,
                listItem.SelectedProductId,
                listItem.StoreId,
                listItem.SortOrder,
                listItem.Checked,
                _userService.CurrentUserName());

                var success = await _eventRepository.AppendEventsAsync(ListItemEntity.StreamId, 0, ListItemEntity.GetEvents());

                await Task.WhenAll(
                    InvokeDaprMethods(ListItemEntity.Id, ListItemEntity.GetEvents()));

                if (success)
                    entities.Add(ListItemEntity);
            }

            result = entities.Count() > 0
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

    private static string FailedToCreateMessage(CreateListItemCommand command) =>
        $"Failed to create ListItem\nCommand: '{JsonConvert.SerializeObject(command)}'";
}