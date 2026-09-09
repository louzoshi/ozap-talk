#!/usr/bin/env bash
#
# Dump diário do Postgres + rotação. Chamado pelo launchd às 03:30.
# Instalado pelo setup.sh em __SOPA_HOME__/bin/backup.sh.
#
set -euo pipefail

SOPA_HOME="__SOPA_HOME__"
BACKUP_DIR="$SOPA_HOME/backups"
LOG_FILE="$SOPA_HOME/logs/api.log"
RETENTION_DAYS=14
MAX_LOG_MB=50

export PATH="__BREW_PREFIX__/opt/__PG_FORMULA__/bin:$PATH"

set -a
# shellcheck source=/dev/null
. "$SOPA_HOME/etc/sopa-talk.env"
set +a

mkdir -p "$BACKUP_DIR"
STAMP="$(date +%Y-%m-%d-%H%M)"
TARGET="$BACKUP_DIR/sopatalk-$STAMP.dump"

PGPASSWORD="$SOPA_DB_PASSWORD" pg_dump \
  --host=localhost --username=sopatalk --dbname=sopatalk \
  --format=custom --compress=9 --file="$TARGET"

echo "$(date '+%F %T') backup ok: $TARGET ($(du -h "$TARGET" | cut -f1))"

# Rotação dos dumps.
find "$BACKUP_DIR" -name 'sopatalk-*.dump' -type f -mtime "+$RETENTION_DAYS" -delete

# Rotação do log da API: um nível de histórico basta.
if [ -f "$LOG_FILE" ] && [ "$(du -m "$LOG_FILE" | cut -f1)" -gt "$MAX_LOG_MB" ]; then
  mv "$LOG_FILE" "$LOG_FILE.1"
  : > "$LOG_FILE"
  echo "$(date '+%F %T') log rotacionado"
fi
