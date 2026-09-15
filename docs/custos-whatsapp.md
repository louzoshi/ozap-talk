# Custos WhatsApp / Meta — ozap-talk

> Estimativas de referência (agosto/2026). Os valores e regras da Meta mudam com
> frequência — **confirme sempre** em <https://developers.facebook.com/docs/whatsapp/pricing>
> antes de precificar planos. As datas abaixo (out/2026, ago/2026) vêm de relatos do
> mercado; validar na fonte oficial.

## As 3 contas de um agente de IA no WhatsApp

Não confundir. São cobranças separadas, de fornecedores diferentes.

| Conta | Do que é | Quem cobra |
|---|---|---|
| **1. Entrega de mensagem** | mandar / receber mensagem no WhatsApp | Meta |
| **2. "Cérebro" do agente** | o LLM ler a conversa + a base de conhecimento e escrever a resposta (tokens) | Anthropic (Claude). Desde ago/2026 a Meta também tem cobrança de token própria para IA — ver abaixo |
| **3. Infra** | servidor, banco, storage | provedor de hospedagem (ver `docs/custos.md`) |

**"Treinar" o agente NÃO é custo da Meta.** Não há fine-tuning de modelo. "Treinar"
aqui = montar o system prompt + subir os documentos da empresa (base de conhecimento)
+ definir as skills. Custo: tempo de engenharia, um pouco de storage, embeddings
(centavos). Não passa pela Meta.

## O que a Meta cobra (conta 1)

- **Acesso à Cloud API:** grátis. Sem mensalidade, sem taxa de setup.
- **Receber mensagem do cliente:** grátis, sempre.
- **Responder dentro da janela de 24h** (o cliente mandou mensagem; você responde em
  até 24h com texto livre — *service messages*): **grátis hoje**.
  ⚠️ A partir de **1º de outubro de 2026** essa janela deixa de ser 100% gratuita
  (service messages e respostas utility dentro da janela voltam a ser cobradas).
- **Enviar template** (para iniciar conversa ou responder fora das 24h): **pago**,
  por mensagem entregue, varia por país e categoria.

### Preço de template — referência Brasil

| Categoria | Para quê | Preço aprox. por mensagem |
|---|---|---|
| **Marketing** | promoção, novidade, reengajamento | ~US$ 0,0625 |
| **Utility** (utilidade) | confirmação de pedido, status, lembrete, cobrança | ~US$ 0,008 |
| **Authentication** | código de verificação (OTP) | ~US$ 0,0315 |
| **Service** (resposta livre na janela de 24h) | atendimento | US$ 0,00 (até out/2026) |

## Categoria "Meta Business Agent" (ago/2026) — afeta o ozap-talk diretamente

Relatos do mercado indicam que a Meta criou uma categoria para **respostas geradas
por IA**, cobrada **por token, ~US$ 2,00 por 1 milhão de tokens**. Ou seja: quando o
agente do ozap-talk responde um cliente, além do token pago à Anthropic, a Meta passa
a cobrar o token dela.

Status: **provável, confirmar na fonte oficial antes de usar em cálculo de preço.**

## Traduzindo para os fluxos do produto

### Cenário A — cliente manda mensagem, agente responde (dentro de 24h)

- Meta: grátis hoje (pode virar pago em out/2026, ou já pela categoria de IA)
- Anthropic: tokens da resposta — fração de centavo por mensagem
- Módulo responsável: `Channels` (janela de 24h) + `AiAgent`

### Cenário B — agente manda mensagem primeiro (ex.: "seu boleto vence amanhã")

- Meta: **sempre pago** — é um template. Utility ~US$ 0,008
- Anthropic: tokens
- Módulo responsável: `Channels` (envio de template) + `Chatbot`/`AiAgent` (gatilho)

## Quem paga

- **Produção:** quem paga é o **cliente do ozap-talk** (a empresa que contrata). O
  custo é repassado ou embutido no preço do plano — mesmo modelo do Umbler Talk.
- **Desenvolvimento / teste:** paga você, mas a Meta fornece um **número de teste**
  com envio gratuito para até **5 números verificados** — dá para testar o fluxo
  inteiro (webhook, janela de 24h, template, agente) sem gasto.

## Integração: direto na Cloud API, sem BSP

Decisão registrada em `docs/adr/0003` e `docs/custos.md`: integrar **direto na Cloud
API da Meta**, sem intermediário (Twilio, 360dialog, Gupshup, Zenvia). BSP adiciona
markup de ~US$ 0,003–0,010 por mensagem ou mensalidade fixa. O custo de ir direto
(verificação do Meta Business Manager, gestão de templates e de rate limit) já está
previsto no módulo `Channels`.

## Fontes

- <https://developers.facebook.com/docs/whatsapp/pricing> (oficial — sempre a referência final)
- [Authgear — WhatsApp API Pricing (2026)](https://www.authgear.com/post/whatsapp-api-pricing/)
- [respond.io — WhatsApp Business API Pricing 2026](https://respond.io/blog/whatsapp-business-api-pricing)
- [Peppercloud — Free 24-Hour Window Ends in October](https://blog.peppercloud.com/whatsapp-api-pricing-everything-you-need-to-know/)
- [Wati — WhatsApp API Pricing 2026](https://www.wati.io/en/blog/whatsapp-api-pricing-guide/)
