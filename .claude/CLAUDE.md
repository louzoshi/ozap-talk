# sopa-talk

Plataforma de atendimento multicanal via WhatsApp — clone funcional do Umbler Talk
(<https://a.umbler.com/br/talk/>). Produto real, feito para receber clientes pagantes.

## Atribuição — REGRA ABSOLUTA

Nunca cite, mencione ou referencie Claude, Anthropic, "AI", "IA", "gerado por IA" ou
qualquer assistente em **nada** deste repositório. Isso vale para, sem exceção:

- mensagens de commit (sem `Co-Authored-By`, sem trailer de sessão, sem "Generated with")
- títulos e corpo de Pull Requests
- comentários de código, docstrings, TODOs
- CHANGELOG, release notes, documentação
- nomes de branch

O autor de todo o trabalho é o dono do repositório. Escreva como ele escreveria.
`includeCoAuthoredBy` já está desligado em `.claude/settings.json`; mantenha assim.

## Stack

- **Backend:** ASP.NET Core (.NET 10), C#
- **Persistência:** PostgreSQL + EF Core (Npgsql), um schema por módulo
- **Jobs em background:** Hangfire (storage no Postgres). RabbitMQ só se/quando escalar
- **Realtime:** SignalR (backplane Redis apenas com mais de 1 instância)
- **Frontend:** React 19 + Vite (SPA), servido como estático pelo host ASP.NET
- **Testes:** xUnit, FluentAssertions, Testcontainers (Postgres real), NetArchTest

## Arquitetura

Monólito modular. Um deploy, fronteiras internas fortes. Ver `docs/arquitetura.md`
e os ADRs em `docs/adr/`.

Módulos: `Channels` (WhatsApp Cloud API), `Inbox` (multiatendimento), `Crm`,
`Chatbot`, `AiAgent`. Cada módulo tem 4 projetos: `Domain`, `Application`,
`Infrastructure`, `Api`.

### Regras que não se quebram

1. Um módulo **nunca** acessa o DbContext, as tabelas ou os tipos internos de outro.
   Comunicação entre módulos só por **integration events** (`SopaTalk.SharedKernel.Messaging`).
2. `Domain` não referencia EF Core, HTTP, SDKs externos. Isso vive em `Infrastructure`.
3. Toda entidade persistente carrega `TenantId`. Filtro global no EF Core **e**
   Row-Level Security no Postgres. Nunca confie só em um.
4. Webhooks da Meta são processados de forma **idempotente** (chave de deduplicação).
5. Cada módulo se registra sozinho via seu `IModuleInstaller`. O host (`SopaTalk.Api`)
   só compõe — não conhece o interior de nenhum módulo.
6. Handlers retornam `Result` para desfechos de negócio esperados; exceção só para o
   excepcional.

## Convenções de código

- Nullable habilitado, warnings como erro (exceto auditoria NuGet).
- Versões de pacote **só** em `Directory.Packages.props` (central package management).
- Um arquivo por slice de caso de uso em `*.Application`.
- Um arquivo de endpoints por feature em `*.Api`.
- Nomes, comentários e documentação em português; identificadores de código em inglês.

## Ambiente

VS Code + Dev Container (`.devcontainer/`) é o caminho recomendado. Setup nativo e
por SO em `docs/ambiente.md`. Versões fixadas: `global.json` (.NET), `.nvmrc` (Node 22),
`.config/dotnet-tools.json` (dotnet-ef).

## Comandos

```bash
dotnet tool restore                                # dotnet-ef na versão do time
dotnet restore SopaTalk.slnx
npm --prefix frontend ci

docker compose -f infra/docker-compose.yml up -d   # Postgres + Valkey locais
dotnet build SopaTalk.slnx
dotnet test SopaTalk.slnx
dotnet format SopaTalk.slnx                         # formatar C# antes de commitar

dotnet run --project src/SopaTalk.Api              # http://localhost:5080  (jobs em /jobs)
dotnet run --project src/SopaTalk.Workers          # processador de jobs
npm --prefix frontend run dev                      # SPA em http://localhost:5173
npm --prefix frontend run lint                     # eslint
```
