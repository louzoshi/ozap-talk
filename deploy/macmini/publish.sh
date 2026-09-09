#!/usr/bin/env bash
#
# Build + publicação no Mac mini. Roda de dentro do repositório.
# Troca o app por um diretório novo e só então reinicia o serviço; se o build
# falhar, o que está no ar continua no ar.
#
# Uso:  ./deploy/macmini/publish.sh
#
set -euo pipefail

SOPA_HOME="${SOPA_HOME:-/usr/local/sopa-talk}"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
SERVICE="team.sopa.talk.api"
STAGING="$SOPA_HOME/app.new"
PREVIOUS="$SOPA_HOME/app.previous"

log() { printf '\n\033[1m==> %s\033[0m\n' "$*"; }
die() { printf '\033[31mx  %s\033[0m\n' "$*" >&2; exit 1; }

BREW_PREFIX="$(brew --prefix 2>/dev/null || echo /opt/homebrew)"
export PATH="$BREW_PREFIX/opt/node@22/bin:/usr/local/share/dotnet:$PATH"

case "$(uname -m)" in
  arm64) RID=osx-arm64 ;;
  x86_64) RID=osx-x64 ;;
  *) die "Arquitetura não suportada: $(uname -m)" ;;
esac

[ -d "$SOPA_HOME" ] || die "$SOPA_HOME não existe. Rode o setup.sh primeiro."

cd "$REPO_ROOT"

log "Frontend (SPA -> wwwroot)"
npm --prefix frontend ci
npm --prefix frontend run build

log "Backend ($RID)"
rm -rf "$STAGING"
dotnet publish src/SopaTalk.Api \
  -c Release \
  -r "$RID" \
  --no-self-contained \
  -o "$STAGING"

[ -x "$STAGING/SopaTalk.Api" ] || die "Publicação não gerou o executável."

log "Trocando a versão no ar"
rm -rf "$PREVIOUS"
[ -d "$SOPA_HOME/app" ] && mv "$SOPA_HOME/app" "$PREVIOUS"
mv "$STAGING" "$SOPA_HOME/app"

log "Reiniciando $SERVICE"
launchctl kickstart -k "gui/$(id -u)/$SERVICE"

log "Aguardando o health check"
for _ in $(seq 1 60); do
  if curl -fsS --max-time 2 http://127.0.0.1:5080/health >/dev/null 2>&1; then
    printf '\033[32mno ar\033[0m — http://127.0.0.1:5080/health\n\n'
    exit 0
  fi
  sleep 1
done

printf '\033[31mo app não respondeu em 60s.\033[0m Últimas linhas do log:\n\n'
tail -n 40 "$SOPA_HOME/logs/api.log" || true
printf '\nPara voltar à versão anterior:\n'
printf '  rm -rf %s/app && mv %s %s/app && launchctl kickstart -k gui/%s/%s\n' \
  "$SOPA_HOME" "$PREVIOUS" "$SOPA_HOME" "$(id -u)" "$SERVICE"
exit 1
