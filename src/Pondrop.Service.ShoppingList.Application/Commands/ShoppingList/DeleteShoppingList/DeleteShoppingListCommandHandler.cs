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

public class DeleteShoppingListCommandHandler : DirtyCommandHandler<ShoppingListEntity, DeleteShoppingListCommand, Result<ShoppingListRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<ShoppingListEntity> _ShoppingListCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<DeleteShoppingListCommand> _validator;
    private readonly ILogger<DeleteShoppingListCommandHandler> _logger;

    public DeleteShoppingListCommandHandler(
        IOptions<ShoppingListUpdateConfiguration> shoppingListUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<ShoppingListEntity> ShoppingListCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<DeleteShoppingListCommand> validator,
        ILogger<DeleteShoppingListCommandHandler> logger) : base(eventRepository, shoppingListUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _ShoppingListCheckpointRepository = ShoppingListCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ShoppingListRecord>> Handle(DeleteShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update list item failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListRecord>.Error(errorMessage);
        }

        var result = default(Result<ShoppingListRecord>);

        try
        {
            var ShoppingListEntity = await _ShoppingListCheckpointRepository.GetByIdAsync(command.Id.Value);
            ShoppingListEntity ??= await GetFromStreamAsync(command.Id.Value);

            if (ShoppingListEntity is not null)
            {
                if (ShoppingListEntity.CreatedBy == _userService.CurrentUserName() || _userService.CurrentUserName() == "admin")
                {
                    var evtPayload = new DeleteShoppingList(
                    command.Id.Value);
                    var createdBy = _userService.CurrentUserName();

                    var success = await UpdateStreamAsync(ShoppingListEntity, evtPayload, createdBy);

                    if (!success)
                    {
                        await _ShoppingListCheckpointRepository.FastForwardAsync(ShoppingListEntity);
                        success = await UpdateStreamAsync(ShoppingListEntity, evtPayload, createdBy);
                    }

                    await Task.WhenAll(
                        InvokeDaprMethods(ShoppingListEntity.Id, ShoppingListEntity.GetEvents(ShoppingListEntity.AtSequence)));

                    result = success
                              ? Result<ShoppingListRecord>.Success(_mapper.Map<ShoppingListRecord>(ShoppingListEntity))
                              : Result<ShoppingListRecord>.Error(FailedToCreateMessage(command));
                }
                else
                {
                    result = Result<ShoppingListRecord>.Error(FailedToCreateMessage(command));
                }
            }
            else
            {
                result = Result<ShoppingListRecord>.Error($"Shopping List does not exist '{command.Id}'");
            }

    }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ShoppingListRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(DeleteShoppingListCommand command) =>
        $"Failed to update shoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}