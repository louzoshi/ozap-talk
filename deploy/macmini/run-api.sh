#!/usr/bin/env bash
#
# Wrapper que o launchd executa: carrega os segredos e entrega o processo à API.
# Instalado pelo setup.sh em __OZAP_HOME__/bin/run-api.sh.
#
set -euo pipefail

OZAP_HOME="__OZAP_HOME__"
ENV_FILE="$OZAP_HOME/etc/ozap-talk.env"

export PATH="__BREW_PREFIX__/bin:__BREW_PREFIX__/opt/__PG_FORMULA__/bin:/usr/local/share/dotnet:/usr/bin:/bin:/usr/sbin:/sbin"

[ -f "$ENV_FILE" ] || { echo "ausente: $ENV_FILE" >&2; exit 78; }

set -a
# shellcheck source=/dev/null
. "$ENV_FILE"
set +a

# O executável publicado procura o runtime pelo DOTNET_ROOT.
if [ -z "${DOTNET_ROOT:-}" ]; then
  for candidate in /usr/local/share/dotnet __BREW_PREFIX__/opt/dotnet/libexec __BREW_PREFIX__/share/dotnet; do
    if [ -x "$candidate/dotnet" ]; then
      export DOTNET_ROOT="$candidate"
      break
    fi
  done
fi

# Espera o Postgres: no boot o launchd pode chegar antes dele.
for _ in $(seq 1 60); do
  pg_isready -q -h localhost && break
  sleep 2
done

cd "$OZAP_HOME/app"
exec ./OzapTalk.Api
