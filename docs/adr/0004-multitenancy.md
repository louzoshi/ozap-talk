# ADR 0004 — Multi-tenancy: banco compartilhado, schema por módulo, RLS

- Status: aceito
- Data: 2026-08-29

## Contexto

sopa-talk é SaaS B2B: cada cliente (empresa) é um tenant, e o sistema processa dado
pessoal dos clientes **dos** clientes (LGPD). Vazamento de dados entre tenants é o
pior defeito possível e não é corrigível "depois" — a decisão precisa estar certa
desde a primeira migration.

## Decisão

**Um banco PostgreSQL, um schema por módulo, isolamento lógico por `TenantId`**, com
duas barreiras independentes:

1. **Aplicação:** toda entidade persistente herda `TenantId`. Cada `DbContext` aplica
   um *global query filter* `e => e.TenantId == _tenantContext.TenantId`. Em escrita,
   o `TenantId` é carimbado no `SaveChanges` a partir do `ITenantContext`, nunca vindo
   do cliente.
2. **Banco:** Row-Level Security (RLS) ativado nas tabelas com tenant; a policy usa
   uma *session variable* (`app.tenant_id`) que a infraestrutura seta por conexão.
   Se um filtro da aplicação falhar, o Postgres ainda bloqueia.

`ITenantContext` resolve o tenant:
- **API:** da claim `tenant_id` do token autenticado.
- **Workers:** do argumento do job (todo job carrega `tenantId` explícito).

## Alternativas consideradas

- **Um banco por tenant:** isolamento máximo, mas custo e complexidade operacional
  (migrations ×N, provisionamento, conexões) altos demais para o estágio. Reavaliar
  se um cliente enterprise exigir isolamento físico — a fronteira de módulo/schema
  facilita mover um tenant para banco próprio depois.
- **Schema por tenant:** explode o número de schemas e migrations; EF Core lida mal.
- **Só filtro de aplicação, sem RLS:** uma única barreira; um `IgnoreQueryFilters()`
  esquecido vira incidente. Rejeitado.

## Consequências

- Todo agregado novo **precisa** de `TenantId` e de índice que o inclua.
- Queries de relatório que cruzam tenants (uso interno da plataforma) rodam com um
  contexto administrativo explícito, auditado.
- Testes de arquitetura verificam que nenhuma entidade persistente escapa da regra.
- Connection pooling precisa setar/limpar `app.tenant_id` corretamente por request
  (interceptor no `DbContext`).
