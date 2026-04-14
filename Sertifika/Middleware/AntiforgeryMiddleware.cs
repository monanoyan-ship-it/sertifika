using Microsoft.AspNetCore.Antiforgery;

namespace Sertifika.Middleware;

public class AntiforgeryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery)
    {
        _next = next;
        _antiforgery = antiforgery;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;

        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
        {
            var tokens = _antiforgery.GetAndStoreTokens(context);
            if (tokens.RequestToken != null)
            {
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions
                {
                    HttpOnly = false,
                    Secure = context.Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                });
            }
        }
        else if (ShouldValidate(context))
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "CSRF dogrulamasi basarisiz." });
                return;
            }
        }

        await _next(context);
    }

    private static bool ShouldValidate(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/api/auth/register", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/api/onedrive-accounts/oauth/callback", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("/api/certificates/verify", StringComparison.OrdinalIgnoreCase)) return false;
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }
}

public static class AntiforgeryMiddlewareExtensions
{
    public static IApplicationBuilder UseAntiforgeryTokens(this IApplicationBuilder app)
        => app.UseMiddleware<AntiforgeryMiddleware>();
}
