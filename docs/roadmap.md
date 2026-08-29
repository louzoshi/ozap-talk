# Roadmap — sopa-talk

Objetivo: paridade funcional com o Umbler Talk, entregue em releases que já sejam
vendáveis. Cada release fecha um ciclo utilizável por um cliente real.

## R0 — Fundação (em andamento)

Esqueleto do monólito modular, build verde, infra local.

- [x] Solution, módulos, SharedKernel, hosts, CI local de build
- [x] Central Package Management, `Directory.Build.props`
- [x] docker-compose (Postgres + Valkey)
- [ ] Mediador CQRS próprio + pipeline de validação (FluentValidation)
- [ ] `TenantId` + filtro global EF Core + RLS + primeira migration por módulo
- [ ] Autenticação (ASP.NET Identity ou IdP) + cadastro de empresa/usuário + 2FA
- [ ] Outbox + bus in-process com entrega confiável
- [ ] Serilog estruturado + health checks + Sentry
- [ ] Pipeline CI (build + test + `dotnet list package --vulnerable`)

## R1 — Plataforma de Atendimento (primeiro produto vendável)

Multiatendimento sobre a WhatsApp Cloud API.

- [ ] **Channels**: conectar número via Cloud API, verificar assinatura do webhook,
      ingestão idempotente, enviar texto/mídia, templates (HSM), janela de 24h,
      tratamento de messaging tiers e rate limit
- [ ] **Inbox**: lista de conversas, atribuição manual e automática, setores/equipes,
      filas de atendimento, transferência, notas internas, tags, respostas rápidas
- [ ] Realtime: `InboxHub` (mensagem nova, conversa atribuída, digitando)
- [ ] Webchat widget para site (canal adicional)
- [ ] Frontend: tela de inbox, autenticação, gestão de usuários/setores
- [ ] Relatórios essenciais: volume de conversas, tempo de resposta, por atendente
- [ ] Opt-in / consentimento por contato (LGPD + política da Meta)

## R2 — CRM para WhatsApp

- [ ] **Crm**: pipelines/kanban, negócios (deals), estágios, motivos de perda
- [ ] `Contact` como view local do Crm, alimentada por integration events do Channels/Inbox
- [ ] Distribuição/roteamento automático de leads
- [ ] Frontend: board kanban, ficha do contato com histórico de conversa
- [ ] Importação de contatos via CSV
- [ ] Múltiplos pipelines (limite por plano)

## R3 — ChatBot para WhatsApp

- [ ] **Chatbot**: modelo de fluxo (nós, condições, ações), versionamento de fluxo
- [ ] Motor de execução: roda no `Workers`, dispara por gatilho (palavra-chave,
      horário, primeira mensagem, tag)
- [ ] Handoff para o Inbox quando o fluxo pede humano
- [ ] Agendamento de mensagens, lembretes, registro de compromisso
- [ ] Frontend: construtor visual de fluxo (drag-and-drop)

## R4 — Agente IA para WhatsApp

- [ ] **AiAgent**: base de conhecimento por empresa (upload de documentos, ingestão,
      embeddings, busca)
- [ ] Loop do agente com LLM (Claude) + ferramentas/skills (consultar pedido,
      agendar, abrir chamado)
- [ ] Critérios de handoff configuráveis (confiança baixa, pedido explícito, assunto
      sensível)
- [ ] Guardrails: não inventar preço, não dar diagnóstico, sempre citar fonte
- [ ] Frontend: configuração do agente, curadoria da base, histórico de execuções

## R5 — Plataforma / enterprise

- [ ] API pública + Webhooks para clientes
- [ ] Logs de auditoria avançados
- [ ] Multiunidade / multimarca
- [ ] Relatórios completos (por assunto/tag, funil, SLA)
- [ ] Planos e cobrança (limites de números, pipelines, equipes por assinatura)
- [ ] Billing do WhatsApp repassado/embutido

## Fora de escopo até haver demanda real

- Extrair módulo para microserviço
- RabbitMQ / MassTransit
- Multi-região
- App mobile nativo
