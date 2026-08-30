# WhatsApp para testar o sopa-talk

> **Você NUNCA usa seu número pessoal.** Assim que um número entra na WhatsApp Cloud
> API, ele **não funciona mais** no app comum do WhatsApp / WhatsApp Business. Receber
> mensagem no seu número pessoal é seguro; **registrar** ele na API é o que você perde.

## Resumo da decisão

| Fase | O que usar | Custo | Risco |
|---|---|---|---|
| **Agora (dev)** | Número de teste **grátis da Meta** | R$ 0, sem cartão | Zero |
| Depois (produção / >5 destinatários) | Chip pré-pago dedicado ao projeto | ~R$ 10 (o chip) + templates | Baixo |

Comece com o número de teste da Meta. Adicione seu número pessoal como **destinatário**
(só para *receber* as mensagens de teste no seu WhatsApp real) — isso é seguro.

## O número de teste da Meta

Quando você cria um app no Meta for Developers e adiciona o produto "WhatsApp", a
Meta **provisiona automaticamente** um número de teste:

- Grátis, sem adicionar forma de pagamento
- Envia mensagem para até **5 números destinatários** que você cadastra
- Mensagens ilimitadas para esses 5, sem cobrança
- Não precisa de verificação de negócio (Business Verification)
- **Limitações:** número é da Meta (não seu), só 5 destinatários, não serve para
  produção

### Passo a passo

1. <https://developers.facebook.com> → entrar com uma conta Facebook (pode criar uma
   só para isso).
2. **My Apps → Create App → tipo "Business"**.
3. No painel do app: **Add Product → WhatsApp → Set up**. Isso cria uma WhatsApp
   Business Account de teste + o número de teste.
4. Vá em **WhatsApp → API Setup**. Você verá:
   - o número de teste e o `Phone number ID`
   - um **token de acesso temporário** (validade 24h)
   - campo **"To"** para adicionar destinatários → adicione seu número pessoal,
     confirme o código que chega no seu WhatsApp
5. Clique em **Send message** (template `hello_world`) → chega no seu WhatsApp real.
6. Para o app não depender do token de 24h: **Business Settings → System Users →**
   criar um system user, dar acesso ao app, **gerar token permanente** com as
   permissões `whatsapp_business_messaging` e `whatsapp_business_management`.
7. **Webhook:** em WhatsApp → Configuration, aponte a URL do webhook para o seu
   `localhost` exposto por um túnel (ver abaixo) e defina um *verify token* qualquer.

### O que guardar nas configs (nunca no git)

```
WhatsApp__PhoneNumberId      = ...
WhatsApp__BusinessAccountId  = ...
WhatsApp__AccessToken        = ...        (token do system user)
WhatsApp__WebhookVerifyToken = (string que você inventar)
WhatsApp__AppSecret          = ...        (para validar a assinatura do webhook)
```

Use `dotnet user-secrets` (dev) ou variáveis de ambiente. O `.gitignore` já bloqueia
`appsettings.*.local.json` e `.env`.

## Túnel para o webhook (localhost)

A Meta precisa alcançar sua máquina para entregar os webhooks. Em dev:

```bash
# cloudflared (grátis, sem conta)
cloudflared tunnel --url http://localhost:5080
# -> te dá uma URL https pública temporária; use ela + /api/channels/webhook
```

Alternativa: `ngrok http 5080` (precisa de conta grátis).

## Quando precisar de um número real (dedicado ao projeto)

- **Melhor opção:** um **chip pré-pago** (Vivo/Claro/TIM), ~R$ 10, num celular
  velho ou slot dual-SIM. **Nunca instale WhatsApp nele.** Registre direto na Cloud
  API. Custo recorrente: só a recarga mínima para manter o chip ativo.
- **eSIM** também serve.
- **Evite:** números de "receber SMS grátis" online — a Meta bloqueia a maioria e são
  compartilhados. VoIP funciona às vezes, mas é arriscado.

### Custo de um número real na Cloud API

O número em si = só o custo do chip. A Meta cobra **por template enviado**
(marketing / utility / authentication — ver `docs/custos-whatsapp.md`). Resposta
dentro da janela de 24h e conversa de teste = **grátis**. Ou seja: um número de
desenvolvimento fica em ~R$ 0 de taxa Meta se você não disparar templates.

Forma de pagamento: só é exigida para número real, e você **só é cobrado** se enviar
template pago. Dá para definir **limite de gasto** na WhatsApp Business Account.

### Verificação de negócio

- Número de teste: **não precisa**.
- Número real "não verificado": funciona, com limites (ex.: 250 conversas iniciadas
  pelo negócio por 24h, nome de exibição pendente).
- **Business Verification** (enviar documentos da empresa): sobe os limites. Só
  quando for para produção — não é necessário para desenvolver.

## Fontes

- <https://developers.facebook.com/docs/whatsapp/cloud-api/get-started>
- <https://developers.facebook.com/docs/whatsapp/cloud-api/get-started/add-a-phone-number>
- `docs/custos-whatsapp.md` (o que é pago)
