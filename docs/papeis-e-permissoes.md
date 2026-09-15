# Papéis e permissões

Cada usuário pertence a **uma** conta (tenant) e tem **um** papel. O papel define o
que ele pode fazer na ferramenta. Modelo espelhado do Umbler Talk.

## Os quatro papéis

| Papel (`MembershipRole`) | Rótulo | O que pode |
|---|---|---|
| `Owner` | Proprietário | Tudo na ferramenta **+ painel financeiro**. Exatamente um por conta; definido no registro da empresa. |
| `Admin` | Admin | Tudo na ferramenta, **sem** painel financeiro. |
| `Operator` | Operador | Enviar mensagens para contatos, encerrar/atribuir conversas, ajustar configurações básicas de atendimento. |
| `Member` | Membro | Somente leitura: acompanha conversas, **não** envia mensagens nem altera configurações. |

> O painel financeiro ainda não existe. Até existir, Owner e Admin têm o mesmo alcance
> prático; a policy `"owner"` está reservada para quando o Billing entrar.

## Alçada — quem gerencia quem

`OzapTalk.Accounts.Domain.Users.MembershipRules.CanManage(ator, alvo)`:

- **Owner** gerencia todos, menos outro Owner.
- **Admin** gerencia apenas `Operator` e `Member` (não mexe em Admin nem Owner).
- **Operator** e **Member** não gerenciam ninguém.

Trocar o papel de alguém exige alçada sobre o **papel atual** e sobre o **papel novo**.
Ninguém altera o papel do Owner, nem o próprio papel. Transferência de propriedade é
um fluxo à parte (fora de escopo).

## Convite

1. `POST /api/accounts/invitations` (policy `admin`) com `{ email, role }`. O papel
   precisa estar dentro da alçada de quem convida; `Owner` nunca é convidável.
2. Gera um token (32 bytes aleatórios). Só o **hash SHA-256** vai para o banco
   (`accounts.invitations`); o token cru vai no link. Validade: 72 h.
3. Em desenvolvimento não há envio de e-mail: `LoggingInvitationEmailSender` registra
   o link no log e a resposta do `POST` traz `acceptUrl`. Trocar por um provedor real
   (`IInvitationEmailSender`) antes de produção.
4. `GET /api/accounts/invite/{token}` — prévia pública (empresa, e-mail, papel).
5. `POST /api/accounts/invite/{token}` com `{ displayName, password }` — cria o
   `User` com o papel do convite, marca o convite como aceito e já devolve um token
   de sessão (auto-login).
6. `GET /api/accounts/invitations` lista os pendentes; `DELETE /api/accounts/invitations/{id}`
   cancela.

O e-mail é identificador **global** de login (índice único em `accounts.users.Email`
sem escopo de tenant): um e-mail já usado em qualquer conta não pode ser convidado.

## Gestão de membros

| Rota | Policy | Efeito |
|---|---|---|
| `GET /api/accounts/members` | `admin` | Lista os usuários da conta. |
| `PATCH /api/accounts/members/{id}/role` | `admin` | Troca o papel (sujeito à alçada). |
| `DELETE /api/accounts/members/{id}` | `admin` | Desativa o usuário (`IsActive = false`). |

## Onde as permissões são aplicadas

Policies em `src/OzapTalk.Api/Program.cs` (a partir do claim `role` do JWT):

| Policy | Papéis | Usada em |
|---|---|---|
| `operator` | Owner, Admin, Operator | Inbox: enviar mensagem, atribuir, encerrar, marcar como lida. |
| `admin` | Owner, Admin | Conectar número WhatsApp; toda a gestão de equipe. |
| `owner` | Owner | (reservada para o painel financeiro) |
| _fallback_ | qualquer autenticado | Leitura da Inbox, `/accounts/me`. É o teto do `Member`. |

## Limitação conhecida — staleness do JWT

Trocar o papel de alguém (ou desativá-lo) **não** invalida o access token que a pessoa
já tem. O papel novo passa a valer no próximo login ou quando o token expira (~60 min).
Refresh token + revogação estão no R0 e resolvem isso.
