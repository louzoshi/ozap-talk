#!/usr/bin/env bash
#
# Dump diário do Postgres + rotação. Chamado pelo launchd às 03:30.
# Instalado pelo setup.sh em __OZAP_HOME__/bin/backup.sh.
#
set -euo pipefail

OZAP_HOME="__OZAP_HOME__"
BACKUP_DIR="$OZAP_HOME/backups"
LOG_FILE="$OZAP_HOME/logs/api.log"
RETENTION_DAYS=14
MAX_LOG_MB=50

export PATH="__BREW_PREFIX__/opt/__PG_FORMULA__/bin:$PATH"

set -a
# shellcheck source=/dev/null
. "$OZAP_HOME/etc/ozap-talk.env"
set +a

mkdir -p "$BACKUP_DIR"
STAMP="$(date +%Y-%m-%d-%H%M)"
TARGET="$BACKUP_DIR/ozaptalk-$STAMP.dump"

PGPASSWORD="$OZAP_DB_PASSWORD" pg_dump \
  --host=localhost --username=ozaptalk --dbname=ozaptalk \
  --format=custom --compress=9 --file="$TARGET"

echo "$(date '+%F %T') backup ok: $TARGET ($(du -h "$TARGET" | cut -f1))"

# Rotação dos dumps.
find "$BACKUP_DIR" -name 'ozaptalk-*.dump' -type f -mtime "+$RETENTION_DAYS" -delete

# Rotação do log da API: um nível de histórico basta.
if [ -f "$LOG_FILE" ] && [ "$(du -m "$LOG_FILE" | cut -f1)" -gt "$MAX_LOG_MB" ]; then
  mv "$LOG_FILE" "$LOG_FILE.1"
  : > "$LOG_FILE"
  echo "$(date '+%F %T') log rotacionado"
fi
