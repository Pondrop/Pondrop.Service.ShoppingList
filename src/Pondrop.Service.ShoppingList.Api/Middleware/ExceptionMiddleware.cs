using FluentValidation;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;

namespace Pondrop.Service.ShoppingList.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Exception thrown for {0} {1}", context.Request.Method, context.Request.GetDisplayUrl());

        var code = HttpStatusCode.InternalServerError; // 500 if unexpected
        if (exception is ValidationException) code = HttpStatusCode.BadRequest;
        context.Response.StatusCode = (int)code;

        var result = "An error has occurred";

        return context.Response.WriteAsync(result);
    }
}
