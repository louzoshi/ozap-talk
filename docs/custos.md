# Custos — sopa-talk

Todos os valores são **estimativas de referência** (agosto/2026). Confirme os preços
atuais antes de decidir. Moeda: USD salvo indicação.

## Infraestrutura (deploy pequeno, com redundância mínima)

### Opção A — VPS (Hetzner Cloud) — recomendada para começar

| Item | Especificação | Custo/mês |
|---|---|---|
| App server ×2 | CPX21 (3 vCPU, 4 GB) — API + Workers | ~€16 |
| PostgreSQL gerenciado | plano inicial com backup/PITR | ~€20–30 |
| Valkey/Redis | container no app server ou 1 CX22 dedicado | €0–8 |
| Load balancer | Hetzner LB11 | ~€6 |
| Object storage | Cloudflare R2 (sem egress) | €0–5 |
| **Total infra** | | **~€50–75/mês** |

### Opção B — PaaS (Render / Railway / Fly.io)

Menos trabalho de operação, mais caro: **~US$ 80–160/mês** para API + Workers +
Postgres + Redis com um tier pago mínimo.

### Opção C — AWS / Azure

Setup HA pequeno (2× container, RDS/Flexible Server, ElastiCache, ALB):
**~US$ 150–350/mês**. Escolha só se o cliente exige nuvem específica.

## Serviços SaaS de apoio

| Serviço | Uso | Custo/mês |
|---|---|---|
| Sentry | erros + traces | US$ 0 (dev) → ~26 |
| E-mail transacional (Resend / Postmark) | convites, alertas, relatórios | US$ 10–20 |
| Domínio + TLS | Let's Encrypt grátis | ~US$ 15/ano |
| Monitor de uptime (BetterStack / UptimeRobot) | | US$ 0–20 |

## WhatsApp Cloud API (o custo variável que dói)

Detalhamento completo (as 3 contas, janela de 24h, categoria "Meta Business Agent"
para IA, quem paga): **`docs/custos-whatsapp.md`**.

Desde 2025 a Meta cobra **por mensagem de template**, com preço por país e por
categoria. Conversa iniciada pelo usuário (categoria *service*) é **gratuita**
(até out/2026, segundo relatos do mercado).

Referência Brasil (confirmar em <https://developers.facebook.com/docs/whatsapp/pricing>):

| Categoria de template | Preço aproximado por mensagem |
|---|---|
| Marketing | ~US$ 0,0625 |
| Utility (utilidade) | ~US$ 0,008 |
| Authentication (código) | ~US$ 0,0315 |
| Service (resposta na janela de 24h) | US$ 0,00 |

**Decisão:** integrar **direto na Cloud API da Meta**, sem BSP (Twilio, 360dialog,
Gupshup, Zenvia). BSP adiciona markup por mensagem ou mensalidade fixa. O custo de
ir direto é: verificação do Meta Business Manager, gestão de templates e de rate
limit por conta própria — já previsto no módulo `Channels`.

Este custo é **repassado ou embutido** no preço cobrado do cliente sopa-talk, não
absorvido.

## Licenças de software (o núcleo é todo gratuito para uso comercial)

| Componente | Licença | Custo |
|---|---|---|
| .NET / ASP.NET Core / EF Core | MIT | 0 |
| C# | — | 0 |
| PostgreSQL | PostgreSQL License | 0 |
| React / Vite | MIT | 0 |
| Valkey | BSD-3 | 0 |
| Hangfire.Core / .AspNetCore | LGPLv3 (ok em SaaS fechado) | 0 |
| Hangfire.PostgreSql | MIT | 0 |
| Serilog, FluentValidation, xUnit, NetArchTest | Apache-2.0 / MIT | 0 |
| **Hangfire.Pro** (storage Redis, batches) | comercial | ~US$ 500/dev, perpétuo — **só se precisar** |
| MediatR / AutoMapper | comercial acima de faturamento | evitados (mediador próprio, mapeamento manual) |

## Piso realista para operar profissionalmente

**~US$ 80–150/mês de infra + apoio**, mais o variável de WhatsApp proporcional ao
volume dos clientes. Um único cliente pagante de ticket médio já cobre a infra.

## Próximo passo

Rodar um teste real: conectar 1 número na Cloud API, enviar ~1.000 mensagens de
cada categoria, medir custo Meta + carga no Postgres/Hangfire, e registrar os
números reais aqui.
