using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Web;

namespace HamsterStudioGUI.Debug;

public class RouteDebugMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RouteDebugMiddleware> _logger;

    public RouteDebugMiddleware(RequestDelegate next, ILogger<RouteDebugMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        if (endpoint != null)
        {
            _logger.LogInformation($"Matched Route: {endpoint.DisplayName}");

            var routePattern = (endpoint as RouteEndpoint)?.RoutePattern?.RawText;
            if (!string.IsNullOrEmpty(routePattern))
            {
                _logger.LogInformation($"Route Pattern: {routePattern}");
            }

            // 记录路由参数
            var routeValues = context.Request.RouteValues;
            foreach (var kvp in routeValues)
            {
                _logger.LogInformation($"Route Parameter - {kvp.Key}: {kvp.Value}");
            }
        }
        else
        {
            _logger.LogWarning("No endpoint matched for: {Path}", HttpUtility.UrlDecode(context.Request.Path));
        }

        await _next(context);
    }
}
