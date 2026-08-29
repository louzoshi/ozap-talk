# Arquitetura — sopa-talk

## Visão geral

sopa-talk é **um produto**: uma plataforma de atendimento via WhatsApp. As quatro
"ofertas" do Umbler Talk (Plataforma de Atendimento, CRM, ChatBot, Agente IA) são
recortes de marketing da mesma aplicação, não produtos separados. Todos compartilham
o mesmo núcleo: contatos, conversas, mensagens, canais, usuários e tenants.

## Estilo: monólito modular

Um único processo de API + um processo de workers. Deploy único. Internamente
dividido em módulos com fronteiras fortes, cada um pronto para virar serviço
independente se um dia bater um limite real de escala.

```
┌─────────────────────────────────────────────────────────────┐
│  SopaTalk.Api  (host HTTP)                                   │
│  - compõe os módulos via IModuleInstaller                    │
│  - auth, health, OpenAPI, dashboard Hangfire, SPA fallback   │
└───────────────┬─────────────────────────────────────────────┘
                │ referencia apenas *.Api de cada módulo
   ┌────────────┼────────────┬────────────┬────────────┐
┌──▼───┐   ┌────▼───┐   ┌────▼───┐   ┌────▼────┐   ┌───▼─────┐
│Channels│ │ Inbox  │   │  Crm   │   │ Chatbot │   │ AiAgent │
└──┬───┘   └────┬───┘   └────┬───┘   └────┬────┘   └───┬─────┘
   │ integration events (in-process bus, troca por RabbitMQ depois)
   └────────────┴────────────┴────────────┴────────────┘
                        │
              ┌─────────▼──────────┐   ┌──────────────────┐
              │ PostgreSQL          │   │ SopaTalk.Workers │
              │ 1 banco, 1 schema   │   │ Hangfire server  │
              │ por módulo          │   │ (mesmos módulos) │
              └────────────────────┘   └──────────────────┘
```

## Módulos

| Módulo | Responsabilidade | Agregados previstos |
|---|---|---|
| **Channels** | Integração WhatsApp Cloud API: conexão de número, ingestão de webhook (idempotente), envio de mensagem/mídia/template, controle da janela de 24h, tiers de messaging | `WhatsAppChannel`, `InboundMessage`, `OutboundMessage`, `MessageTemplate` |
| **Inbox** | Multiatendimento: conversas, atribuição a atendente/setor, filas de atendimento, notas internas, tags, respostas rápidas, transferência/handoff | `Conversation`, `Agent`, `Team`, `QuickReply` |
| **Crm** | Pipelines/kanban de negociação ligados aos contatos | `Pipeline`, `Deal`, `Stage`, `Contact` (view local) |
| **Chatbot** | Construtor visual de fluxo + motor de execução que roda antes/no lugar do atendente | `Flow`, `FlowVersion`, `FlowRun` |
| **AiAgent** | Camada de LLM sobre o Chatbot: base de conhecimento por empresa, skills/ações, critérios de handoff | `Agent`, `KnowledgeBase`, `KnowledgeDocument`, `AgentRun` |

Fora dos módulos, no `SharedKernel`: primitivos de domínio (`Entity`, `AggregateRoot`,
`IDomainEvent`), `Result`, contratos do event bus, `ITenantContext` e `IModuleInstaller`.

## Camadas dentro de um módulo

```
SopaTalk.<Módulo>.Domain          entidades, agregados, value objects, domain events
        └─ referencia: SharedKernel

SopaTalk.<Módulo>.Application     casos de uso (vertical slices): Command/Query + Handler + Validator
        └─ referencia: Domain, SharedKernel
        └─ pasta Contracts/ = integration events (API pública do módulo, versionada)

SopaTalk.<Módulo>.Infrastructure  DbContext (1 schema), repositórios, adapters externos, handlers de job
        └─ referencia: Application, Domain, SharedKernel

SopaTalk.<Módulo>.Api             endpoints HTTP (minimal API) + o IModuleInstaller do módulo
        └─ referencia: Application, Infrastructure, SharedKernel
```

Regra de dependência (Clean Architecture): **o domínio não depende de nada**.
EF Core, HttpClient, SDK da Meta, Hangfire — tudo isso só aparece em `Infrastructure`.

## Padrões adotados

- **Modular Monolith** — forma geral. Referência: `kgrzybek/modular-monolith-with-ddd`.
- **Vertical Slice Architecture** — organização interna de `Application`. Um arquivo por
  caso de uso, sem camadas cerimoniais.
- **Clean / Hexagonal** — apenas a regra de direção de dependência.
- **DDD tático** — só onde a regra é rica: `Inbox`, `Crm`, `Chatbot`. `Channels` e
  `AiAgent` são mais orquestração/integração.
- **CQRS leve** — comandos e queries separados, despachados por um mediador simples
  próprio (evita a licença comercial do MediatR).
- **Integration Events** — única forma de um módulo falar com outro.
- **Result pattern** — desfecho de negócio esperado não vira exceção.

## Comunicação entre módulos

Proibido: `CrmDbContext` dentro do `Inbox`; `using SopaTalk.Crm.Domain` fora do Crm.

Permitido: `Inbox` publica `ConversationClosed` no `IEventBus`; `Crm` tem um
`IIntegrationEventHandler<ConversationClosed>` que reage. Hoje o bus é in-process e
síncrono dentro da transação; a troca por RabbitMQ (via outbox) não muda publishers
nem handlers.

Os tipos de integration event ficam em `SopaTalk.<Módulo>.Application/Contracts/` e
são tratados como API pública — mudança quebra-compatibilidade exige versionar.

## Multi-tenancy

Ver `docs/adr/0004-multitenancy.md`. Resumo: `TenantId` em toda entidade, filtro
global no EF Core, RLS no Postgres como segunda barreira, tenant resolvido da claim
`tenant_id` do token (API) ou dos argumentos do job (workers).

## Realtime

SignalR no host. Um hub por área que precisa de push (ex.: `InboxHub`). Enquanto
roda 1 instância, sem backplane. Com 2+, `Microsoft.AspNetCore.SignalR.StackExchangeRedis`
apontando para o Valkey/Redis.

## Background jobs

Hangfire com storage no Postgres (`Hangfire.PostgreSql`, MIT). O processo
`SopaTalk.Workers` roda o Hangfire server; a API só enfileira e expõe o dashboard
em `/jobs`. Casos: processar webhook, enviar template, lembrete agendado, disparo em
massa (com respeito ao rate limit da Meta), reprocessamento com retry/backoff.

## O que ainda NÃO está no esqueleto (decisões tomadas, implementação pendente)

- Mediador CQRS próprio + pipeline de validação
- Outbox pattern para os integration events
- `TenantId` nos `DbContext` + RLS + migrations
- Autenticação real (ASP.NET Identity ou IdP) + 2FA
- SignalR hubs
- Adapter da WhatsApp Cloud API + verificação de assinatura de webhook
- OpenTelemetry (traces/métricas) — ver `docs/observabilidade.md` quando existir
- Pipeline CI/CD + ambiente de staging
