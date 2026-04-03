using CorpAI.Gateway.Middleware;
using CorpAI.Gateway.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<IThemeGeneratorService, ThemeGeneratorService>();
builder.Services.AddSingleton<IAssetDownloaderService, AssetDownloaderService>();
builder.Services.AddHttpClient<IAssetDownloaderService, AssetDownloaderService>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.ClientId = builder.Configuration["Keycloak:ClientId"];
    options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.UsePkce = true;
    
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            var isPopup = context.Request.Query.ContainsKey("popup") || 
                          context.Request.Headers["X-Auth-Mode"] == "popup";

            if (isPopup)
            {
                context.HandleResponse();
                var response = new
                {
                    type = "auth_redirect",
                    url = context.ProtocolMessage.CreateAuthenticationRequestUrl()
                };
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(response);
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("admin"));
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(15);
    });
    options.AddFixedWindowLimiter("AiApiLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
});

var app = builder.Build();

// Middleware
app.UseMiddleware<DynamicCssMiddleware>();
app.UseStaticFiles();

// CSP Enforcement
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && !path.StartsWith("/webui"))
    {
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self'; " +
            "img-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-src 'self';");
    }
    else
    {
        // Relaxed for Open WebUI
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' ws: wss: http://ollama:11434; " +
            "frame-src 'self'; " +
            "worker-src 'self' blob:;");
    }
    await next();
});

app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(15) });
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Callback for Popup Auth
app.MapGet("/signin-callback", async (HttpContext context) =>
{
    try
    {
        await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(@"
            <html><body style='background:none;margin:0;padding:0;'>
                <script>
                    window.opener.postMessage('auth-success', '*');
                    window.close();
                </script>
                <p>Authentication successful. Closing window...</p>
            </body></html>");
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Authentication failed");
    }
});

app.MapControllers();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");

app.Run();