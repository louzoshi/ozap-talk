# Ambiente de desenvolvimento

Duas formas de rodar o projeto. Escolha uma.

| | Dev Container (recomendado) | Nativo |
|---|---|---|
| Setup | 1 clique no VS Code | instalar SDK/Node/Docker por SO |
| Igual em Win/Mac/Linux | sim, garantido | quase — versões podem divergir |
| Inner loop | bom | mais rápido |
| Requisito | VS Code + Docker | .NET 10 SDK + Node 22 + Docker |

---

## Opção A — Dev Container (VS Code)

Ver [`.devcontainer/README.md`](../.devcontainer/README.md). Resumo:

1. VS Code + extensão **Dev Containers**.
2. Docker rodando (Docker Desktop no Win/Mac; Docker Engine no Linux; ou Rancher/Podman Desktop).
3. Abrir o repo → **Dev Containers: Reopen in Container**.
4. Pronto: tools restauradas, Postgres + Valkey no ar. Rodar via **Run and Debug → "API + Workers"**.

---

## Opção B — Nativo

### Versões

| Ferramenta | Versão | Fixada em |
|---|---|---|
| .NET SDK | 10.0.x (feature band ≥ 111) | `global.json` |
| Node.js | 22 LTS | `.nvmrc` / `frontend/package.json` engines |
| dotnet-ef | 10.0.11 | `.config/dotnet-tools.json` |

### Linux (Ubuntu/Debian)

```bash
# .NET 10 SDK — https://learn.microsoft.com/dotnet/core/install/linux
sudo apt update && sudo apt install -y dotnet-sdk-10.0

# Node 22 via nvm (respeita o .nvmrc)
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.1/install.sh | bash
nvm install    # lê o .nvmrc

# Docker
sudo apt install -y docker.io docker-compose-v2
sudo systemctl enable --now docker
sudo usermod -aG docker $USER   # relogar depois
```

### macOS

```bash
brew install --cask dotnet-sdk          # ou o instalador da Microsoft
brew install nvm && nvm install         # lê o .nvmrc
brew install --cask docker              # ou rancher-desktop / podman-desktop
```

### Windows

```powershell
winget install Microsoft.DotNet.SDK.10
winget install CoreyButler.NVMforWindows   # depois: nvm install 22 ; nvm use 22
winget install Docker.DockerDesktop        # ou Rancher Desktop / Podman Desktop
```

> No Windows, use o repositório sempre com fim de linha LF — o `.gitattributes` já
> cuida disso, não mexa em `core.autocrlf`.

### Bootstrap (qualquer SO, depois das ferramentas instaladas)

```bash
git clone https://github.com/louzoshi/sopa-talk.git
cd sopa-talk

dotnet tool restore                 # dotnet-ef na versão do time
dotnet restore SopaTalk.slnx
npm --prefix frontend ci

docker compose -f infra/docker-compose.yml up -d
dotnet build SopaTalk.slnx
dotnet test  SopaTalk.slnx
```

### Rodar

```bash
dotnet run --project src/SopaTalk.Api        # http://localhost:5080
dotnet run --project src/SopaTalk.Workers    # jobs
cd frontend && npm run dev                    # http://localhost:5173
```

- Scalar (API docs): <http://localhost:5080/scalar/v1>
- Hangfire: <http://localhost:5080/jobs> (só em Development)
- Health: <http://localhost:5080/health>

Em Development, as migrations de todos os módulos rodam sozinhas no start. A chave
JWT de dev já vem em `appsettings.Development.json`. Para sobrescrever sem tocar no
arquivo: `dotnet user-secrets set "Jwt:SigningKey" "<valor>" --project src/SopaTalk.Api`.
Em produção, `Jwt__SigningKey` vem de variável de ambiente / secret manager.

### HTTPS local (opcional)

Se usar o profile `https` do `dotnet run`:

```bash
dotnet dev-certs https --trust
```

---

## Problemas comuns

| Sintoma | Causa | Correção |
|---|---|---|
| `permission denied ... docker.sock` | usuário fora do grupo `docker` | relogar após `usermod -aG docker $USER` |
| API não sobe: "Failed to connect to ...:5432" | Postgres não está no ar | `docker compose -f infra/docker-compose.yml up -d` |
| `dotnet-ef: command not found` | tool não restaurada | `dotnet tool restore` e prefixe com `dotnet ef ...` |
| Front não conecta na API | porta errada | API é **5080**; o proxy do Vite já aponta pra lá |
| Testcontainers falha | Docker não acessível | subir Docker; no Dev Container, feature `docker-outside-of-docker` |
| Diffs enormes de fim de linha | `.gitattributes` não aplicado | `git add --renormalize . && git commit` |
