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
            
            var path = context.Request.Path.Value?.ToLower();

            
            var role = context.Request.Headers["X-Role"].FirstOrDefault() ?? "User";

            
            if (path is not null && path.StartsWith("/admin"))
            {
                if (role != "Admin")
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: Admin role required.");
                    return; 
                }
            }

          
            await _next(context);
        }
    }

 
    public static class RoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseRoleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleMiddleware>();
        }
    }
}
