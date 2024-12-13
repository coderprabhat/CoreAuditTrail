using Serilog;

namespace CoreAuditTrail.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capture the user's IP address
            var userIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

            // Capture the user's name if available
            var userName = context.User.Identity?.Name ?? "Anonymous";

            // Capture the API URL
            var apiUrl = context.Request.Path;

            // Log the information
            Log.Information($"API Request: IP Address: {userIpAddress}, User Name: {userName}, API URL: {apiUrl}");

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

}
