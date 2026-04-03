using CorpAI.Gateway.Services;

namespace CorpAI.Gateway.Middleware;

public class DynamicCssMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IThemeGeneratorService _themeService;

    public DynamicCssMiddleware(RequestDelegate next, IThemeGeneratorService themeService)
    {
        _next = next;
        _themeService = themeService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (path != null && path.StartsWith("/css/theme.") && path.EndsWith(".css"))
        {
            var expected = $"/css/{_themeService.GetCssFileName()}";
            if (path == expected)
            {
                context.Response.ContentType = "text/css";
                context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
                await context.Response.WriteAsync(_themeService.GetCssContent());
                return;
            }
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not Found");
            return;
        }
        await _next(context);
    }
}