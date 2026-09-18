# Система партнерских отчислений

Три независимых HTTP-сервиса на .NET 10, у каждого свой Postgres. Cервисы оьщаются по HTTP, исходящие вызовы фиксируются в Outbox и дожимаются retry. Общей БД и распределённых транзакций нет — согласованность через локальный commit и идемпотентные ключи.

Локально всё поднимает `docker compose`: сначала все БД, затем миграция каждого API (`pre_start`), после неё — сами сервисы.

## Взаимодействие

```mermaid
flowchart LR
  client[Клиент]

  users[Users.Api]
  commissions[Commissions.Api]
  wallets[Wallets.Api]

  pgU[(Postgres Users)]
  pgC[(Postgres Commissions)]
  pgW[(Postgres Wallets)]

  client -->|пользователи и дерево| users
  client -->|события и начисления| commissions
  client -->|кошелёк и выплаты| wallets

  commissions -->|цепочка предков| users
  wallets -->|невыплаченные и ACK| commissions

  users --> pgU
  commissions --> pgC
  wallets --> pgW
```

## Запуск

```bash
docker compose up --build
```
