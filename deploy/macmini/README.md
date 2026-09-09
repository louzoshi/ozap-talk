# Deploy no Mac mini

Instância única: **Postgres + um processo .NET + um túnel**. Sem Docker, sem Redis,
sem processo de workers separado — a API já roda o Hangfire dentro dela.

| | |
|---|---|
| App | `/usr/local/sopa-talk/app` |
| Segredos | `/usr/local/sopa-talk/etc/sopa-talk.env` (600) |
| Logs | `/usr/local/sopa-talk/logs/api.log` |
| Backups | `/usr/local/sopa-talk/backups` (14 dias) |
| Porta interna | `127.0.0.1:5080` |

Os serviços rodam como **LaunchAgent** do seu usuário, não como daemon de root: é
bem mais simples de operar e casa com o `brew services` do Postgres. O preço é que
**precisa haver uma sessão aberta** — ligue o login automático do usuário em
_Ajustes do Sistema › Usuários e Grupos › Opções de início de sessão_.

---

## Antes de começar: leia isto

Migrar o número da empresa para a **WhatsApp Cloud API** é irreversível na prática:
depois disso o número **para de funcionar no app WhatsApp e no WhatsApp Business**
do celular. Todo o atendimento passa a acontecer só pela sopa-talk. Não existe meio
caminho — o número está na API ou no app, nunca nos dois.

Além disso, hoje o produto:

- **envia só texto.** Mídia (foto, áudio, PDF) e template (HSM) estão no R1, ainda abertos.
- **só responde dentro da janela de 24h.** Fora dela a Meta exige template aprovado.
  Ou seja: o cliente precisa ter falado primeiro.
- **não envia e-mail de convite.** O `IInvitationEmailSender` registrado é o
  `LoggingInvitationEmailSender`. A tela de Equipe mostra o link de aceite na hora em
  que você convida — você copia e manda no WhatsApp. Para 5 pessoas resolve.
- **não tem refresh token.** O JWT expira em 60 min e o time faz login de novo.

Nada disso bloqueia o começo, mas é melhor saber antes do que descobrir na segunda-feira.

---

## 1. Setup (uma vez)

No Mac mini, com o repositório clonado:

```bash
./deploy/macmini/setup.sh
```

O script instala as dependências (`dotnet-sdk`, `node@22`, `postgresql@17`,
`cloudflared`), cria os diretórios, cria o banco e o papel `sopatalk`, gera os
segredos, instala os serviços do launchd e configura a máquina para não dormir e
voltar sozinha depois de queda de energia. Pode rodar de novo sem medo.

Depois, abra `/usr/local/sopa-talk/etc/sopa-talk.env` e preencha:

| Variável | De onde vem |
|---|---|
| `Channels__AppSecret` | Meta for Developers › seu app › Configurações › Básico |
| `Accounts__AppBaseUrl` | sua URL pública (a mesma do túnel) |

A senha do banco, a chave do JWT e o `WebhookVerifyToken` já vieram gerados.

## 2. Publicar

```bash
./deploy/macmini/publish.sh
```

Builda a SPA (que sai direto no `wwwroot` da API), publica o backend para o RID do
Mac, troca a pasta do app e reinicia o serviço. Se o build falhar, o que está no ar
continua no ar. As migrations sobem com o app — `Database__MigrateOnStartup=true`
no env file.

A versão anterior fica em `/usr/local/sopa-talk/app.previous`. Para voltar:

```bash
S=/usr/local/sopa-talk
rm -rf $S/app && mv $S/app.previous $S/app
launchctl kickstart -k gui/$(id -u)/team.sopa.talk.api
```

## 3. Túnel

A Meta precisa alcançar o Mac mini por HTTPS numa URL fixa. O túnel do Cloudflare
resolve sem abrir porta no roteador e sem lidar com certificado. É gratuito e exige
um domínio seu apontado para a Cloudflare.

```bash
cloudflared tunnel login
cloudflared tunnel create sopa-talk
cloudflared tunnel route dns sopa-talk atendimento.suaempresa.com.br

cp deploy/macmini/cloudflared/config.yml.example /usr/local/sopa-talk/etc/cloudflared.yml
# ajuste credentials-file e hostname

launchctl bootstrap gui/$(id -u) ~/Library/LaunchAgents/team.sopa.talk.tunnel.plist
```

Confira: `curl -fsS https://atendimento.suaempresa.com.br/health`

## 4. WhatsApp

No [Meta for Developers](https://developers.facebook.com/), no app com o produto
WhatsApp adicionado:

1. **Webhook** → URL de callback `https://SEU-DOMINIO/api/channels/webhook`,
   token de verificação = o `Channels__WebhookVerifyToken` do env file.
   Assine o campo `messages`.
2. **Número** → migre o número da empresa (ver o aviso lá em cima), aprove o nome
   de exibição, complete a verificação do negócio.
3. Na sopa-talk, conecte o canal (`POST /api/channels`) com o Phone Number ID, o
   WABA ID e o token de acesso permanente do app.

Detalhes e o caminho de teste com número da sandbox: `docs/whatsapp-teste.md`.

## 5. Convidar o time

O dono se cadastra pela tela de cadastro (`/register`) — é ele quem cria a empresa.
Para os outros 4, em **Equipe** (visível para Proprietário e Admin):

1. Convide por e-mail e papel. Para atendimento, use **Operador**
   (papéis em `docs/papeis-e-permissoes.md`).
2. O diálogo mostra o link de aceite — copie e mande para a pessoa.
   O convite vale **72 horas**.
3. Ela abre o link, escolhe o nome e **define a própria senha** (mínimo 8 caracteres),
   e já entra logada.

Não existe "senha padrão" definida pelo administrador: cada pessoa cria a sua no
aceite do convite. Se o link se perder, ele também sai no log:

```bash
grep -i convite /usr/local/sopa-talk/logs/api.log | tail -5
```

---

## Operação

```bash
/usr/local/sopa-talk/bin/status.sh                      # retrato geral

launchctl kickstart -k gui/$(id -u)/team.sopa.talk.api  # reiniciar a API
launchctl bootout   gui/$(id -u)/team.sopa.talk.api     # parar
tail -f /usr/local/sopa-talk/logs/api.log               # acompanhar

/usr/local/sopa-talk/bin/backup.sh                      # backup agora
```

**Atualizar:** `git pull && ./deploy/macmini/publish.sh`.

**Restaurar um backup:**

```bash
launchctl bootout gui/$(id -u)/team.sopa.talk.api
pg_restore --clean --if-exists -h localhost -U sopatalk -d sopatalk ARQUIVO.dump
launchctl bootstrap gui/$(id -u) ~/Library/LaunchAgents/team.sopa.talk.api.plist
```

Os dumps ficam só no Mac mini. Copie o diretório de backups para fora da máquina
(iCloud, um NAS, um bucket) — um Mac mini é um ponto único de falha.

## Segurança

- A API escuta **só em `127.0.0.1`**. Quem entra de fora passa pelo túnel, com HTTPS.
  Para liberar acesso direto na rede local, troque `ASPNETCORE_URLS` por
  `http://0.0.0.0:5080` no env file.
- O painel do Hangfire em `/jobs` fica **fechado em produção** — o
  `HangfireDashboardAuthorizationFilter` só libera em Development. Para inspecionar,
  faça um túnel SSH até a porta 5080.
- `sopa-talk.env` é 600 e nunca vai para o repositório. Trocar o `Jwt__SigningKey`
  derruba todas as sessões abertas.
- Row-Level Security no Postgres continua na lista do R0. Com um tenant só isso não
  te afeta hoje; antes do segundo cliente, sim.

## Quando isso deixa de servir

Este desenho aguenta bem 5 atendentes e um número. Passa a apertar quando:

- entrar um **segundo número ou um segundo cliente** (aí RLS e outbox param de ser opcionais);
- o time crescer para o ponto de precisar de **mais de uma instância** (aí entra o
  Valkey como backplane do SignalR — a string já está no `appsettings.json`);
- a **queda do Mac mini** virar prejuízo, e não incômodo (aí é hora de sair da mesa).
