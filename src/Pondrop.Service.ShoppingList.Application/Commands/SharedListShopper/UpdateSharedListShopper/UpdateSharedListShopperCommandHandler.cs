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
using Pondrop.Service.ShoppingList.Domain.Events.SharedListShopper;

namespace Pondrop.Service.ShoppingList.Application.Commands;

public class UpdateSharedListShopperCommandHandler : DirtyCommandHandler<SharedListShopperEntity, UpdateSharedListShopperCommand, Result<List<SharedListShopperRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<SharedListShopperEntity> _sharedListShopperCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateSharedListShopperCommand> _validator;
    private readonly ILogger<UpdateSharedListShopperCommandHandler> _logger;

    public UpdateSharedListShopperCommandHandler(
        IOptions<SharedListShopperUpdateConfiguration> SharedListShopperUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<SharedListShopperEntity> sharedListShopperCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateSharedListShopperCommand> validator,
        ILogger<UpdateSharedListShopperCommandHandler> logger) : base(eventRepository, SharedListShopperUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _sharedListShopperCheckpointRepository = sharedListShopperCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<List<SharedListShopperRecord>>> Handle(UpdateSharedListShopperCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update SharedListShopper failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<List<SharedListShopperRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<SharedListShopperRecord>>);

        try
        {
            var entities = new List<SharedListShopperEntity>();
            foreach (var sharedListShopper in command.SharedListShoppers)
            {
                var sharedListShopperEntity = await _sharedListShopperCheckpointRepository.GetByIdAsync(sharedListShopper.Id);
                sharedListShopperEntity ??= await GetFromStreamAsync(sharedListShopper.Id);

                if (sharedListShopperEntity is not null)
                {
                    if (sharedListShopperEntity.CreatedBy == _userService.CurrentUserName() ||_userService.CurrentUserType() == Service.Models.User.UserType.Admin)
                    {
                        var evtPayload = new UpdateSharedListShopper(
                        sharedListShopper.Id,
                        sharedListShopper.UserId,
                        sharedListShopperEntity.ListPrivilege,
                        sharedListShopper.SortOrder
                        );
                    var createdBy = _userService.CurrentUserName();

                    var success = await UpdateStreamAsync(sharedListShopperEntity, evtPayload, createdBy);

                    if (!success)
                    {
                        await _sharedListShopperCheckpointRepository.FastForwardAsync(sharedListShopperEntity);
                        success = await UpdateStreamAsync(sharedListShopperEntity, evtPayload, createdBy);
                    }

                    entities.Add(sharedListShopperEntity);

                    await Task.WhenAll(
                        InvokeDaprMethods(sharedListShopperEntity.Id, sharedListShopperEntity.GetEvents(sharedListShopperEntity.AtSequence)));
                    }
                    else
                    {
                        result = Result<List<SharedListShopperRecord>>.Error($"List Item does not belong to '{_userService.CurrentUserId}'");
                    }
                }
                else
                {
                    result = Result<List<SharedListShopperRecord>>.Error($"List Item does not exist '{sharedListShopper}'");
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

    private static string FailedToCreateMessage(UpdateSharedListShopperCommand command) =>
        $"Failed to update SharedListShopper\nCommand: '{JsonConvert.SerializeObject(command)}'";
}