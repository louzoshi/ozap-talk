# infra

## Local

```bash
docker compose -f infra/docker-compose.yml up -d
docker compose -f infra/docker-compose.yml down          # para
docker compose -f infra/docker-compose.yml down -v       # para e apaga os volumes
```

Sobe:

- **postgres** — `localhost:5432`, banco/usuário/senha `ozaptalk`
- **valkey** — `localhost:6379` (drop-in do Redis, BSD; usado quando houver 2+ instâncias)

As connection strings padrão dos `appsettings.json` já apontam para esses valores.

## Produção

Não é este arquivo. Ver `docs/custos.md` para a topologia de referência
(Hetzner Cloud: 2× app server + Postgres gerenciado + LB). IaC entra em R0/R5.
