using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// 1. PostgreSQL (Keycloak DB)
var postgres = builder.AddPostgres("postgres")
    .WithImage("postgres:15-alpine")
    .WithDataVolume("corpai-postgres-data")
    .WithLifetime(ContainerLifetime.Persistent);
var keycloakDb = postgres.AddDatabase("keycloak-db");

// 2. Keycloak
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak:26.0")
    .WithArgs("start-dev")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", $"jdbc:postgresql://postgres:5432/keycloak-db")
    .WithEnvironment("KC_DB_USERNAME", "postgres")
    .WithEnvironment("KC_DB_PASSWORD", "postgres")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_HOSTNAME", builder.Configuration["DOMAIN_NAME"] ?? "localhost")
    .WithReference(keycloakDb)
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithLifetime(ContainerLifetime.Persistent);

// 3. Ollama
var ollama = builder.AddContainer("ollama", "ollama/ollama:latest")
    .WithDataVolume("corpai-ollama-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHttpEndpoint(targetPort: 11434, name: "http");

// 4. ChromaDB
var chroma = builder.AddContainer("chroma", "chromadb/chroma:latest")
    .WithDataVolume("corpai-chroma-data")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithHttpEndpoint(targetPort: 8000, name: "http");

// 5. Open WebUI
var openWebUI = builder.AddContainer("open-webui", "ghcr.io/open-webui/open-webui:main")
    .WithDataVolume("corpai-openwebui-data")
    .WithEnvironment("OLLAMA_BASE_URL", "http://ollama:11434")
    .WithEnvironment("WEBUI_SECRET_KEY", "your-secure-secret-key")
    .WithEnvironment("ENABLE_OAUTH_SIGNUP", "true")
    .WithEnvironment("OPENID_PROVIDER_URL", "http://keycloak:8080/realms/corp-ai/.well-known/openid-configuration")
    .WithEnvironment("OPENID_CLIENT_ID", "open-webui")
    .WithEnvironment("OPENID_CLIENT_SECRET", "webui-secret")
    .WithHttpEndpoint(targetPort: 8080, name: "http")
    .WithLifetime(ContainerLifetime.Persistent);

// 6. Certbot
var certVolume = "corpai-certbot-data";
var certbot = builder.AddContainer("certbot", "certbot/dns-cloudflare:latest")
    .WithEnvironment("CF_API_EMAIL", builder.Configuration["CF_API_EMAIL"] ?? "admin@example.com")
    .WithEnvironment("CF_API_KEY", builder.Configuration["CF_API_KEY"] ?? "your-key")
    .WithArgs("certonly", "--dns-cloudflare", "--dns-cloudflare-propagation-seconds", "60", "--email", builder.Configuration["ADMIN_EMAIL"] ?? "admin@example.com", "--agree-tos", "--no-eff-email", "-d", builder.Configuration["DOMAIN_NAME"] ?? "localhost", "--keep-until-expiring")
    .WithDataVolume(certVolume, "/etc/letsencrypt")
    .WithLifetime(ContainerLifetime.Persistent);

// 7. Keycloak Initializer
var initializer = builder.AddProject<CorpAI_KeycloakInitializer>("initializer")
    .WithReference(keycloak)
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WaitFor(keycloak);

// 8. Gateway
var gateway = builder.AddProject<CorpAI_Gateway>("gateway")
    .WithReference(keycloak)
    .WithReference(ollama)
    .WithReference(chroma)
    .WithReference(openWebUI)
    .WithEnvironment("DOMAIN_NAME", builder.Configuration["DOMAIN_NAME"] ?? "localhost")
    .WithDataVolume(certVolume, "/certs", isReadOnly: true)
    .WithExternalHttpEndpoints()
    .WaitFor(initializer);

builder.Build().Run();