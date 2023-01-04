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

public class CreateListItemCommandHandler : DirtyCommandHandler<ListItemEntity, CreateListItemCommand, Result<ListItemRecord>>
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

    public override async Task<Result<ListItemRecord>> Handle(CreateListItemCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create ListItem failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ListItemRecord>.Error(errorMessage);
        }

        var result = default(Result<ListItemRecord>);

        try
        {
            var ListItemEntity = new ListItemEntity(
                command.ItemTitle,
                command.AddedBy,
                command.SelectedCategoryId,
                command.Quantity,
                command.ItemNetSize,
                command.ItemUOM,
                command.SelectedPreferenceIds,
                command.SelectedProductId,
                _userService.CurrentUserName());

            var success = await _eventRepository.AppendEventsAsync(ListItemEntity.StreamId, 0, ListItemEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(ListItemEntity.Id, ListItemEntity.GetEvents()));

            result = success
                ? Result<ListItemRecord>.Success(_mapper.Map<ListItemRecord>(ListItemEntity))
                : Result<ListItemRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ListItemRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateListItemCommand command) =>
        $"Failed to create ListItem\nCommand: '{JsonConvert.SerializeObject(command)}'";
}