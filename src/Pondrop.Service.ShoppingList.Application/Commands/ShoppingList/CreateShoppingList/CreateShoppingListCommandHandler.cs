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

public class CreateShoppingListCommandHandler : DirtyCommandHandler<ShoppingListEntity, CreateShoppingListCommand, Result<ShoppingListRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateShoppingListCommand> _validator;
    private readonly ILogger<CreateShoppingListCommandHandler> _logger;

    public CreateShoppingListCommandHandler(
        IOptions<ShoppingListUpdateConfiguration> ShoppingListUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateShoppingListCommand> validator,
        ILogger<CreateShoppingListCommandHandler> logger) : base(eventRepository, ShoppingListUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<ShoppingListRecord>> Handle(CreateShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create ShoppingList failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListRecord>.Error(errorMessage);
        }

        var result = default(Result<ShoppingListRecord>);

        try
        {
            var ShoppingListEntity = new ShoppingListEntity(
                command.Name,
                command.ShoppingListType,
                command.Stores,
                new List<Guid>(),
                command.ListItemIds,
                _userService.CurrentUserName());
          
            var success = await _eventRepository.AppendEventsAsync(ShoppingListEntity.StreamId, 0, ShoppingListEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(ShoppingListEntity.Id, ShoppingListEntity.GetEvents()));

            result = success
                ? Result<ShoppingListRecord>.Success(_mapper.Map<ShoppingListRecord>(ShoppingListEntity))
                : Result<ShoppingListRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<ShoppingListRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateShoppingListCommand command) =>
        $"Failed to create ShoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}