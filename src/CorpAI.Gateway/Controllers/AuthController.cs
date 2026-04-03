using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Security.Claims;

namespace CorpAI.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var keycloakUrl = _config["Keycloak:Authority"];
            var clientId = _config["Keycloak:ClientId"];
            var clientSecret = _config["Keycloak:ClientSecret"];

            var tokenResponse = await GetKeycloakTokenAsync(request.Username, request.Password, clientId, clientSecret);
            if (!tokenResponse.IsSuccessStatusCode)
                return Unauthorized(new { Message = "Invalid credentials" });

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
            if (tokenData == null) return Unauthorized(new { Message = "Invalid token response" });

            var claims = DecodeIdToken(tokenData.Id_Token);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Ok(new { Success = true, Message = "Logged in successfully", User = new { Username = request.Username, Email = tokenData.Email, Name = tokenData.Name } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return StatusCode(500, new { Message = "Authentication failed" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { Success = true, Message = "Logged out successfully" });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
        var username = User.Identity?.Name ?? "";
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        return Ok(new { IsAuthenticated = isAuthenticated, Username = username, Email = email });
    }

    private async Task<HttpResponseMessage> GetKeycloakTokenAsync(string username, string password, string clientId, string clientSecret)
    {
        var client = _httpClientFactory.CreateClient();
        var form = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "username", username },
            { "password", password },
            { "grant_type", "password" },
            { "scope", "openid profile email" }
        };
        return await client.PostAsync($"{_config["Keycloak:Authority"]}/protocol/openid-connect/token", new FormUrlEncodedContent(form));
    }

    private IEnumerable<Claim> DecodeIdToken(string jwt)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(jwt);
        return jwtToken.Claims.Select(c => new Claim(c.Type, c.Value));
    }
}

public record LoginRequest(string Username, string Password);
public record KeycloakTokenResponse { public string Access_Token { get; set; } = ""; public string Id_Token { get; set; } = ""; public string Refresh_Token { get; set; } = ""; public int Expires_In { get; set; } public string Email { get; set; } = ""; public string Name { get; set; } = ""; }