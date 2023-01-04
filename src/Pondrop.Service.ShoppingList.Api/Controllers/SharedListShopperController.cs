using AspNetCore.Proxy;
using AspNetCore.Proxy.Options;
using Azure;
using Azure.Search.Documents.Indexes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Pondrop.Service.Interfaces;
using Pondrop.Service.SharedListShopper.Application.Commands;
using Pondrop.Service.ShoppingList.Api.Models;
using Pondrop.Service.ShoppingList.Api.Services;
using Pondrop.Service.ShoppingList.Api.Services.Interfaces;
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

    [HttpPost]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSharedListShopper([FromBody] CreateSharedListShopperCommand command)
    {

        var result = await _mediator.Send(command);
        return await result.MatchAsync<IActionResult>(
            async i =>
            {
                await _serviceBusService.SendMessageAsync(new UpdateSharedListShopperCheckpointByIdCommand() { Id = i!.Id });
                return StatusCode(StatusCodes.Status201Created, i);
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