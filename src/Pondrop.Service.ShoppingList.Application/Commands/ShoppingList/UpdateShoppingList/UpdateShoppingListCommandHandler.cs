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

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateShoppingListCommandHandler : DirtyCommandHandler<ShoppingListEntity, UpdateShoppingListCommand, Result<ShoppingListRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateShoppingListCommand> _validator;
    private readonly ILogger<UpdateShoppingListCommandHandler> _logger;

    public UpdateShoppingListCommandHandler(
        IOptions<ShoppingListUpdateConfiguration> shoppingListUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateShoppingListCommand> validator,
        ILogger<UpdateShoppingListCommandHandler> logger) : base(eventRepository, shoppingListUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ShoppingListRecord>> Handle(UpdateShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update shoppingList failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListRecord>.Error(errorMessage);
        }

        var result = default(Result<ShoppingListRecord>);

        try
        {
            var shoppingListEntity = await _shoppingListCheckpointRepository.GetByIdAsync(command.Id);
            shoppingListEntity ??= await GetFromStreamAsync(command.Id);

            if (shoppingListEntity is not null)
            {
                var evtPayload = new UpdateShoppingList(
                    command.Id,
                    command.Name,
                    command.ShoppingListType,
                command.SelectedStoreIds,
                command.SharedListShopperIds,
                command.ListItemIds);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(shoppingListEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _shoppingListCheckpointRepository.FastForwardAsync(shoppingListEntity);
                    success = await UpdateStreamAsync(shoppingListEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(shoppingListEntity.Id, shoppingListEntity.GetEvents(shoppingListEntity.AtSequence)));

                result = success
                    ? Result<ShoppingListRecord>.Success(_mapper.Map<ShoppingListRecord>(shoppingListEntity))
                    : Result<ShoppingListRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<ShoppingListRecord>.Error($"ShoppingList does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ShoppingListRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateShoppingListCommand command) =>
        $"Failed to update shoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}