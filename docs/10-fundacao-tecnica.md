# 10. Fundação técnica (infraestrutura do projeto)

Esta etapa entrega **apenas a fundação técnica** — sem CRUDs, regras
operacionais ou telas de negócio.

## Estrutura de diretórios

```
sistemaan/
├── docs/                          # Documentação oficial (fonte de verdade)
├── backend/
│   ├── SistemaAN.sln
│   ├── Directory.Build.props       # TargetFramework, Nullable, etc. (compartilhado)
│   ├── Dockerfile                  # build multi-stage (sdk → aspnet)
│   └── src/
│       ├── SistemaAN.Domain/        # entidades base, eventos de domínio (sem regras)
│       ├── SistemaAN.Application/   # interfaces, exceções, DI (sem casos de uso)
│       ├── SistemaAN.Infrastructure/# EF Core, PostgreSQL, JWT, interceptors
│       └── SistemaAN.Api/           # composition root, logs, erros, Swagger, health
├── frontend/                       # Angular 19 (standalone) — sem telas de negócio
├── docker-compose.yml              # db + api + web
└── .env.example
```

## Camadas do backend (Clean Architecture)

```
Api ──► Application ──► Domain
 │           ▲
 └──► Infrastructure ──┘
```

| Camada | Conhece | Responsabilidade |
|---|---|---|
| **Domain** | nada | Entidades base (`Entity`, `AuditableEntity`, `AggregateRoot`), `IDomainEvent`. Núcleo puro. |
| **Application** | Domain | Contratos (`IApplicationDbContext`, `IDateTimeProvider`), exceções de aplicação, registro de DI. |
| **Infrastructure** | Application, Domain | EF Core + Npgsql, `ApplicationDbContext`, interceptor de auditoria, base de JWT, provedor de tempo. |
| **Api** | Application, Infrastructure | Composition root, pipeline HTTP, Serilog, tratamento global de erros, Swagger, health checks, CORS. |

A regra de dependência aponta sempre **para dentro**: Domain não referencia
ninguém; Infrastructure e Api dependem das abstrações da Application.

## Componentes da fundação

| Requisito | Implementação |
|---|---|
| Banco | PostgreSQL 16 (container) |
| ORM | EF Core 9 + Npgsql + `UseSnakeCaseNamingConvention` (alinha com o doc 07) |
| Migrations | `ApplicationDbContextFactory` (design-time) + tabela `__ef_migrations_history` |
| Auditoria | `AuditableEntityInterceptor` preenche `created_at`/`updated_at` |
| JWT | `AddJwtBearer` configurado via `JwtSettings`; **emissão/login virão na próxima etapa** |
| Logs | Serilog (console), configurável por `appsettings` |
| Erros | `IExceptionHandler` global → `ProblemDetails` (RFC 7807) |
| Docs API | Swagger/OpenAPI com esquema de segurança Bearer |
| Health | `/health` com verificação de conectividade ao banco |
| CORS | política para o frontend (origens via configuração) |
| Config | `appsettings` + variáveis de ambiente (sobrescrevem) |

## Endpoints técnicos (não-negócio)

- `GET /health` — saúde da aplicação e do banco.
- `GET /api/meta` — nome, versão e ambiente da API (valida a fundação).
- `GET /swagger` — documentação interativa (apenas em Development).

## Pontos de extensão (próximas etapas)

- **Application:** registrar casos de uso/handlers e validadores em `AddApplication`.
- **Infrastructure:** declarar `DbSet` e `IEntityTypeConfiguration` por módulo;
  adicionar migrations refletindo o [modelo físico](07-modelo-fisico-banco.md).
- **Api:** controllers por módulo; políticas de autorização (próxima etapa: auth e permissões).
- **Frontend:** rotas, módulos de feature e serviços de API.

## Central Operacional — contrato de leitura (sem backend)

A Central é uma **camada fina de leitura/agregação** (sem regra de negócio). Para
permitir integração incremental futura sem retrabalho, ela consome um contrato:

- `CentralResumo` (`features/central/central.model.ts`) — tipo do que a tela exibe.
- `CentralService` (`features/central/central.service.ts`) — hoje devolve **mock**;
  **futuramente** passará a consumir **`GET /api/central/resumo`** sem alterar a tela.
- A tela trata **loading / erro / vazio** e cada card é **resiliente a dados ausentes**.

> Quando os módulos (Clientes, Pets, Produção, Estoque, Entregas, Financeiro)
> existirem, um endpoint agregador (BFF/read-model) preencherá `CentralResumo`
> campo a campo. A Central **não** chama vários endpoints nem contém lógica de negócio.

## Observação sobre versões

A fundação foi escrita para **.NET 9 / EF Core 9 / Angular 19 / PostgreSQL 16**.
O build completo requer o SDK do .NET e o ambiente Node/Angular (ou Docker).
