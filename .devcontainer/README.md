# Dev Container

The recommended way to work on ozap-talk in VS Code — identical toolchain on
Windows, macOS and Linux.

## Requirements

- VS Code + the **Dev Containers** extension (`ms-vscode-remote.remote-containers`)
- Docker running on the host:
  - **Linux:** Docker Engine (`sudo apt install docker.io docker-compose-v2`)
  - **macOS / Windows:** Docker Desktop, or a free alternative — Rancher Desktop or
    Podman Desktop (both work with this setup and with Testcontainers)

## Use

1. Open the repo in VS Code.
2. Command Palette → **Dev Containers: Reopen in Container**.
3. First build takes a few minutes. When it finishes:
   - `dotnet tool restore`, `dotnet restore` and `npm ci` have already run
   - Postgres + Valkey are up (via `infra/docker-compose.yml` on the host Docker)
4. Run the backend: **Run and Debug → "API + Workers"** (or `dotnet run --project src/OzapTalk.Api`).
5. Run the frontend: `npm --prefix frontend run dev`.

Ports 5080 (API) and 5173 (frontend) are forwarded automatically.

## Notes

- The container reaches the database at `host.docker.internal`; running natively
  (outside the container) it is `localhost`. Both are already configured — you do
  not set connection strings by hand.
- If the base image tag fails to pull, bump `image` in `devcontainer.json`.
- Prefer not to use a container? Follow the native setup in `docs/ambiente.md`.
