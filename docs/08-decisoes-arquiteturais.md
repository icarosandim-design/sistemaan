# 8. Decisões arquiteturais

## Estilo de arquitetura: Monólito Modular

Para ~150 clientes e equipe pequena, microsserviços seriam complexidade sem
retorno. Adota-se um **monólito modular**: módulos com fronteiras claras dentro
de uma única solução .NET. Dá a velocidade de um monólito e organização para,
*se* algum dia necessário, extrair módulos.

```
┌─────────────────────────────────────────────┐
│  Angular SPA (navegador / tablet da cozinha) │
└───────────────────┬─────────────────────────┘
                   │ HTTPS / REST + JWT
┌───────────────────▼─────────────────────────┐
│  ASP.NET Core Web API                        │
│   Apresentação (Controllers/DTOs)            │
│   Aplicação (casos de uso, serviços)         │
│   Domínio (entidades, regras, estados)       │
│   Infraestrutura (EF Core, repositórios, JWT)│
└───────────────────┬─────────────────────────┘
                   │
          ┌─────────▼─────────┐
          │ PostgreSQL (EF Core)│
          └────────────────────┘
```

## Decisões transversais

| Decisão | Escolha | Justificativa |
|---|---|---|
| Camadas | 4 camadas clássicas | Suficiente para o porte; sem CQRS/Event Sourcing. |
| Autenticação | JWT + refresh token | Equipe interna; RBAC enxuto (Admin/Operação/Entrega). |
| ORM | EF Core (code-first + migrations) | Versionamento de schema, produtividade. |
| Eventos | Tabela `eventos_dominio` (timeline) | A operação é orientada a "o que aconteceu e quando". |
| Jobs | Worker/agendador interno (job diário) | Materialização de entregas e planejamento. |
| Docker | compose para dev (api+db+web); imagens separadas em produção | Ambiente reprodutível. |
| Fuso/datas | `timestamptz` (UTC) no banco; conversão na borda | Datas de entrega são sensíveis a fuso. |

## Decisões de modelagem de dados

| Decisão | Escolha | Justificativa |
|---|---|---|
| Nomenclatura | `snake_case` | Idiomático no PostgreSQL. |
| Chave primária | `bigint identity` | Mais leve que UUID; volume não exige UUID. |
| Receita personalizada | pertence a **1 pet** | Gramatura por ingrediente é específica do pet; sem compartilhamento. |
| Receita da Casa × Produto | separadas (1:N) | Uma receita pode ter várias embalagens; estoque no Produto. |
| Referência polimórfica | duas FKs anuláveis + CK exclusiva | Mantém integridade referencial real (vs. `(tipo,id)` solto). |
| Ficha técnica | duas tabelas (casa/personalizada) | FK real para cada tipo. |
| Saldo de estoque | derivado via **trigger** dos movimentos | Garante invariante `saldo = Σ movimentos`. |
| Disponível | **view** (`saldo − reservado`) | Evita dessincronização; leitura barata. |
| Reserva | entidade explícita | "Reservado para futuro" e "disponível" tornam-se consultas diretas. |
| Snapshot de entrega | `itens_entrega` imutável | Mudança de plano não reescreve histórico. |
| Estoque | só quantidade (sem lote/validade) | Escopo da Fase 1; giro rápido. |
| Financeiro | só cadastral | Escopo travado; contas a receber é fase futura. |

## Horizontes de planejamento

Dois horizontes distintos (configuráveis em `configuracoes`):

| Horizonte | Default | Racional |
|---|---|---|
| **Produção** | 14 dias | Comida natural é perecível; não produzir cedo demais. |
| **Reserva/visibilidade** | 30 dias | Ver compromissos do mês sem comprometer físico cedo. |

Restrição: `horizonte_reserva ≥ horizonte_producao`.

## Onde cada regra vive

- **Trigger no banco:** invariantes de dado puro — saldo de estoque, `atualizado_em`.
- **Aplicação (transacional):** regras de processo — confirmação de entrega,
  planejamento, materialização de entregas, geração de eventos.
- A mesma regra **não** é duplicada nos dois lugares.

## Política de alocação de reserva

`fifo_urgencia`: quando o disponível não cobre todas as entregas, aloca primeiro
para a entrega com **data mais próxima**. Configurável.
