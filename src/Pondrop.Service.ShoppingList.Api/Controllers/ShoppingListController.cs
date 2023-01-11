using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.ShoppingList.Api.Services;
using Pondrop.Service.ShoppingList.Application.Commands;
using Pondrop.Service.ShoppingList.Application.Queries;
using Pondrop.Service.ShoppingList.Domain.Models;

namespace Pondrop.Service.ShoppingList.ApiControllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ShoppingListController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IUserService _userService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly ILogger<ShoppingListController> _logger;

    public ShoppingListController(
        IMediator mediator,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        IUserService userService,
        ILogger<ShoppingListController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
        _userService = userService;
        _rebuildCheckpointQueueService = rebuildCheckpointQueueService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllShoppingLists()
    {
        var result = await _mediator.Send(new GetAllShoppingListsQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetShoppingListById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetShoppingListByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShoppingList([FromBody] CreateShoppingListCommand command)
    {

        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _mediator.Send(new UpdateShoppingListCheckpointByIdCommand() { Id = i!.Id });
                var sharedListShopperResult = await _mediator.Send(new CreateSharedListShopperCommand()
                {
                    ShoppingListId = i!.Id,
                    SharedListShoppers = new List<SharedListShopperCreateRecord>() { new SharedListShopperCreateRecord()
                    { ListPrivilege = Domain.Enums.ShoppingList.ListPrivilegeType.admin, UserId = new Guid(_userService.CurrentUserId()), SortOrder = command.SortOrder } }
                });

                return await sharedListShopperResult.MatchAsync<IActionResult>(
                     async s =>
                     {
                         await _mediator.Send(new UpdateSharedListShopperCheckpointByIdCommand() { Id = s!.FirstOrDefault().Id });
                         var addShopperToShoppingListResult = await _mediator.Send(new AddSharedListShoppersToShoppingListCommand()
                         {
                             ShoppingListId = i!.Id,
                             SharedListShopperIds = new List<Guid>() { s!.FirstOrDefault().Id }
                         });

                         return await addShopperToShoppingListResult.MatchAsync<IActionResult>(
                             async a =>
                             {
                                 await _mediator.Send(new UpdateShoppingListCheckpointByIdCommand() { Id = a!.Id });
                                 var shoppingList = await _mediator.Send(new GetShoppingListByIdQuery() { Id = a!.Id });
                                 return StatusCode(StatusCodes.Status201Created, shoppingList.Value);
                                 i!.SharedListShopperIds.Add(sharedListShopperResult.Value.FirstOrDefault().Id);
                             }, (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
                     }, (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPut]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateShoppingList([FromBody] UpdateShoppingListCommand command)
    {
        var result = await _mediator.Send(command);
        var resultItems = new List<ShoppingListResponseRecord>();
        return await result.MatchAsync<IActionResult>(
            async items =>
            {

                if (items != null)
                    foreach (var item in items)
                    {
                        await _mediator.Send(new UpdateShoppingListCheckpointByIdCommand() { Id = item!.Id });
                        var sharedListShopperResponse = await _mediator.Send(new GetSharedListShopperByShoppingListIdQuery() { ShoppingListId = item.Id });
                        var sharedListShopper = sharedListShopperResponse.Value;
                        var sharedListShopperResult = await _mediator.Send(new UpdateSharedListShopperCommand()
                        {
                            ShoppingListId = item!.Id,
                            SharedListShoppers = new List<SharedListShopperUpdateRecord>() { new SharedListShopperUpdateRecord()
                    {Id =  sharedListShopper.FirstOrDefault(s => s.UserId == new Guid(_userService.CurrentUserId()))?.Id ?? Guid.Empty, UserId = new Guid(_userService.CurrentUserId()), SortOrder = command.ShoppingLists.FirstOrDefault(s => s.Id == item!.Id)?.SortOrder ?? 0 } }
                        });

                        await sharedListShopperResult.MatchAsync<IActionResult>(
                             async s =>
                             {
                                 await _mediator.Send(new UpdateSharedListShopperCheckpointByIdCommand() { Id = s!.FirstOrDefault().Id });
                                 var result = await _mediator.Send(new GetShoppingListByIdQuery() { Id = item!.Id });
                                 if (result.Value != null)
                                     resultItems.Add(result.Value);
                                 return StatusCode(StatusCodes.Status202Accepted);
                             },
                             (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
                    }


                return new OkObjectResult(resultItems);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    //[HttpDelete]
    //[Route("remove")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> RemoveShoppingList([FromBody] DeleteShoppingListCommand command)
    //{
    //    var result = await _mediator.Send(command);
    //    return await result.MatchAsync<IActionResult>(
    //       async items =>
    //       {
    //           if (items != null)
    //               foreach (var item in items)
    //                   await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = item!.Id });

    //            return new OkObjectResult(items);
    //        },
    //        (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    //}

    //[HttpPost]
    //[Route("listitem/add")]
    //[ProducesResponseType(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> AddListItemToShoppingList([FromBody] AddListItemToShoppingListCommand command)
    //{
    //    var result = await _mediator.Send(command);
    //    return await result.MatchAsync<IActionResult>(
    //        async i =>
    //        {
    //            await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = i!.Id });
    //            return StatusCode(StatusCodes.Status201Created, i);
    //        },
    //        (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    //}

    //[HttpPost]
    //[Route("listitem/remove")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> RemoveListItemToShoppingList([FromBody] RemoveListItemToShoppingListCommand command)
    //{
    //    var result = await _mediator.Send(command);
    //    return await result.MatchAsync<IActionResult>(
    //        async i =>
    //        {
    //            await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = i!.Id });
    //            return new OkObjectResult(i);
    //        },
    //        (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    //}

    //[HttpPost]
    //[Route("sharedlistshopper/add")]
    //[ProducesResponseType(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> AddSharedListShopperToShoppingList([FromBody] AddSharedListShopperShoppingListCommand command)
    //{
    //    var result = await _mediator.Send(command);
    //    return await result.MatchAsync<IActionResult>(
    //        async i =>
    //        {
    //            await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = i!.Id });
    //            return StatusCode(StatusCodes.Status201Created, i);
    //        },
    //        (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    //}

    //[HttpPost]
    //[Route("sharedlistshopper/remove")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> RemoveSharedListShopperToShoppingList([FromBody] RemoveSharedListShopperToShoppingListCommand command)
    //{
    //    var result = await _mediator.Send(command);
    //    return await result.MatchAsync<IActionResult>(
    //        async i =>
    //        {
    //            await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = i!.Id });
    //            return new OkObjectResult(i);
    //        },
    //        (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    //}

    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateShoppingListCheckpointByIdCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpPost]
    [Route("rebuild/checkpoint")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult RebuildCheckpoint()
    {
        _rebuildCheckpointQueueService.Queue(new RebuildShoppingListCheckpointCommand());
        return new AcceptedResult();
    }
}