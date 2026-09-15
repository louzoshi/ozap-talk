# Segurança — ozap-talk

Estado do que está implementado. O que falta está marcado como TODO e no `roadmap.md`.

## Autenticação

Módulo **`Accounts`**. Sem IdP externo por enquanto — o próprio produto emite os tokens.

- **Cadastro:** `POST /api/accounts/register` cria a empresa (`Account`) + o usuário
  dono (`User`, papel `Owner`) numa transação, e já devolve um token (login automático).
- **Login:** `POST /api/accounts/sessions` valida e-mail + senha, devolve token.
- **Usuário atual:** `GET /api/accounts/me` (autenticado).

### Senhas

`Pbkdf2PasswordHasher` embrulha o hasher do ASP.NET Core Identity: **PBKDF2
HMAC-SHA512, salt por usuário, 210.000 iterações** (formato v3). `Verify` é
constant-time e devolve `false` para hash malformado. O login roda um `Verify` mesmo
quando o e-mail não existe, para não vazar por timing qual e-mail está cadastrado.
Erro de credencial é sempre o mesmo (`auth.invalid_credentials`), nunca diz se foi
o e-mail ou a senha.

### Token (JWT)

- HS256, chave simétrica de `Jwt:SigningKey`.
- Claims: `sub` (user id), `tenant_id` (account id), `email`, `name`, `role`.
- Validação no host: issuer, audience, assinatura, expiração (skew de 30s).
  `MapInboundClaims = false` — claims ficam com o nome original.
- Expira em `Jwt:AccessTokenMinutes` (padrão 60). **Sem refresh token ainda** (TODO).

### Configuração da chave

| Ambiente | Onde | Valor |
|---|---|---|
| Development | `src/OzapTalk.Api/appsettings.Development.json` | chave fixa de dev (não é segredo, versionada de propósito) |
| Produção / staging | variável de ambiente `Jwt__SigningKey` ou secret manager | **≥ 32 chars, aleatória, única por ambiente** |

Para sobrescrever localmente sem tocar no arquivo: `dotnet user-secrets set "Jwt:SigningKey" "<valor>" --project src/OzapTalk.Api`.

## Autorização

- **Fallback policy:** todo endpoint exige usuário autenticado por padrão. Endpoints
  públicos declaram `.AllowAnonymous()` explicitamente (`register`, `sessions`,
  `/health`, docs em dev, `_ping`).
- **Políticas por papel:** `operator` (`Owner`/`Admin`/`Operator`), `admin`
  (`Owner`/`Admin`), `owner` (`Owner`, reservada para o financeiro). Papéis:
  `Owner`, `Admin`, `Operator`, `Member`. Detalhes, alçada e fluxo de convite em
  [`papeis-e-permissoes.md`](papeis-e-permissoes.md).

## Multi-tenancy (isolamento entre clientes)

Duas barreiras, conforme `docs/adr/0004`.

### 1. Aplicação — implementado

- Toda entidade com dado de cliente implementa `ITenantOwned` (`Guid TenantId`).
- `TenantResolutionMiddleware` copia a claim `tenant_id` do token para o
  `ITenantContext` scoped (uma instância por request; jobs setam o seu).
- `TenantDbContext` (base de todo DbContext de módulo):
  - **filtro global** por entidade tenant-owned: `e => e.TenantId == CurrentTenantId`.
    `CurrentTenantId` é membro do contexto, então o EF re-liga à instância em
    execução a cada query (seguro com o model em cache).
  - **`TenantSaveChangesInterceptor`**: carimba `TenantId` no insert; **bloqueia**
    update/delete cujo tenant difere do escopo atual.
- Exceção documentada: `IUserRepository.FindByEmailAsync` / `EmailExistsAsync` usam
  `IgnoreQueryFilters()` — login e cadastro não têm tenant em escopo. Nenhuma outra
  query ignora o filtro.

### 2. Banco — Row-Level Security — TODO

Ainda não implementado. Próximo item: policy de RLS no Postgres usando uma session
variable (`app.tenant_id`) setada por conexão via interceptor, para que uma falha no
filtro da aplicação ainda seja barrada pelo banco.

## Hangfire dashboard

`/jobs` só é acessível em **Development** (`HangfireDashboardAuthorizationFilter`).
Produção: TODO — cookie/sessão de admin da plataforma.

## O que ainda falta (R0)

- Refresh token + revogação
- Row-Level Security no Postgres
- 2FA (TOTP)
- Lockout / rate limiting no login
- Fluxo de convite de usuário (hoje só o dono é criado)
- Auditoria de eventos de segurança
- Rotação de chave JWT (kid / múltiplas chaves de validação)
- Verificação de e-mail
- LGPD: exclusão de dados, registro de consentimento, contrato de operador
