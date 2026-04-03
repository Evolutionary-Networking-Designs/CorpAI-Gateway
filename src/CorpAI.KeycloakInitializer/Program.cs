using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var keycloakUrl = builder.Configuration["services:keycloak:http:0"] ?? "http://keycloak:8080";
var adminUser = builder.Configuration["KEYCLOAK_ADMIN"] ?? "admin";
var adminPassword = builder.Configuration["KEYCLOAK_ADMIN_PASSWORD"] ?? "admin";

var targetRealm = "corp-ai";
var targetClient = "dotnet-ai-client";
var targetClientSecret = "change-this-secret";

var httpClient = new HttpClient { BaseAddress = new Uri(keycloakUrl) };

Console.WriteLine("Waiting for Keycloak...");
await WaitForKeycloakAsync(httpClient);

Console.WriteLine("Authenticating...");
var adminToken = await GetAdminTokenAsync(httpClient, adminUser, adminPassword);
httpClient.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

Console.WriteLine($"Creating Realm: {targetRealm}...");
await CreateRealmAsync(httpClient, targetRealm);

Console.WriteLine($"Creating Client: {targetClient}...");
await CreateClientAsync(httpClient, targetRealm, targetClient, targetClientSecret);

Console.WriteLine("Creating Open WebUI Client...");
await CreateOpenWebUIClientAsync(httpClient, targetRealm);

Console.WriteLine("Creating Admin User...");
await CreateAdminUserAsync(httpClient, targetRealm, "admin@corp.internal");

Console.WriteLine("Enabling WebAuthn...");
await EnableWebAuthnAsync(httpClient, targetRealm, "localhost");

Console.WriteLine("Keycloak configuration complete.");

// --- Helpers ---

async Task WaitForKeycloakAsync(HttpClient client)
{
    for (int i = 0; i < 30; i++) { try { if ((await client.GetAsync("realms/master")).IsSuccessStatusCode) return; } catch { } await Task.Delay(5000); }
    throw new Exception("Keycloak did not start.");
}

async Task<string> GetAdminTokenAsync(HttpClient client, string user, string pass)
{
    var form = new Dictionary<string, string> { { "client_id", "admin-cli" }, { "username", user }, { "password", pass }, { "grant_type", "password" } };
    var response = await client.PostAsync("realms/master/protocol/openid-connect/token", new FormUrlEncodedContent(form));
    response.EnsureSuccessStatusCode();
    return (await response.Content.ReadFromJsonAsync<TokenResponse>())?.AccessToken ?? throw new Exception("No token");
}

async Task CreateRealmAsync(HttpClient client, string realm)
{
    var payload = new { realm = realm, enabled = true, theme = "corp-ai-theme", loginTheme = "corp-ai-theme" };
    var response = await client.PostAsJsonAsync("admin/realms", payload);
    if (response.StatusCode == System.Net.HttpStatusCode.Conflict) Console.WriteLine("Realm exists.");
    else response.EnsureSuccessStatusCode();
}

async Task CreateClientAsync(HttpClient client, string realm, string clientId, string secret)
{
    var payload = new { clientId = clientId, enabled = true, secret = secret, redirectUris = new[] { "https://localhost:7001/*" }, directAccessGrantsEnabled = true, standardFlowEnabled = true, publicClient = false };
    var response = await client.PostAsJsonAsync($"admin/realms/{realm}/clients", payload);
    if (response.StatusCode == System.Net.HttpStatusCode.Conflict) Console.WriteLine("Client exists.");
    else response.EnsureSuccessStatusCode();
}

async Task CreateOpenWebUIClientAsync(HttpClient client, string realm)
{
    var payload = new { clientId = "open-webui", enabled = true, secret = "webui-secret", redirectUris = new[] { "https://localhost:7001/webui/*" }, standardFlowEnabled = true };
    var response = await client.PostAsJsonAsync($"admin/realms/{realm}/clients", payload);
    if (response.StatusCode == System.Net.HttpStatusCode.Conflict) Console.WriteLine("WebUI Client exists.");
}

async Task CreateAdminUserAsync(HttpClient client, string realm, string email)
{
    var userPayload = new { username = "admin", email = email, enabled = true, credentials = new[] { new { type = "password", value = "admin", temporary = false } } };
    var response = await client.PostAsJsonAsync($"admin/realms/{realm}/users", userPayload);
    if (response.StatusCode == System.Net.HttpStatusCode.Conflict) { Console.WriteLine("User exists."); return; }
    response.EnsureSuccessStatusCode();
    // Role assignment logic omitted for brevity
}

async Task EnableWebAuthnAsync(HttpClient client, string realm, string domain)
{
    var payload = new { webAuthnPolicyRpId = domain };
    var response = await client.PutAsJsonAsync($"admin/realms/{realm}", payload);
    Console.WriteLine(response.IsSuccessStatusCode ? "WebAuthn enabled." : "Failed to enable WebAuthn.");
}

internal record TokenResponse(string AccessToken);