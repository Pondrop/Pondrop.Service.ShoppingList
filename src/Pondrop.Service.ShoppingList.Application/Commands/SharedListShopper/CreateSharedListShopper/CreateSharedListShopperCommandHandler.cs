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

public class CreateSharedListShopperCommandHandler : DirtyCommandHandler<SharedListShopperEntity, CreateSharedListShopperCommand, Result<SharedListShopperRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateSharedListShopperCommand> _validator;
    private readonly ILogger<CreateSharedListShopperCommandHandler> _logger;

    public CreateSharedListShopperCommandHandler(
        IOptions<SharedListShopperUpdateConfiguration> sharedListShopperUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateSharedListShopperCommand> validator,
        ILogger<CreateSharedListShopperCommandHandler> logger) : base(eventRepository, sharedListShopperUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<SharedListShopperRecord>> Handle(CreateSharedListShopperCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create SharedListShopper failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<SharedListShopperRecord>.Error(errorMessage);
        }

        var result = default(Result<SharedListShopperRecord>);

        try
        {
            var SharedListShopperEntity = new SharedListShopperEntity(
                command.ShopperId,
                command.ListPrivilege,
                _userService.CurrentUserName());
          
            var success = await _eventRepository.AppendEventsAsync(SharedListShopperEntity.StreamId, 0, SharedListShopperEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(SharedListShopperEntity.Id, SharedListShopperEntity.GetEvents()));

            result = success
                ? Result<SharedListShopperRecord>.Success(_mapper.Map<SharedListShopperRecord>(SharedListShopperEntity))
                : Result<SharedListShopperRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<SharedListShopperRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateSharedListShopperCommand command) =>
        $"Failed to create SharedListShopper\nCommand: '{JsonConvert.SerializeObject(command)}'";
}