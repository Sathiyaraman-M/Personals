using Personals.Common.Serialization;
using Personals.Common.Wrappers.Abstractions;
using Personals.Infrastructure.Exceptions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Personals.Server.Middlewares;

public partial class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, e,
                ResultSerializationContext.Default.ValidationFailedResult);
        }
        catch (InvalidOperationException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, e,
                ResultSerializationContext.Default.ValidationFailedResult);
        }
        catch (EntityNotFoundException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.NotFound, e,
                ResultSerializationContext.Default.ObjectNotFoundResult);
        }
        catch (DatabaseOperationFailedException e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, e,
                ResultSerializationContext.Default.GenericFailedResult);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, e,
                ResultSerializationContext.Default.GenericFailedResult);
        }
    }

    private async Task HandleExceptionAsync<TResult>(HttpContext context, HttpStatusCode code, Exception exception,
        JsonTypeInfo<TResult> jsonTypeInfo) where TResult : IFailedResult
    {
        LogError(logger, exception.Message, code, context.Request.Path, context.Request.Method, exception);
        var result = TResult.Fail(exception.Message);
        context.Response.StatusCode = (int)code;
        context.Response.ContentType = "application/json";
        var jsonString = JsonSerializer.Serialize(result, jsonTypeInfo);
        await context.Response.WriteAsync(jsonString);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error,
        Message =
            "{Message} \n StatusCode: {StatusCode} \n Request Path: {Request} \n Request Method: {RequestMethod}")]
    private static partial void LogError(ILogger logger, string message, HttpStatusCode statusCode, string request,
        string requestMethod, Exception e);
}