# ADR 0002 — Stack: ASP.NET Core + React/Vite + PostgreSQL

- Status: aceito
- Data: 2026-08-29

## Contexto

Dois objetivos: (1) produto sério, manutenível e escalável; (2) portfólio que
demonstre competência no mercado B2B brasileiro (onde vagas de produto pedem
C#/.NET, Entity Framework, SQL Server, RabbitMQ, Blazor).

## Decisão

- **Backend:** ASP.NET Core (.NET 10), C#.
- **Persistência:** PostgreSQL + EF Core (Npgsql).
- **Frontend:** React 19 + Vite como SPA, servido como estático pelo host ASP.NET.
  Sem servidor Node em produção.
- **Realtime:** SignalR.

## Alternativas consideradas

- **100% TypeScript (Next.js full-stack):** iteração solo mais rápida, mas sinal
  "enterprise" mais fraco, tipagem sem garantia em runtime, e jobs exigiriam
  BullMQ+Redis. Rejeitado dado o objetivo de portfólio.
- **100% .NET com Blazor:** um só idioma, deploy simples. Rejeitado porque React é
  muito mais marketável que Blazor e tem ecossistema de componentes maior; Blazor
  WASM ainda tem custo de carga inicial.
- **Next.js em vez de Vite para o front:** SSR não agrega em app atrás de login
  (sem SEO no inbox) e traria de volta um runtime Node em produção. Vite SPA
  mantém o deploy 100% .NET.
- **SQL Server:** sem vantagem sobre Postgres para este caso e com custo de licença.
  Postgres é padrão de mercado e mais rico.

## Consequências

- Dois ecossistemas de pacote (NuGet + npm) — fricção aceita, mitigada por o front
  ser só cliente estático (um deploy).
- Tipagem forte ponta a ponta (C# + TS).
- Demonstra exatamente as duas competências pedidas: backend C#/.NET e frontend React.
