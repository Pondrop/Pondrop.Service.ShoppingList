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

public class DeleteShoppingListCommandHandler : DirtyCommandHandler<ShoppingListEntity, DeleteShoppingListCommand, Result<List<ShoppingListRecord>>>
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

    public override async Task<Result<List<ShoppingListRecord>>> Handle(DeleteShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update ShoppingList failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ShoppingListRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ShoppingListRecord>>);

        try
        {
            var entities = new List<ShoppingListEntity>();
            foreach (var ShoppingList in command.Ids)
            {
                var ShoppingListEntity = await _ShoppingListCheckpointRepository.GetByIdAsync(ShoppingList);
                ShoppingListEntity ??= await GetFromStreamAsync(ShoppingList);

                if (ShoppingListEntity is not null)
                {
                    if (ShoppingListEntity.CreatedBy == _userService.CurrentUserName() || _userService.CurrentUserType() == Service.Models.User.UserType.Admin)
                    {
                        var evtPayload = new DeleteShoppingList(
                        ShoppingList);
                        var createdBy = _userService.CurrentUserName();

                        var success = await UpdateStreamAsync(ShoppingListEntity, evtPayload, createdBy);

                        if (!success)
                        {
                            await _ShoppingListCheckpointRepository.FastForwardAsync(ShoppingListEntity);
                            success = await UpdateStreamAsync(ShoppingListEntity, evtPayload, createdBy);
                        }

                        entities.Add(ShoppingListEntity);

                        await Task.WhenAll(
                            InvokeDaprMethods(ShoppingListEntity.Id, ShoppingListEntity.GetEvents(ShoppingListEntity.AtSequence)));
                    }
                    else
                    {
                        result = Result<List<ShoppingListRecord>>.Error($"ShoppingList does not belong to '{_userService.CurrentUserId}'");
                    }
                }
                else
                {
                    result = Result<List<ShoppingListRecord>>.Error($"ShoppingList does not exist '{ShoppingList}'");
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

    private static string FailedToCreateMessage(DeleteShoppingListCommand command) =>
        $"Failed to update shoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}