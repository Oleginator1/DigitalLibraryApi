using System.Threading.Tasks;

namespace DigitalLibraryApi.Middleware
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get the requested path
            var path = context.Request.Path.Value?.ToLower();

            // Get role from header (default: User)
            var role = context.Request.Headers["X-Role"].FirstOrDefault() ?? "User";

            // Protect admin endpoints
            if (path is not null && path.StartsWith("/admin"))
            {
                if (role != "Admin")
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: Admin role required.");
                    return; // stop pipeline
                }
            }

            // Allow request to continue
            await _next(context);
        }
    }

    // Extension method for easy registration
    public static class RoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleMiddleware>();
        }
    }
}
