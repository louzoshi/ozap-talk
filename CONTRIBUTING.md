# Contributing

## Setup

See [`docs/ambiente.md`](docs/ambiente.md). Fastest path in VS Code:
**Dev Containers: Reopen in Container**.

## Day-to-day

```bash
dotnet build SopaTalk.slnx
dotnet test  SopaTalk.slnx
dotnet format SopaTalk.slnx            # format C# before committing
npm --prefix frontend run lint
npm --prefix frontend run format
```

## Branches & commits

- Branch off `main`: `feat/<short-desc>`, `fix/<short-desc>`, `chore/<short-desc>`.
- Conventional Commits in **English**: `feat(inbox): ...`, `fix(channels): ...`,
  `docs: ...`, `test: ...`, `chore: ...`.
- Open a PR into `main`. Keep PRs small and focused on one release item from
  [`docs/roadmap.md`](docs/roadmap.md).

## Rules the CI (and reviewers) enforce

1. `dotnet build` is warning-free (`TreatWarningsAsErrors`).
2. `dotnet test` passes — including the architecture tests in
   `tests/SopaTalk.ArchitectureTests`.
3. A module never references another module's `Domain`/`Infrastructure`. Cross-module
   communication goes through integration events in `SopaTalk.SharedKernel.Messaging`.
4. `Domain` projects take no dependency on EF Core, ASP.NET Core or external SDKs.
5. Every persistent entity carries `TenantId`.
6. Package versions are added to `Directory.Packages.props`, never to a `.csproj`.
7. New tools go in `.config/dotnet-tools.json` (`dotnet tool install <name>`), not
   as global installs.

## Attribution

No AI-assistant credit anywhere in this repository — commits, PRs, comments, docs,
branch names. See [`.claude/CLAUDE.md`](.claude/CLAUDE.md).

## Architecture decisions

Anything that changes structure, a dependency, or a boundary gets an ADR in
[`docs/adr/`](docs/adr/) (copy the format of an existing one).
