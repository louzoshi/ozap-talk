# Contributing

## Setup

See [`docs/ambiente.md`](docs/ambiente.md). Fastest path in VS Code:
**Dev Containers: Reopen in Container**.

## Day-to-day

```bash
dotnet build OzapTalk.slnx
dotnet test  OzapTalk.slnx
dotnet format OzapTalk.slnx            # format C# before committing
npm --prefix frontend run lint
npm --prefix frontend run format
```

## Branches & commits

- Branch off `main`: `feat/<short-desc>`, `fix/<short-desc>`, `chore/<short-desc>`.
- Conventional Commits in **English**: `feat(inbox): ...`, `fix(channels): ...`,
  `docs: ...`, `test: ...`, `chore: ...`.
- Open a PR into `main`. Keep PRs small and focused on one release item from
  [`docs/roadmap.md`](docs/roadmap.md).

## Rules CI enforces

Every push and pull request runs [`.github/workflows/ci.yml`](.github/workflows/ci.yml).
A red build blocks the merge.

1. `dotnet build` is warning-free (`TreatWarningsAsErrors`).
2. `dotnet test` passes — including the architecture tests in
   `tests/OzapTalk.ArchitectureTests`, which cover rules 5 and 6 below.
3. `dotnet format --verify-no-changes` — C# matches `.editorconfig`.
4. The SPA lints, type-checks and builds (`npm run lint`, `npm run build`).

## Rules reviewers enforce

Not yet automated — catch these in review.

5. A module never references another module's `Domain`/`Infrastructure`. Cross-module
   communication goes through integration events in `OzapTalk.SharedKernel.Messaging`.
6. `Domain` projects take no dependency on EF Core, ASP.NET Core or external SDKs.
7. Every persistent entity carries `TenantId`.
8. Package versions are added to `Directory.Packages.props`, never to a `.csproj`.
9. New tools go in `.config/dotnet-tools.json` (`dotnet tool install <name>`), not
   as global installs.

## Attribution

No AI-assistant credit anywhere in this repository — commits, PRs, comments, docs,
branch names. See [`.claude/CLAUDE.md`](.claude/CLAUDE.md).

## Architecture decisions

Anything that changes structure, a dependency, or a boundary gets an ADR in
[`docs/adr/`](docs/adr/) (copy the format of an existing one).
