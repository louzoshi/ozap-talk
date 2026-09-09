#!/usr/bin/env bash
#
# Setup único do Mac mini que hospeda o sopa-talk.
# Idempotente: rodar de novo não estraga nada. Não roda como root.
#
# Uso:  ./deploy/macmini/setup.sh
#
set -euo pipefail

SOPA_HOME="${SOPA_HOME:-/usr/local/sopa-talk}"
SOPA_USER="$(id -un)"
PG_FORMULA="postgresql@17"
DB_NAME="sopatalk"
DB_USER="sopatalk"
ENV_FILE="$SOPA_HOME/etc/sopa-talk.env"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

log()  { printf '\n\033[1m==> %s\033[0m\n' "$*"; }
warn() { printf '\033[33m!  %s\033[0m\n' "$*"; }
die()  { printf '\033[31mx  %s\033[0m\n' "$*" >&2; exit 1; }

[ "$(id -u)" -ne 0 ] || die "Não rode como root. O serviço roda no seu usuário ($SOPA_USER)."
[ "$(uname -s)" = "Darwin" ] || die "Este script é para macOS."

# --- 1. Homebrew e dependências --------------------------------------------
command -v brew >/dev/null || die "Instale o Homebrew primeiro: https://brew.sh"
BREW_PREFIX="$(brew --prefix)"

log "Instalando dependências"
brew install --quiet node@22 "$PG_FORMULA" cloudflared
brew list --cask dotnet-sdk >/dev/null 2>&1 || brew install --cask dotnet-sdk

export PATH="$BREW_PREFIX/opt/$PG_FORMULA/bin:$BREW_PREFIX/opt/node@22/bin:/usr/local/share/dotnet:$PATH"

command -v dotnet >/dev/null || die "dotnet não está no PATH. Abra um terminal novo e rode de novo."
REQUIRED_SDK="$(python3 -c "import json;print(json.load(open('$REPO_ROOT/global.json'))['sdk']['version'])")"
dotnet --list-sdks | grep -q "^${REQUIRED_SDK%.*}" \
  || warn "global.json pede o SDK $REQUIRED_SDK. Instalado: $(dotnet --version). Ajuste se o build falhar."

# --- 2. Diretórios ----------------------------------------------------------
log "Preparando $SOPA_HOME"
sudo mkdir -p "$SOPA_HOME"/{app,bin,etc,logs,backups}
sudo chown -R "$SOPA_USER":staff "$SOPA_HOME"
chmod 700 "$SOPA_HOME/etc"

# --- 3. Postgres ------------------------------------------------------------
log "Subindo o Postgres"
brew services start "$PG_FORMULA" >/dev/null

for _ in $(seq 1 30); do
  pg_isready -q -h localhost && break
  sleep 1
done
pg_isready -q -h localhost || die "Postgres não respondeu. Veja: brew services info $PG_FORMULA"

# A senha é gerada uma vez e reaproveitada nas execuções seguintes.
if [ -f "$ENV_FILE" ] && grep -q '^SOPA_DB_PASSWORD=' "$ENV_FILE"; then
  DB_PASSWORD="$(grep '^SOPA_DB_PASSWORD=' "$ENV_FILE" | head -1 | cut -d= -f2-)"
  log "Reaproveitando os segredos de $ENV_FILE"
else
  DB_PASSWORD="$(openssl rand -base64 32 | tr -dc 'A-Za-z0-9' | cut -c1-32)"
fi

log "Criando papel e banco"
psql -q -d postgres -v ON_ERROR_STOP=1 \
  -v user="$DB_USER" -v pass="$DB_PASSWORD" <<'SQL'
SELECT format('CREATE ROLE %I LOGIN', :'user')
 WHERE NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = :'user') \gexec
SELECT format('ALTER ROLE %I PASSWORD %L', :'user', :'pass') \gexec
SQL

psql -tAq -d postgres -c "SELECT 1 FROM pg_database WHERE datname = '$DB_NAME'" | grep -q 1 \
  || createdb -O "$DB_USER" "$DB_NAME"

# --- 4. Arquivo de ambiente -------------------------------------------------
if [ -f "$ENV_FILE" ]; then
  log "Mantendo $ENV_FILE (já existe)"
else
  log "Gerando $ENV_FILE"
  JWT_KEY="$(openssl rand -base64 48)"
  VERIFY_TOKEN="$(openssl rand -hex 24)"
  sed \
    -e "s|^SOPA_DB_PASSWORD=.*|SOPA_DB_PASSWORD=$DB_PASSWORD|" \
    -e "s|^Jwt__SigningKey=.*|Jwt__SigningKey=$JWT_KEY|" \
    -e "s|^Channels__WebhookVerifyToken=.*|Channels__WebhookVerifyToken=$VERIFY_TOKEN|" \
    "$REPO_ROOT/deploy/macmini/sopa-talk.env.example" > "$ENV_FILE"
  chmod 600 "$ENV_FILE"
fi

# --- 5. Scripts de serviço --------------------------------------------------
log "Instalando scripts em $SOPA_HOME/bin"
for script in run-api.sh backup.sh status.sh; do
  sed -e "s|__BREW_PREFIX__|$BREW_PREFIX|g" \
      -e "s|__SOPA_HOME__|$SOPA_HOME|g" \
      -e "s|__PG_FORMULA__|$PG_FORMULA|g" \
      "$REPO_ROOT/deploy/macmini/$script" > "$SOPA_HOME/bin/$script"
  chmod 755 "$SOPA_HOME/bin/$script"
done

# --- 6. launchd -------------------------------------------------------------
log "Instalando os serviços do launchd"
AGENTS="$HOME/Library/LaunchAgents"
mkdir -p "$AGENTS"
CLOUDFLARED_BIN="$(command -v cloudflared)"

for plist in team.sopa.talk.api team.sopa.talk.backup; do
  sed -e "s|__SOPA_HOME__|$SOPA_HOME|g" \
      -e "s|__BREW_PREFIX__|$BREW_PREFIX|g" \
      -e "s|__CLOUDFLARED__|$CLOUDFLARED_BIN|g" \
      "$REPO_ROOT/deploy/macmini/launchd/$plist.plist" > "$AGENTS/$plist.plist"
  launchctl bootout "gui/$(id -u)/$plist" 2>/dev/null || true
  launchctl bootstrap "gui/$(id -u)" "$AGENTS/$plist.plist"
done

# O túnel só sobe depois do `cloudflared tunnel login` — ver README.
sed -e "s|__SOPA_HOME__|$SOPA_HOME|g" -e "s|__CLOUDFLARED__|$CLOUDFLARED_BIN|g" \
  "$REPO_ROOT/deploy/macmini/launchd/team.sopa.talk.tunnel.plist" \
  > "$AGENTS/team.sopa.talk.tunnel.plist"

# --- 7. Comportamento de servidor ------------------------------------------
log "Configurando a máquina para ficar sempre no ar"
sudo pmset -a sleep 0 disksleep 0 womp 1 autorestart 1 || warn "pmset falhou; ajuste manualmente."
sudo systemsetup -setrestartpowerfailure on >/dev/null 2>&1 || true

cat <<EOF

$(printf '\033[1m')Setup concluído.$(printf '\033[0m')

  Segredos    $ENV_FILE
  App         $SOPA_HOME/app
  Logs        $SOPA_HOME/logs/api.log
  Backups     $SOPA_HOME/backups

Próximos passos (na ordem):

  1. Ligue o login automático deste usuário:
     Ajustes do Sistema > Usuários e Grupos > Opções de início de sessão.
     Os serviços rodam como LaunchAgent — sem sessão aberta eles não sobem.

  2. Preencha em $ENV_FILE:
       Channels__AppSecret     app secret do app da Meta
       Accounts__AppBaseUrl    sua URL pública

  3. Publique:
       $REPO_ROOT/deploy/macmini/publish.sh

  4. Suba o túnel (ver deploy/macmini/README.md, seção "Túnel").

EOF
