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

public class UpdateShoppingListCommandHandler : DirtyCommandHandler<ShoppingListEntity, UpdateShoppingListCommand, Result<List<ShoppingListRecord>>>
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

    public override async Task<Result<List<ShoppingListRecord>>> Handle(UpdateShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update shoppingList failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ShoppingListRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ShoppingListRecord>>);

        try
        {
            var entities = new List<ShoppingListEntity>();
            foreach (var shoppingList in command.ShoppingLists)
            {
                var shoppingListEntity = await _shoppingListCheckpointRepository.GetByIdAsync(shoppingList.Id);
                shoppingListEntity ??= await GetFromStreamAsync(shoppingList.Id);

                if (shoppingListEntity is not null)
                {
                    if (shoppingListEntity.CreatedBy == _userService.CurrentUserName() ||_userService.CurrentUserType() == Service.Models.User.UserType.Admin)
                    {
                        var evtPayload = new UpdateShoppingList(
                            shoppingList.Id,
                            shoppingList.Name,
                            shoppingListEntity.ShoppingListType,
                        shoppingListEntity.SelectedStoreIds,
                        shoppingListEntity.SharedListShopperIds,
                        shoppingListEntity.ListItemIds,
                        shoppingList.SortOrder);
                        var createdBy = _userService.CurrentUserName();

                        var success = await UpdateStreamAsync(shoppingListEntity, evtPayload, createdBy);

                        if (!success)
                        {
                            await _shoppingListCheckpointRepository.FastForwardAsync(shoppingListEntity);
                            success = await UpdateStreamAsync(shoppingListEntity, evtPayload, createdBy);
                        }

                        await Task.WhenAll(
                            InvokeDaprMethods(shoppingListEntity.Id, shoppingListEntity.GetEvents(shoppingListEntity.AtSequence)));

                        if (success)
                            entities.Add(shoppingListEntity);
                    }
                    else
                    {
                        result = Result<List<ShoppingListRecord>>.Error($"ShoppingList does not belong to: '{_userService.CurrentUserId}'");
                        break;
                    }
                }
                else
                {
                    result = Result<List<ShoppingListRecord>>.Error($"ShoppingList does not exist '{shoppingList.Id}'");
                    break;
                }
            }
            result = entities != null && entities.Count() > 0
                        ? Result<List<ShoppingListRecord>>.Success(_mapper.Map<List<ShoppingListRecord>>(entities))
                        : Result<List<ShoppingListRecord>>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<List<ShoppingListRecord>>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateShoppingListCommand command) =>
        $"Failed to update shoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}