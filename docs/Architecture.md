┌─────────────────────────────────────────────────────────────────┐
│                        Mac Mini Host                            │
│                                                                 |
│                                                                 |
│   ┌──────────────┐     ┌─────────────────────────────────────┐  │
│   │   Certbot    │────▶│  Shared Volume (/etc/letsencrypt)   │  │
│   │  (DNS-01)    │     │  - fullchain.pem                    │  │
│   └──────────────┘     │  - privkey.pem                      │  │
│                        └──────────────┬──────────────────────┘  │
│                                       │                         │
│                        ┌──────────────▼──────────────────────┐  │
│                        │                                     │  │
│                        │   Kestrel / YARP (.NET 10)          │  │
│                        │   Port 443 (HTTPS)                  │  │
│                        │   Port 80 (Redirect only)           │  │
│                        │                                     │  │
│                        └──────────────┬──────────────────────┘  │
│                                       │                         │
│         ┌─────────────────────────────┼─────────────────────┐   │
│         │                             │                     │   │
│         ▼                             ▼                     ▼   │
│   ┌──────────┐                 ┌──────────┐          ┌────────┐ │
│   │ Keycloak │                 │  Ollama  │          │ Chroma │ │
│   │  + PG    │                 │          │          │   DB   │ │
│   └──────────┘                 └──────────┘          └────────┘ │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

CorpAI-Gateway/
├── .gitignore
├── README.md
├── ansible/
│   ├── ansible.cfg
│   ├── inventory.ini
│   ├── group_vars/
│   │   └── all.yml
│   ├── playbooks/
│   │   ├── site.yml
│   │   └── tasks/
│   │       ├── deploy_app.yml
│   │       └── certbot_setup.yml
│   └── templates/
│       └── docker-compose.yml.j2
├── src/
│   ├── AiGateway/
│   │   ├── AiGateway.csproj
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Dockerfile
│   │   ├── Services/
│   │   │   ├── DynamicCertificateLoader.cs
│   │   │   └── CertificateReadyStartupFilter.cs
│   │   ├── Pages/
│   │   │   └── _Host.cshtml
│   │   └── Components/
│   │       └── App.razor
│   └── AiGateway.Tests/ (Optional)
└── scripts/
    └── init-secrets.sh