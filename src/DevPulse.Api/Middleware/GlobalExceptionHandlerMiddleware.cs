using System.Net;
using System.Text.Json;
using DevPulse.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Extensions = { ["traceId"] = traceId }
        };

        switch (exception)
        {
            case ValidationException valEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Failed";
                problemDetails.Detail = valEx.Message;
                problemDetails.Extensions["errors"] = valEx.Errors;
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "Resource Not Found";
                problemDetails.Detail = notFoundEx.Message;
                problemDetails.Extensions["resource"] = notFoundEx.ResourceName;
                break;

            case RateLimitExceededException rateLimitEx:
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                problemDetails.Status = (int)HttpStatusCode.TooManyRequests;
                problemDetails.Title = "Rate Limit Exceeded";
                problemDetails.Detail = rateLimitEx.Message;
                if (rateLimitEx.ResetTimeUtc.HasValue)
                {
                    problemDetails.Extensions["resetTimeUtc"] = rateLimitEx.ResetTimeUtc.Value.ToString("O");
                }
                if (rateLimitEx.RemainingLimit.HasValue)
                {
                    problemDetails.Extensions["remaining"] = rateLimitEx.RemainingLimit.Value;
                }
                break;

            case DomainException domainEx:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                problemDetails.Status = (int)HttpStatusCode.UnprocessableEntity;
                problemDetails.Title = "Domain Rule Violation";
                problemDetails.Detail = domainEx.Message;
                break;

            case OperationCanceledException:
                context.Response.StatusCode = 499; // Client Closed Request
                problemDetails.Status = 499;
                problemDetails.Title = "Request Canceled";
                problemDetails.Detail = "The client canceled the request before completion.";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "An Unexpected Error Occurred";
                problemDetails.Detail = _env.IsDevelopment()
                    ? exception.ToString()
                    : "An internal server error occurred while processing your request. Please try again later.";
                break;
        }

        context.Response.ContentType = "application/problem+json";
        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
