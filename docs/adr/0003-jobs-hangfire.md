# ADR 0003 — Background jobs com Hangfire (storage Postgres)

- Status: aceito
- Data: 2026-08-29

## Contexto

A plataforma precisa processar trabalho fora do request HTTP: ingestão de webhooks
da Meta em rajada, envio de templates com rate limit, retries com backoff, lembretes
agendados, disparo em massa, ingestão de base de conhecimento. Time de uma pessoa
que vai operar o sistema sozinha (inclusive debugar envio falho de madrugada).

## Decisão

**Hangfire** com storage no **PostgreSQL** (`Hangfire.PostgreSql`, MIT). O processo
`OzapTalk.Workers` roda o Hangfire server; a `OzapTalk.Api` apenas enfileira e expõe
o dashboard em `/jobs` (a ser restrito a admins da plataforma).

Redis/Valkey **não** entra agora. Entra quando:
1. a API rodar em 2+ instâncias (backplane do SignalR), ou
2. o volume de jobs exigir filas dedicadas (aí avaliar Hangfire.Pro ou RabbitMQ).

## Alternativas consideradas

- **RabbitMQ + MassTransit:** mais "correto" para arquitetura orientada a eventos e
  escala maior, mas três peças de infra a mais e carga operacional alta para um dev
  solo. MassTransit v9 vai virar pago (v8 é Apache-2.0). Adiado — dá para adicionar
  RabbitMQ para fluxos event-driven específicos depois, sem remover o Hangfire.
- **BullMQ + Redis:** é biblioteca Node; exigiria um segundo runtime só para filas.
  Rejeitado (backend é .NET).
- **Quartz.NET:** só agendamento, sem infra de job/retry/dashboard pronta.
- **Hangfire.Pro desde já (US$ 500):** não se justifica antes de ter volume; o
  storage Postgres cobre o MVP e o início da operação.

## Consequências

- Uma peça de infra (o Postgres que já existe) resolve fila + agendamento + retry.
- Dashboard pronto para operação solo.
- Ponto de atenção: fila sobre Postgres usa polling e lock de linha; tem teto de
  vazão. Quando aparecer contenção, migrar a camada de fila (RabbitMQ) — decisão
  registrada em ADR futuro, com os números que motivaram.
