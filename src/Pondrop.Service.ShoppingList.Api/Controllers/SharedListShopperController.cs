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
public class SharedListShopperController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IServiceBusService _serviceBusService;
    private readonly IRebuildCheckpointQueueService _rebuildCheckpointQueueService;
    private readonly ILogger<SharedListShopperController> _logger;

    public SharedListShopperController(
        IMediator mediator,
        IServiceBusService serviceBusService,
        IRebuildCheckpointQueueService rebuildCheckpointQueueService,
        ILogger<SharedListShopperController> logger)
    {
        _mediator = mediator;
        _serviceBusService = serviceBusService;
        _rebuildCheckpointQueueService = rebuildCheckpointQueueService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllSharedListShoppers()
    {
        var result = await _mediator.Send(new GetAllSharedListShoppersQuery());
        return result.Match<IActionResult>(
            i => new OkObjectResult(i),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSharedListShopperById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetSharedListShopperByIdQuery() { Id = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }

    [HttpGet]
    [Route("byshoppinglistid/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSharedListShopperByShoppingListId([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetSharedListShopperByShoppingListIdQuery() { ShoppingListId = id });
        return result.Match<IActionResult>(
            i => i is not null ? new OkObjectResult(i) : new NotFoundResult(),
            (ex, msg) => new BadRequestObjectResult(msg));
    }



    //[HttpPost]
    //[Route("create")]
    //[ProducesResponseType(StatusCodes.Status201Created)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> CreateSharedListShopper([FromBody] CreateSharedListShopperCommand command)
    //{

    //    var result = await _mediator.Send(command);
    //    return await result.MatchAsync<IActionResult>(
    //        async items =>
    //        {
    //            if (items is null)
    //                return StatusCode(StatusCodes.Status400BadRequest);

    //            foreach (var i in items)
    //            {
    //                await _serviceBusService.SendMessageAsync(new UpdateSharedListShopperCheckpointByIdCommand() { Id = i!.Id });
    //            }

    //            var updateResult = await _mediator!.Send(new AddSharedListShoppersToShoppingListCommand() { ShoppingListId = command.ShoppingListId, SharedListShopperIds = items?.Select(i => i.Id)?.ToList() });
    //            return await updateResult.MatchAsync<IActionResult>(
    //                async s =>
    //                {
    //                    await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = s!.Id });
    //                    return StatusCode(StatusCodes.Status201Created, items);
    //                },
    //                (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg))); ;
    //        },
    //        (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    //}

    [HttpPut]
    [Route("update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSharedListShopper([FromBody] UpdateSharedListShopperCommand command)
    {
        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async items =>
            {
                if (items != null)
                    foreach (var item in items)
                        await _serviceBusService.SendMessageAsync(new UpdateSharedListShopperCheckpointByIdCommand() { Id = item!.Id });

                return new OkObjectResult(items);
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }


    [HttpDelete]
    [Route("remove")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveSharedListShopper([FromBody] DeleteSharedListShopperCommand command)
    {

        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async items =>
            {
                if (items is null)
                    return StatusCode(StatusCodes.Status400BadRequest);

                foreach (var i in items)
                {
                    await _serviceBusService.SendMessageAsync(new UpdateSharedListShopperCheckpointByIdCommand() { Id = i!.Id });
                }

                var updateResult = await _mediator!.Send(new RemoveSharedListShopperToShoppingListCommand() { ShoppingListId = command.ShoppingListId, SharedListShopperIds = items?.Select(i => i.Id)?.ToList() });
                return await updateResult.MatchAsync<IActionResult>(
                    async s =>
                    {
                        await _serviceBusService.SendMessageAsync(new UpdateShoppingListCheckpointByIdCommand() { Id = s!.Id });
                        return StatusCode(StatusCodes.Status201Created, items);
                    },
                    (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg))); ;
            },
            (ex, msg) => Task.FromResult<IActionResult>(new BadRequestObjectResult(msg)));
    }

    [HttpPost]
    [Route("update/checkpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] UpdateSharedListShopperCheckpointByIdCommand command)
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
        _rebuildCheckpointQueueService.Queue(new RebuildSharedListShopperCheckpointCommand());
        return new AcceptedResult();
    }
}