# ADR 0001 — Monólito modular, não microserviços

- Status: aceito
- Data: 2026-08-29

## Contexto

ozap-talk cobre quatro áreas funcionais (Plataforma de Atendimento, CRM, ChatBot,
Agente IA) que o mercado enxerga como produtos distintos. Time: uma pessoa.
Objetivo: produto pronto para clientes reais, não protótipo de estudo.

## Decisão

Construir como **monólito modular**: um deploy de API, um de workers, um banco
PostgreSQL com um schema por módulo. Módulos (`Channels`, `Inbox`, `Crm`,
`Chatbot`, `AiAgent`) com fronteiras fortes:

- comunicação entre módulos só por integration events (bus in-process hoje);
- nenhum módulo acessa schema/tipos internos de outro;
- cada módulo se auto-registra por um `IModuleInstaller`; o host só compõe.

## Alternativas consideradas

- **Microserviços / repo por produto**: rejeitado. Quatro bancos, quatro deploys,
  sincronização de contato/conversa entre serviços, auth distribuída — custo
  operacional incompatível com um dev e sem ganho real nesta escala. O próprio
  Umbler Talk é uma aplicação só.
- **Monólito sem módulos (camadas por tipo técnico)**: rejeitado. Vira big ball of
  mud; impossível extrair parte depois sem reescrita.

## Consequências

- Positivo: um deploy, uma transação, refactor cross-módulo barato, onboarding
  simples, custo de infra mínimo.
- Positivo: um módulo bem isolado hoje é um microserviço fácil amanhã (trocar bus
  in-process por RabbitMQ + outbox, mover o schema).
- Negativo: disciplina manual nas fronteiras — mitigado por testes de arquitetura
  (`NetArchTest`) no CI.
- Negativo: um bug de recurso pode derrubar o processo inteiro — mitigado por 2+
  instâncias atrás de load balancer e workers em processo separado.
