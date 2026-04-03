Corp AI Gateway
===============

A **privacy-first, enterprise-grade AI gateway** built on .NET 10, designed for internal corporate use. This system provides a secure, self-hosted interface to Large Language Models (LLMs) via Ollama, with complete data sovereignty and zero external dependencies.

* * *

🏗️ Architecture Overview
-------------------------

    ┌─────────────────────────────────────────────────────────────────────────────┐
    │                              Mac Mini Host                                  │
    │                                                                             │
    │  ┌─────────────────────────────────────────────────────────────────────┐    │
    │  │                     .NET 10 Gateway (YARP)                          │    │
    │  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────────┐  │    │
    │  │  │   MVC UI    │  │  Admin API  │  │     Reverse Proxy (YARP)    │  │    │
    │  │  │  (Razor)    │  │  (Assets)   │  │  /api/chat → Ollama         │  │    │
    │  │  │             │  │             │  │  /admin/keycloak → Keycloak │  │    │
    │  │  │  /webui     │  │             │  │  /admin/ollama → Ollama     │  │    │
    │  │  │  (iframe)   │  │             │  │  /webui → Open WebUI        │  │    │
    │  │  └─────────────┘  └─────────────┘  └─────────────────────────────┘  │    │
    │  └─────────────────────────────────────────────────────────────────────┘    │
    │                          │                                                  │
    │         ┌────────────────┼────────────────┬─────────────────┐               │
    │         ▼                ▼                ▼                 ▼               │
    │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
    │  │  Keycloak   │  │   Ollama    │  │  ChromaDB   │  │ Open WebUI  │         │
    │  │   (OIDC)    │  │    (LLM)    │  │  (Vector)   │  │   (Chat)    │         │
    │  │             │  │             │  │             │  │             │         │
    │  │  ┌───────┐  │  │  Llama 3    │  │ Embeddings  │  │  RAG/Docs   │         │
    │  │  │PG DB  │  │  │  Mistral    │  │ Collections │  │  History    │         │
    │  │  └───────┘  │  │  etc.       │  │             │  │             │         │
    │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘         │
    │                                                                             │
    │  ┌─────────────┐                                                            │
    │  │   Certbot   │  ← DNS-01 Challenge (Let's Encrypt)                        │
    │  │  (Sidecar)  │                                                            │
    │  └─────────────┘                                                            │
    │                                                                             │
    └─────────────────────────────────────────────────────────────────────────────┘
    

* * *

🚀 Features
-----------

### Core Capabilities

*   **🔒 Privacy-First Architecture**: All data stays on-premises. No external API calls.
*   **🤖 Multi-Model Support**: Run Llama 3, Mistral, and other models via Ollama.
*   **🔐 Enterprise SSO**: Keycloak integration with Passkey/WebAuthn support.
*   **📊 Vector Database**: ChromaDB for RAG (Retrieval Augmented Generation) workflows.
*   **🎨 Modern UI**: MudBlazor-inspired Material Design with strict CSP compliance.

### Security Features

*   **Strict CSP**: No `unsafe-inline` scripts or styles.
*   **Dynamic CSS Generation**: Hashed, cache-busted stylesheets.
*   **API-Driven Auth**: Password grant flow with rate limiting.
*   **Zero External Dependencies**: All assets downloaded and served locally.
*   **Passkey Support**: WebAuthn for passwordless authentication.

### Developer Experience

*   **Aspire Orchestration**: One-command deployment of entire stack.
*   **Hot-Reload Certificates**: Dynamic TLS without restarts.
*   **Admin Dashboard**: Download fonts, frameworks, and manage assets.
*   **Full Observability**: Aspire dashboard with logs, traces, and metrics.

* * *

🛠️ Technology Stack
--------------------

| Component | Technology | Version |
| --------- | ---------- | ------- |
| **Runtime** | .NET | 10.0 |
| **Web Framework** | ASP.NET Core MVC | 10.0 |
| **Reverse Proxy** | YARP | 2.2.0 |
| **Identity Provider** | Keycloak | 26.0 |
| **LLM Engine** | Ollama | Latest |
| **Vector DB** | ChromaDB | Latest |
| **Chat UI** | Open WebUI | Latest |
| **Database** | PostgreSQL | 15 |
| **Orchestration** | .NET Aspire | 9.0 |
| **DOM Manipulation** | AngleSharp | 1.1.2 |
| **Testing** | NUnit | 4.2.2 |

* * *

📋 Prerequisites
----------------

### Development Machine

*   **.NET 10 SDK** ([Download](https://dotnet.microsoft.com/download))
*   **Docker Desktop** ([Download](https://www.docker.com/products/docker-desktop))
*   **Git** ([Download](https://git-scm.com/downloads))

### Deployment Target (Mac Mini)

*   **macOS 12.0+** (Monterey or later)
*   **64GB RAM** (recommended for larger models)
*   **Apple Silicon** (M1/M2/M3/M4)
*   **Docker Desktop for Mac**

### DNS Configuration

*   A domain pointed to your Mac Mini's IP (e.g., `ai.corp.internal`)
*   Cloudflare API credentials for DNS-01 challenge

* * *

📦 Installation
---------------

### 1\. Clone the Repository

`git clone https://github.com/your-org/CorpAI-Gateway.git cd CorpAI-Gateway`

### 2\. Configure Secrets

Create an Ansible Vault file for sensitive configuration:

`cd ansible ansible-vault edit group_vars/all.yml`

**Required Variables:**

`domain_name: "ai.yourcompany.com" admin_email: "admin@yourcompany.com" cf_api_email: "your-cloudflare-email@example.com" cf_api_key: "YOUR_CLOUDFLARE_API_KEY" postgres_password: "STRONG_PASSWORD_HERE" keycloak_admin_password: "STRONG_PASSWORD_HERE" keycloak_client_secret: "STRONG_SECRET_HERE"`

### 3\. Deploy Infrastructure (Optional: Ansible)

For production deployment to a Mac Mini:

`cd ansible ansible-playbook playbooks/site.yml --ask-vault-pass`

### 4\. Run Locally (Aspire)

For development and testing:

`cd src/CorpAI.AppHost dotnet run`

The Aspire Dashboard will open automatically at `http://localhost:15888`.

* * *

⚙️ Configuration
----------------

### Environment Variables

| Variable | Description | Default
| `DOMAIN_NAME` | Public domain for the gateway | `localhost`
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development`
| `Keycloak__Authority` | Keycloak realm URL | `http://keycloak:8080/realms/corp-ai`
| `Keycloak__ClientId` | OIDC client ID | `dotnet-ai-client`
| `Keycloak__ClientSecret` | OIDC client secret | _(required)_

### Keycloak Configuration

The system auto-configures Keycloak on first run:

1.  **Realm**: `corp-ai`
2.  **Client**: `dotnet-ai-client` (for the gateway)
3.  **Client**: `open-webui` (for Open WebUI)
4.  **Admin User**: `admin` / `admin` (change on first login)
5.  **Passkeys**: Enabled by default

### Ollama Models

Pre-download models for offline use:

`# Via Admin Dashboard Navigate to: https://your-domain/admin/assets # Or via API curl -X POST https://your-domain/admin/ollama/api/pull \   -H "Authorization: Bearer YOUR_TOKEN" \  -d '{"name": "llama3"}'`

* * *

🔧 Usage
--------

### Accessing the Application

| Endpoint | Purpose |
| -------- | ------- |
| `/` | Main chat interface |
| `/webui` | Open WebUI (advanced features) |
| `/admin/assets` | Asset management dashboard |
| `/admin/keycloak` | Keycloak Admin Console |
| `/admin/ollama` | Ollama API proxy |
| `/api/chat` | Chat API endpoint |

### Authentication Flow

1.  Navigate to the application URL.
2.  Click **Login**.
3.  Enter credentials (or use Passkey).
4.  Access granted based on role.

**Default Credentials:**

*   Username: `admin`
*   Password: `admin`

### Managing Assets

All external dependencies are downloaded and served locally:

1.  Navigate to **Admin → Asset Manager**.
2.  Click **Download Bootstrap**, **Download jQuery**.
3.  Enter font family (e.g., `Roboto`) and weights (e.g., `400,700`).
4.  Click **Download Fonts**.

Assets are stored in `wwwroot/lib` and `wwwroot/fonts`.

* * *

🔒 Security
-----------

### Content Security Policy

The application enforces a strict CSP:

    default-src 'self';
    script-src 'self';
    style-src 'self';
    img-src 'self' data:;
    connect-src 'self';
    

**Exceptions:**

*   `/webui` path has relaxed CSP for Open WebUI SPA functionality.

### Authentication Methods

| Method | Use Case |
| ------ | -------- |
| **Password** | Traditional login |
| **Passkey (WebAuthn)** | Passwordless, biometric auth |
| **API Token** | Service-to-service auth |

### Rate Limiting

| Endpoint | Limit | Window |
| -------- | ----- | ------ |
| `/api/auth/login` | 5 requests | 15 minutes |
| `/api/chat/send` | 100 requests | 1 minute |
| `/admin/*` | 10 requests | 1 minute |

### Network Security

*   All services communicate via internal Docker network.
*   Only ports 80 and 443 are exposed externally.
*   Keycloak and Ollama are accessible only through YARP proxy.

* * *

🧪 Testing
----------

### Run Unit Tests

`dotnet test`

### Run with Coverage

`dotnet test --collect:"XPlat Code Coverage"`

### Test Categories

| Category | Coverage |
| -------- | -------- |
| **Services** | Theme generation, asset downloading |
| **Controllers** | Auth, Chat, Admin endpoints |
| **Middleware** | Dynamic CSS, CSP enforcement |
| **Integration** | Full auth flow, API proxying |

* * *

📁 Project Structure
--------------------

    CorpAI-Gateway/
    ├── src/
    │   ├── CorpAI.Gateway/           # Main MVC Application
    │   │   ├── Controllers/          # API Controllers
    │   │   ├── Services/             # Business Logic
    │   │   ├── Middleware/           # Custom Middleware
    │   │   ├── Views/                # Razor Views
    │   │   └── wwwroot/              # Static Assets
    │   │       ├── js/               # JavaScript (CSP-compliant)
    │   │       ├── css/              # Dynamic CSS
    │   │       ├── lib/              # Downloaded libraries
    │   │       └── fonts/            # Downloaded fonts
    │   ├── CorpAI.AppHost/           # Aspire Orchestrator
    │   └── CorpAI.KeycloakInitializer/ # Auto-configures Keycloak
    ├── tests/
    │   └── CorpAI.Gateway.Tests/     # Unit Tests
    ├── ansible/                      # Infrastructure as Code
    │   ├── playbooks/                # Deployment Playbooks
    │   └── group_vars/               # Configuration (encrypted)
    └── README.md
    

* * *

🔄 API Reference
----------------

### Authentication

`POST /api/auth/login Content-Type: application/json {   "username": "admin",  "password": "password" }`

**Response:**

`{   "success": true,  "message": "Logged in successfully",  "user": {    "username": "admin",    "email": "admin@corp.internal"  } }`

### Chat

`POST /api/chat/send Authorization: Bearer <token> Content-Type: application/json {   "prompt": "Explain quantum computing" }`

**Response:**

`[   { "role": "user", "content": "Explain quantum computing" },  { "role": "assistant", "content": "Quantum computing uses..." } ]`

### Admin - Asset Management

`POST /api/admin/download-fonts Authorization: Bearer <admin-token> Content-Type: application/json {   "fontFamily": "Roboto",  "weights": "400,700" }`

* * *

🐛 Troubleshooting
------------------

### Common Issues

#### 1\. Certificate Errors

**Problem:** Browser shows certificate warning.

**Solution:**

*   Ensure Certbot has run successfully: `docker logs certbot`
*   Check certificate files exist: `ls -la /opt/ai-system/certbot-data/live/`
*   Verify domain DNS resolves correctly.

#### 2\. Keycloak Login Loop

**Problem:** User keeps getting redirected to login page.

**Solution:**

*   Check cookie settings (SameSite must be `Lax` or `Strict`).
*   Verify Keycloak client redirect URIs include your domain.
*   Clear browser cookies and cache.

#### 3\. Ollama Model Not Found

**Problem:** `Error: model 'llama3' not found`

**Solution:**

`# Pull the model docker exec -it ollama ollama pull llama3`

#### 4\. CSP Violations

**Problem:** Console shows CSP errors.

**Solution:**

*   Ensure all scripts are in external `.js` files.
*   Verify no `onclick` or `style` attributes in HTML.
*   Check dynamic CSS is being served correctly.

* * *

📈 Performance Tuning
---------------------

### Ollama Optimization

For Mac Mini with 64GB RAM:

`# Set model to stay loaded in memory docker exec -it ollama ollama run llama3 --keepalive 24h # Use quantized models for better performance docker exec -it ollama ollama pull llama3:8b-q4_0`

### Database Optimization

`-- PostgreSQL tuning (in Keycloak) ALTER SYSTEM SET shared_buffers = '2GB'; ALTER SYSTEM SET effective_cache_size = '6GB'; ALTER SYSTEM SET max_connections = 200;`

* * *

🤝 Contributing
---------------

1.  Fork the repository.
2.  Create a feature branch: `git checkout -b feature/my-feature`
3.  Commit changes: `git commit -am 'Add new feature'`
4.  Push to branch: `git push origin feature/my-feature`
5.  Submit a Pull Request.

### Code Standards

*   Follow .NET coding conventions.
*   Maintain strict CSP compliance (no inline scripts/styles).
*   Write unit tests for new features.
*   Update documentation for API changes.

* * *

📄 License
----------

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

* * *

🆘 Support
----------

*   **Documentation**: [Wiki](https://github.com/your-org/CorpAI-Gateway/wiki)
*   **Issues**: [GitHub Issues](https://github.com/your-org/CorpAI-Gateway/issues)
*   **Internal Support**: `ai-support@yourcompany.com`

* * *

🗺️ Roadmap
-----------

*    **Multi-tenancy**: Support for multiple departments/teams
*    **Audit Logging**: Comprehensive activity tracking
*    **Model Fine-tuning**: Custom model training pipeline
*    **Mobile App**: Native iOS/Android client
*    **Voice Interface**: Speech-to-text integration
*    **Document Processing**: Advanced RAG with OCR

* * *

**Built with ❤️ by Your Organization**
