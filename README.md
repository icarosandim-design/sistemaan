# SistemaAN — Gestão de Alimentação Natural para Cães

Sistema de gestão operacional (substitui planilhas) para uma empresa de
alimentação natural canina. A **documentação oficial do projeto** está em
[`docs/`](docs/README.md) — ela é a fonte de verdade do negócio e da modelagem.

## Stack

| Camada | Tecnologia |
|---|---|
| Frontend | Angular 19 |
| Backend | ASP.NET Core (.NET 9) — Clean Architecture |
| Banco | PostgreSQL 16 |
| ORM | Entity Framework Core 9 |
| Auth | JWT (Bearer) |
| Logs | Serilog |
| Docs API | Swagger / OpenAPI |
| Ambiente | Docker Compose |

## Estrutura do repositório

```
sistemaan/
├── docs/         # Documentação oficial (negócio + modelagem)
├── backend/      # Solução .NET (Domain, Application, Infrastructure, Api)
├── frontend/     # Aplicação Angular
├── docker-compose.yml
└── .env.example
```

## Como executar (ambiente de desenvolvimento)

Pré-requisitos: Docker + Docker Compose.

```bash
cp .env.example .env        # ajuste os valores (em especial JWT_SECRET_KEY)
docker compose up --build
```

Serviços:

| Serviço | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Health check | http://localhost:8080/health |
| Frontend | http://localhost:4200 |
| PostgreSQL | localhost:5432 |

## Desenvolvimento local (sem Docker)

Backend:
```bash
cd backend
dotnet restore
dotnet run --project src/SistemaAN.Api
```

Frontend:
```bash
cd frontend
npm install
npm start
```

## Estado atual

Esta etapa entrega **apenas a fundação técnica**: estrutura em camadas,
configuração de banco/EF Core, JWT (infraestrutura), logs, tratamento global de
erros, documentação da API e Docker. **Não há** CRUDs, regras operacionais nem
telas de negócio — esses virão nas próximas etapas.
