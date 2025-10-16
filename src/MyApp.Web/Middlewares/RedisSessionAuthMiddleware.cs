using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace MyApp.Web.Middlewares
{
    public class RedisSessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public RedisSessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDistributedCache cache)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sessionId = context.User.FindFirst("SessionId")?.Value;
                if (!string.IsNullOrEmpty(sessionId))
                {
                    var json = await cache.GetStringAsync($"session:{sessionId}");
                    if (string.IsNullOrEmpty(json))
                    {
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Session expired");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class RedisSessionAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseRedisSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RedisSessionAuthMiddleware>();
        }
    }
}
