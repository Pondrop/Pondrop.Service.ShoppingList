using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.ShoppingList.Application.Models;

using Pondrop.Service.ShoppingList.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.ShoppingList.Domain.Events.SharedListShopper;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class DeleteSharedListShopperCommandHandler : DirtyCommandHandler<SharedListShopperEntity, DeleteSharedListShopperCommand, Result<List<SharedListShopperRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<SharedListShopperEntity> _SharedListShopperCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<DeleteSharedListShopperCommand> _validator;
    private readonly ILogger<DeleteSharedListShopperCommandHandler> _logger;

    public DeleteSharedListShopperCommandHandler(
        IOptions<ShoppingListUpdateConfiguration> shoppingListUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<SharedListShopperEntity> SharedListShopperCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<DeleteSharedListShopperCommand> validator,
        ILogger<DeleteSharedListShopperCommandHandler> logger) : base(eventRepository, shoppingListUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _SharedListShopperCheckpointRepository = SharedListShopperCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<List<SharedListShopperRecord>>> Handle(DeleteSharedListShopperCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update list item failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SharedListShopperRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SharedListShopperRecord>>);

        try
        {
            var entities = new List<SharedListShopperEntity>();
            foreach (var SharedListShopper in command.SharedListShopperIds)
            {
                var SharedListShopperEntity = await _SharedListShopperCheckpointRepository.GetByIdAsync(SharedListShopper);
                SharedListShopperEntity ??= await GetFromStreamAsync(SharedListShopper);

                if (SharedListShopperEntity is not null)
                {
                    if (SharedListShopperEntity.CreatedBy == _userService.CurrentUserName() ||_userService.CurrentUserType() == Service.Models.User.UserType.Admin)
                    {
                        var evtPayload = new DeleteSharedListShopper(
                        SharedListShopper);
                    var createdBy = _userService.CurrentUserName();

                    var success = await UpdateStreamAsync(SharedListShopperEntity, evtPayload, createdBy);

                    if (!success)
                    {
                        await _SharedListShopperCheckpointRepository.FastForwardAsync(SharedListShopperEntity);
                        success = await UpdateStreamAsync(SharedListShopperEntity, evtPayload, createdBy);
                    }

                    entities.Add(SharedListShopperEntity);

                    await Task.WhenAll(
                        InvokeDaprMethods(SharedListShopperEntity.Id, SharedListShopperEntity.GetEvents(SharedListShopperEntity.AtSequence)));
                    }
                    else
                    {
                        result = Result<List<SharedListShopperRecord>>.Error($"List Item does not belong to '{_userService.CurrentUserId}'");
                    }
                }
                else
                {
                    result = Result<List<SharedListShopperRecord>>.Error($"List Item does not exist '{SharedListShopper}'");
                }

            }
            result = entities != null && entities.Count() > 0
                      ? Result<List<SharedListShopperRecord>>.Success(_mapper.Map<List<SharedListShopperRecord>>(entities))
                      : Result<List<SharedListShopperRecord>>.Error(FailedToCreateMessage(command));


        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<List<SharedListShopperRecord>>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(DeleteSharedListShopperCommand command) =>
        $"Failed to update shoppingList\nCommand: '{JsonConvert.SerializeObject(command)}'";
}