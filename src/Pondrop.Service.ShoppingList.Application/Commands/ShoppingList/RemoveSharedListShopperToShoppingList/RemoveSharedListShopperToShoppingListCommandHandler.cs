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

public class RemoveSharedListShopperToShoppingListCommandHandler : DirtyCommandHandler<ShoppingListEntity, RemoveSharedListShopperToShoppingListCommand, Result<ShoppingListRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _shoppingListCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<RemoveSharedListShopperToShoppingListCommand> _validator;
    private readonly ILogger<RemoveSharedListShopperToShoppingListCommandHandler> _logger;

    public RemoveSharedListShopperToShoppingListCommandHandler(
        IOptions<ShoppingListUpdateConfiguration> shoppingListUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ShoppingListEntity> shoppingListCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<RemoveSharedListShopperToShoppingListCommand> validator,
        ILogger<RemoveSharedListShopperToShoppingListCommandHandler> logger) : base(eventRepository, shoppingListUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _shoppingListCheckpointRepository = shoppingListCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ShoppingListRecord>> Handle(RemoveSharedListShopperToShoppingListCommand command, CancellationToken cancellationToken)
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
            var shoppingListEntity = await _shoppingListCheckpointRepository.GetByIdAsync(command.ShoppingListId.Value);
            shoppingListEntity ??= await GetFromStreamAsync(command.ShoppingListId.Value);

            if (shoppingListEntity is not null)
            {
                if (shoppingListEntity.CreatedBy == _userService.CurrentUserName() ||_userService.CurrentUserType() == Service.Models.User.UserType.Admin)
                {
                    foreach (var sharedListShopperId in command.SharedListShopperIds)
                    shoppingListEntity.SharedListShopperIds.Remove(sharedListShopperId);

                var evtPayload = new UpdateShoppingList(
                    shoppingListEntity.Id,
                    shoppingListEntity.Name,
                    shoppingListEntity.ShoppingListType,
                    shoppingListEntity.SelectedStoreIds,
                    shoppingListEntity.SharedListShopperIds,
                    shoppingListEntity.ListItemIds,
                    shoppingListEntity.SortOrder);
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
                    result = Result<ShoppingListRecord>.Error($"ShoppingList does not belong to '{_userService.CurrentUserId}'");
                }
            }
            else
            {
                result = Result<ShoppingListRecord>.Error($"ShoppingList does not exist '{command.ShoppingListId.Value}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ShoppingListRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(RemoveSharedListShopperToShoppingListCommand command) =>
        $"Failed to update shoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}