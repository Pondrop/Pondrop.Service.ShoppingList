using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pondrop.Service.Interfaces;
using Pondrop.Service.ShoppingList.Api.Services;
using Pondrop.Service.ShoppingList.Application.Commands;
using Pondrop.Service.ShoppingList.Application.Queries;

namespace Pondrop.Service.ShoppingList.ApiControllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ShoppingListController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly ILogger<ShoppingListController> _logger;

    public ShoppingListController(
        IMediator mediator,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        ILogger<ShoppingListController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
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
                await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateShoppingList([FromBody] UpdateShoppingListCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = i!.Id });
                return new OkObjectResult(i);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

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