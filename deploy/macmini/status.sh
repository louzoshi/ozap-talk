#!/usr/bin/env bash
#
# Retrato rápido do servidor. Instalado pelo setup.sh em __OZAP_HOME__/bin/status.sh.
#
set -uo pipefail

OZAP_HOME="__OZAP_HOME__"
export PATH="__BREW_PREFIX__/bin:__BREW_PREFIX__/opt/__PG_FORMULA__/bin:$PATH"

title() { printf '\n\033[1m%s\033[0m\n' "$1"; }

title "Serviços"
for svc in com.ozaptalk.api com.ozaptalk.tunnel com.ozaptalk.backup; do
  if launchctl print "gui/$(id -u)/$svc" >/dev/null 2>&1; then
    pid="$(launchctl print "gui/$(id -u)/$svc" | awk '/^\tpid = /{print $3}')"
    printf '  %-28s %s\n' "$svc" "${pid:+no ar (pid $pid)}${pid:-carregado, parado}"
  else
    printf '  %-28s \033[33mnão carregado\033[0m\n' "$svc"
  fi
done
printf '  %-28s %s\n' "postgresql@17" "$(pg_isready -h localhost 2>&1 | tail -1)"

title "API"
curl -fsS --max-time 3 http://127.0.0.1:5080/health && echo || echo "  sem resposta em 127.0.0.1:5080"

title "Banco"
set -a
# shellcheck source=/dev/null
. "$OZAP_HOME/etc/ozap-talk.env" 2>/dev/null
set +a
PGPASSWORD="${OZAP_DB_PASSWORD:-}" psql -h localhost -U ozaptalk -d ozaptalk -tAc \
  "select '  tamanho: ' || pg_size_pretty(pg_database_size('ozaptalk'))" 2>/dev/null \
  || echo "  não consegui consultar"

title "Backups (5 mais recentes)"
ls -lht "$OZAP_HOME/backups" 2>/dev/null | head -6 | tail -5 || echo "  nenhum ainda"

title "Log (20 últimas linhas)"
tail -n 20 "$OZAP_HOME/logs/api.log" 2>/dev/null || echo "  vazio"
echo
