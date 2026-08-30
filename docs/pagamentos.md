# Pagamentos e cobrança — sopa-talk

## Decisões

### 1. Fica neste projeto, como um módulo

Cobrança é **core do SaaS** — ela controla o acesso às funcionalidades por plano
(limites de números, pipelines, equipes) e mede o uso de WhatsApp para repassar. É
um módulo do monólito: `Billing`, schema `billing`. **Não** é serviço separado.

### 2. Nunca construímos um processador de pagamento

Integramos um **PSP / gateway**. O sopa-talk **nunca toca em número de cartão** —
usa checkout hospedado / campos tokenizados do PSP, mantendo o escopo PCI no mínimo
(SAQ-A). O PSP cuida de: cobrança recorrente, Pix, boleto, cartão, **parcelamento**,
antifraude, liquidação.

### 3. PSP — a escolher quando houver o primeiro cliente pagante

| PSP | Pix | Cartão recorrente | Parcelado | Boleto | Observação |
|---|---|---|---|---|---|
| **Stripe** | sim | sim (nativo) | sim | sim | Melhor API/docs, produto "Billing" pronto. Suporte BR só em inglês |
| **Pagar.me** (Stone) | sim | sim | sim | sim | BR-native, bom para split/marketplace |
| **Iugu** | sim | sim | sim | sim | Focado em recorrência SaaS BR |
| **Asaas** | sim | sim | sim | sim | Simples, bom para começar, PJ pequena |
| **Mercado Pago** | sim | sim | sim | sim | Alcance grande, API menos limpa |
| **Vindi** | sim | sim | sim | sim | Só recorrência |

Recomendação inicial: **Stripe** se a qualidade de API/webhooks pesa mais;
**Iugu ou Asaas** se quiser suporte local e Pix/boleto mais "brasileiros" de fábrica.

### 4. Pix recorrente

- **Pix comum:** gera um QR novo a cada ciclo; o cliente paga manualmente. Serve,
  mas tem churn de esquecimento.
- **Pix Automático** (2025): autorização de débito recorrente. Preferir quando o PSP
  suportar bem.
- **Cartão:** recorrência nativa, é o caminho mais confiável para assinatura.

### 5. Parcelamento

É recurso de cartão, o PSP resolve. Decidir por plano: **parcelado sem juros**
(sopa-talk absorve a taxa) ou **com juros** (repassado ao cliente). Normalmente só
faz sentido em plano anual.

## O que o módulo `Billing` vai conter

- **Aggregates:** `Subscription` (estado, plano, ciclo, trial), `Plan` + `PlanLimits`,
  `Invoice`, `PaymentMethod` (só o token do PSP), `UsageRecord`.
- **Webhook do PSP:** ingestão idempotente (`payment.succeeded`, `payment.failed`,
  `subscription.updated`, …).
- **Dunning:** retry de pagamento falho, aviso, suspensão, cancelamento.
- **Entitlements:** checagem "esse tenant pode ter mais um número/pipeline/equipe?"
  consumida pelos outros módulos.
- **Uso medido:** `Channels` publica evento a cada template enviado → `Billing`
  acumula `UsageRecord` → entra na próxima fatura (repasse do custo WhatsApp).
- **Eventos publicados:** `SubscriptionActivated`, `SubscriptionPastDue`,
  `SubscriptionCanceled` — outros módulos reagem (ex.: bloquear envio quando past due).

## Quando construir

**Não antes do primeiro cliente-piloto.** Sequência:

1. **Piloto (1–2 clientes):** cobrança manual — link de pagamento do PSP ou nota
   fiscal + Pix na mão. Zero código.
2. **Estrutura de planos definida** (com base no que o piloto mostrou que vale):
   fatia mínima do módulo `Billing` — assinatura + webhook + entitlement por plano.
3. **R5 / escala:** uso medido, dunning completo, portal de autoatendimento, anual,
   parcelamento.

Construir o módulo inteiro antes de ter usuário pagante é prematuro — a estrutura de
planos muda depois do primeiro contato real com o mercado.

## Fora de escopo

- Emissão de nota fiscal (NF-e/NFS-e) — integrar um emissor (eNotas, NFe.io,
  Omie) quando a operação exigir. Fica no `Billing` como integração.
- Antifraude próprio — é do PSP.
- Split de pagamento / marketplace — só se o modelo de negócio mudar.
