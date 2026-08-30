# sopa-talk

A multi-channel customer-service platform built on the official WhatsApp Cloud API —
shared team inbox, CRM, chatbot builder and an AI agent, in a single product. It is a
functional rebuild of [Umbler Talk](https://a.umbler.com/br/talk/), engineered to run
in production with paying customers rather than as a demo.

> **Status: early development.** The modular skeleton compiles and is covered by unit
> and architecture tests. Persistence (migrations, multi-tenancy), authentication and
> the WhatsApp integration are the next milestones — see [Roadmap](#roadmap).

---

## Why

Umbler Talk markets four "products" (Attendance Platform, CRM for WhatsApp, ChatBot
for WhatsApp, AI Agent for WhatsApp). They are four views of **one** application that
share the same core: contacts, conversations, messages, channels, users and tenants.
sopa-talk is built the same way — one product, four feature modules.

## Feature scope

Target: functional parity with Umbler Talk, delivered in releases that are each
sellable on their own.

| Area | What it covers | Status |
|---|---|---|
| **Platform** | Multi-agent shared inbox, teams & departments, routing queues, assignment & handoff, internal notes, tags, quick replies, webchat widget | Planned — R1 |
| **Channels** | Official WhatsApp Cloud API: number onboarding, idempotent webhook ingestion, outbound text/media/templates, 24-hour window, messaging tiers & rate limits | Planned — R1 |
| **CRM** | Visual pipelines / kanban, deals, stages, automatic lead routing, CSV contact import | Planned — R2 |
| **ChatBot** | Visual flow builder, execution engine (keyword / schedule / tag triggers), handoff to a human, message scheduling & reminders | Planned — R3 |
| **AI Agent** | Per-company knowledge base, LLM loop with tools/skills, configurable handoff criteria, guardrails | Planned — R4 |
| **Enterprise** | Public API & webhooks, audit logs, multi-unit / multi-brand, full reporting, plans & billing | Planned — R5 |
| Modular monolith skeleton, shared kernel, hosts, CI-ready build | — | **Done — R0 (in progress)** |

## Architecture

**Modular monolith.** One API process, one workers process, one PostgreSQL database
with a schema per module. Modules have hard boundaries and communicate only through
integration events, so any module can be extracted into its own service later without
a rewrite.

```
              ┌───────────────────────────── SopaTalk.Api (HTTP host) ─────────────────────────────┐
              │  composes modules · auth · OpenAPI · Hangfire dashboard · SignalR · serves the SPA │
              └───────┬───────────────┬───────────────┬───────────────┬───────────────┬────────────┘
                      │               │               │               │               │
                ┌─────▼────┐    ┌─────▼────┐    ┌─────▼────┐    ┌─────▼─────┐   ┌─────▼─────┐
                │ Channels │    │  Inbox   │    │   Crm    │    │  Chatbot  │   │  AiAgent  │
                └─────┬────┘    └─────┬────┘    └─────┬────┘    └─────┬─────┘   └─────┬─────┘
                      │        integration events (in-process bus → RabbitMQ later)  │
                      └───────────────┴───────────────┴───────────────┴──────────────┘
                                              │
                          ┌───────────────────▼────────────────────┐   ┌──────────────────────┐
                          │  PostgreSQL — one schema per module     │   │  SopaTalk.Workers    │
                          │  Hangfire job storage                   │   │  Hangfire server     │
                          └────────────────────────────────────────┘   └──────────────────────┘
```

Each module is four projects: `Domain` (entities, aggregates, domain events) →
`Application` (use-case slices, validators) → `Infrastructure` (EF Core, adapters,
job handlers) → `Api` (HTTP endpoints + the module installer).

Patterns: Modular Monolith · Vertical Slice Architecture · Clean/Hexagonal dependency
rule · tactical DDD where the domain is rich · lightweight CQRS · integration events ·
Result pattern. Rationale is recorded in [`docs/adr/`](docs/adr/).

**Non-negotiable rules** (enforced by architecture tests):

- A module never touches another module's DbContext, tables or internal types.
- `Domain` never references EF Core, HTTP or external SDKs.
- Every persistent entity carries `TenantId`; EF Core global filter **and** Postgres
  Row-Level Security both apply.
- Meta webhooks are processed idempotently.

Full write-up: [`docs/arquitetura.md`](docs/arquitetura.md).

## Tech stack

| Layer | Choice |
|---|---|
| Backend | ASP.NET Core (.NET 10), C# |
| Persistence | PostgreSQL + EF Core (Npgsql), schema per module |
| Background jobs | Hangfire (PostgreSQL storage) |
| Realtime | SignalR (Redis/Valkey backplane once running >1 instance) |
| Frontend | React 19 + Vite (SPA), served as static files by the ASP.NET host |
| Tests | xUnit, FluentAssertions, Testcontainers, NetArchTest |
| Local infra | Docker Compose — PostgreSQL + Valkey |

Package versions are pinned centrally in `Directory.Packages.props`. Commercial
libraries (MediatR, AutoMapper) are avoided; Hangfire runs on its free
PostgreSQL storage.

## Repository layout

```
src/
  SopaTalk.Api/            HTTP host — composes modules, auth, OpenAPI, /jobs dashboard, SPA fallback
  SopaTalk.Workers/        background-job host (Hangfire server)
  SopaTalk.SharedKernel/   domain primitives, Result, event-bus contracts, multi-tenancy, IModuleInstaller
  SopaTalk.SharedKernel.Persistence/  EF Core building blocks — TenantDbContext, tenant interceptor
  Modules/
    Accounts/              companies, users, sign-in, JWT, multi-tenancy
    Channels/              WhatsApp Cloud API
    Inbox/                 multi-agent attendance
    Crm/                   pipelines / deals
    Chatbot/               flow builder + engine
    AiAgent/               knowledge base, skills, handoff
    (each module: Domain / Application / Infrastructure / Api)
frontend/                  React + Vite SPA
infra/                     docker-compose (PostgreSQL + Valkey)
tests/
  SopaTalk.Inbox.UnitTests/       domain unit tests
  SopaTalk.ArchitectureTests/     module-boundary enforcement
docs/                      architecture, roadmap, cost models, ADRs
```

## Getting started

Two supported setups — full details, per-OS install steps and troubleshooting in
[`docs/ambiente.md`](docs/ambiente.md).

### Option A — Dev Container (recommended, VS Code)

Identical toolchain on Windows, macOS and Linux.

1. Install VS Code + the **Dev Containers** extension, and have Docker running.
2. Open the repo → Command Palette → **Dev Containers: Reopen in Container**.
3. First build restores everything and starts PostgreSQL + Valkey. Then run
   **Run and Debug → "API + Workers"**, and `npm --prefix frontend run dev`.

See [`.devcontainer/README.md`](.devcontainer/README.md).

### Option B — Native

**Prerequisites** (versions are pinned — see `global.json`, `.nvmrc`,
`.config/dotnet-tools.json`):

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (feature band ≥ 111)
- [Node.js](https://nodejs.org) 22 LTS (`nvm install` reads `.nvmrc`)
- Docker (Engine + Compose) — also required by the Testcontainers integration tests

**Bootstrap:**

```bash
dotnet tool restore                          # dotnet-ef at the team's version
dotnet restore SopaTalk.slnx
npm --prefix frontend ci

docker compose -f infra/docker-compose.yml up -d   # PostgreSQL :5432, Valkey :6379
dotnet build SopaTalk.slnx
dotnet test  SopaTalk.slnx
```

The default connection strings in `appsettings.json` already point at the Compose
services (database / user / password all `sopatalk`).

### Run

```bash
dotnet run --project src/SopaTalk.Api        # http://localhost:5080
dotnet run --project src/SopaTalk.Workers    # background-job processor
cd frontend && npm run dev                   # http://localhost:5173
```

- API docs (Scalar): `http://localhost:5080/scalar/v1`
- Job dashboard: `http://localhost:5080/jobs` (Development only)
- Health check: `http://localhost:5080/health`
- Module smoke endpoints: `GET /api/<module>/_ping`

Try the auth flow:

```bash
curl -sX POST localhost:5080/api/accounts/register -H 'content-type: application/json' \
  -d '{"companyName":"Acme","slug":"acme","ownerName":"Ana","email":"ana@acme.com","password":"supersecret"}'
# -> { accessToken, expiresAtUtc, user }

curl -s localhost:5080/api/accounts/me -H "authorization: Bearer <accessToken>"
```

In Development, migrations for every module run automatically on startup.

The Vite dev server proxies `/api`, `/jobs` and `/health` to the ASP.NET host, so the
SPA and API share an origin in development — the same single-origin setup used in
production, where `npm run build` emits straight into the API's `wwwroot`.

Contributing workflow and the rules CI enforces: [`CONTRIBUTING.md`](CONTRIBUTING.md).

### Current state

- **Auth works.** Register a company, sign in, call authenticated endpoints. Password
  hashing (PBKDF2), JWT issuance/validation and tenant isolation (EF Core global
  filter + write interceptor) are in place. See [`docs/seguranca.md`](docs/seguranca.md).
- **Still missing in R0:** refresh tokens, 2FA, login lockout, Postgres Row-Level
  Security, a CQRS dispatcher, the transactional outbox, CI.
- **No WhatsApp integration yet.** The `Channels` module is a skeleton (R1).
- **Frontend is a shell.** Sidebar + placeholder pages; no login screen yet.

## Roadmap

| Release | Theme | Highlights |
|---|---|---|
| **R0** | Foundation *(in progress)* | CQRS dispatcher, migrations + `TenantId` + RLS, authentication + 2FA, transactional outbox, structured logging + Sentry, CI pipeline |
| **R1** | Attendance Platform *(first sellable release)* | WhatsApp Cloud API, multi-agent inbox, teams & routing, realtime, webchat widget, essential reports, opt-in tracking |
| **R2** | CRM | Kanban pipelines, deals, lead routing, CSV import |
| **R3** | ChatBot | Visual flow builder, execution engine, scheduling & reminders |
| **R4** | AI Agent | Knowledge base, LLM tool-use loop, configurable handoff, guardrails |
| **R5** | Enterprise | Public API & webhooks, audit logs, multi-unit, full reporting, billing |

Detail and task lists: [`docs/roadmap.md`](docs/roadmap.md).

## Documentation

| Document | Contents |
|---|---|
| [`docs/arquitetura.md`](docs/arquitetura.md) | Architecture, modules, layering, boundary rules |
| [`docs/roadmap.md`](docs/roadmap.md) | Releases R0–R5 with checklists |
| [`docs/ambiente.md`](docs/ambiente.md) | Dev environment — Dev Container and per-OS native setup |
| [`docs/seguranca.md`](docs/seguranca.md) | Auth model, password hashing, JWT, multi-tenancy — what's done and what's not |
| [`docs/custos.md`](docs/custos.md) | Infrastructure & SaaS cost model |
| [`docs/pagamentos.md`](docs/pagamentos.md) | Billing — PSP choice, Pix/card/installments, when to build the `Billing` module |
| [`docs/custos-whatsapp.md`](docs/custos-whatsapp.md) | WhatsApp / Meta billing — what is paid, what is free, the 24-hour window, AI-agent token billing |
| [`docs/whatsapp-teste.md`](docs/whatsapp-teste.md) | How to get a test WhatsApp number without risking a personal one |
| [`docs/adr/`](docs/adr/) | Architecture decision records (modular monolith, stack, background jobs, multi-tenancy) |

> Documentation prose is written in Portuguese; code identifiers are in English.

## License

Private and proprietary. All rights reserved.
