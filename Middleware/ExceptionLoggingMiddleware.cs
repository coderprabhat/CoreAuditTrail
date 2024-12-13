using Serilog;

namespace CoreAuditTrail.Middleware
{
    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Proceed to the next middleware
            }
            catch (Exception ex)
            {
                // Log the detailed exception
                Log.Error(ex, "An unhandled exception occurred while processing the request. " +
                    "API: {ApiUrl}, IP: {UserIpAddress}, Exception: {ExceptionMessage}",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP",
                    ex.Message);

                // Respond with a generic error message
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("An unexpected error occurred. Please check the logs for details.");
            }
        }
    }

}
