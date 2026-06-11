# 7. Modelo físico do banco (PostgreSQL)

## Convenções

| Item | Convenção |
|---|---|
| Nomenclatura | `snake_case` |
| Chave primária | `id bigint GENERATED ALWAYS AS IDENTITY` |
| Datas/hora | `timestamptz` (UTC) para eventos; `date` para datas operacionais |
| Dinheiro | `numeric(12,2)` |
| Enums | `varchar` + `CHECK` |
| Polimorfismo | duas FKs anuláveis + `CHECK` de exclusividade |
| Saldo de estoque | tabela derivada, mantida por **trigger** |
| Disponível | **view** (`saldo − reservado`), nunca coluna materializada |
| Soft delete | `ativo boolean` em cadastros |

> Legenda: **PK** primária · **FK** estrangeira · **UQ** único · **CK** check · **IX** índice.

---

## Módulo Identidade & Acesso

### `usuarios`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| nome | varchar(120) | not null |
| email | varchar(160) | not null, UQ |
| senha_hash | varchar(255) | not null |
| ativo | boolean | not null, default true |
| criado_em | timestamptz | not null, default now() |

### `papeis`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| nome | varchar(40) | not null, UQ (`Admin`, `Operacao`, `Entrega`) |

### `usuarios_papeis`
| Campo | Tipo | Regras |
|---|---|---|
| usuario_id | bigint | PK, FK→usuarios (on delete cascade) |
| papel_id | bigint | PK, FK→papeis (on delete cascade) |

---

## Módulo Clientes & Pets

### `clientes`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| nome | varchar(160) | not null |
| telefone | varchar(40) | |
| email | varchar(160) | |
| endereco | varchar(255) | |
| valor_plano | numeric(12,2) | CK ≥ 0 |
| situacao | varchar(20) | not null, default `ativo`, CK ∈ {ativo, inativo, inadimplente} |
| obs_financeira | text | |
| ativo | boolean | not null, default true |
| criado_em | timestamptz | not null, default now() |
| atualizado_em | timestamptz | not null, default now() |

### `pets`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| cliente_id | bigint | not null, FK→clientes (on delete restrict) |
| nome | varchar(120) | not null |
| porte | varchar(20) | CK ∈ {pequeno, medio, grande} |
| peso_kg | numeric(6,2) | CK > 0 |
| frequencia | varchar(20) | not null, CK ∈ {semanal, quinzenal, mensal, personalizada} |
| frequencia_dias | int | CK > 0 |
| proxima_data_entrega | date | |
| ativo | boolean | not null, default true |
| criado_em | timestamptz | default now() |
| atualizado_em | timestamptz | default now() |
- **CK:** `frequencia <> 'personalizada' OR frequencia_dias IS NOT NULL`
- **IX:** `(proxima_data_entrega)`, `(cliente_id)`

### `planos_alimentares`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| pet_id | bigint | not null, FK→pets (on delete cascade) |
| vigente | boolean | not null, default true |
| criado_em | timestamptz | default now() |
- **UQ parcial:** `(pet_id) WHERE vigente = true` → um único plano vigente por pet.

### `itens_plano`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| plano_id | bigint | not null, FK→planos_alimentares (on delete cascade) |
| produto_id | bigint | FK→produtos (nullable) |
| receita_personalizada_id | bigint | FK→receitas_personalizadas (nullable) |
| quantidade_pacotes | int | not null, CK > 0 |
- **CK exclusividade:** `(produto_id IS NULL) <> (receita_personalizada_id IS NULL)`
- **IX:** `(plano_id)`

---

## Módulo Catálogo / Receitas

> **Atualizado pelo módulo de Ingredientes** (ver [doc 13](13-modulo-ingredientes.md)).
> O ingrediente passou a conter categoria, tipo/coeficiente de conversão e custo
> atual; a antiga `fatores_correcao` foi **substituída** pelo coeficiente de
> conversão no próprio ingrediente, e o custo passou a ter histórico dedicado.

### `categorias_ingredientes`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| nome | varchar(60) | not null, UQ |
| ativo | boolean | default true |
> Seed: Proteína, Carboidrato, Vegetal, Óleo, Suplemento, Tempero.

### `ingredientes`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| nome | varchar(120) | not null, UQ |
| categoria_id | bigint | not null, FK→categorias_ingredientes |
| tipo_conversao | varchar(15) | not null, CK ∈ {perda, ganho, sem_conversao} |
| coeficiente_conversao | numeric(8,4) | not null, CK > 0 (rendimento = cozido ÷ cru) |
| custo_atual_kg | numeric(12,2) | not null, CK ≥ 0 |
| ativo | boolean | default true |
| criado_em / atualizado_em | timestamptz | auditoria |
- **CK:** `tipo_conversao <> 'sem_conversao' OR coeficiente_conversao = 1`

### `historico_custos_ingredientes`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| ingrediente_id | bigint | not null, FK→ingredientes |
| data_alteracao | timestamptz | not null, default now() |
| valor_anterior | numeric(12,2) | not null |
| valor_novo | numeric(12,2) | not null |
| usuario_id | bigint | FK→usuarios (nullable) |
- **IX:** `(ingrediente_id, data_alteracao DESC)`. Registro criado automaticamente ao alterar `custo_atual_kg`.

### `receitas_casa`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| codigo | varchar(30) | not null, UQ |
| nome | varchar(120) | not null |
| ativa | boolean | default true |
| criado_em | timestamptz | default now() |

### `produtos`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| receita_casa_id | bigint | not null, FK→receitas_casa (on delete restrict) |
| nome | varchar(120) | not null (ex.: "Frango 250g") |
| gramatura_g | int | not null, CK > 0 |
| estoque_minimo | int | not null, default 0, CK ≥ 0 |
| lote_producao | int | CK > 0 (nullable) |
| ativo | boolean | default true |
- **UQ:** `(receita_casa_id, gramatura_g)`
- **IX:** `(receita_casa_id)`

### `receitas_personalizadas`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| codigo | varchar(30) | not null, UQ (ex.: VET-001) |
| nome | varchar(120) | not null |
| pet_id | bigint | not null, FK→pets (on delete restrict) |
| status | varchar(25) | not null, default `produzir`, CK ∈ {produzir, pronta_para_entrega} |
| ciclo_ref | date | nullable |
| ativa | boolean | default true |
| atualizado_em | timestamptz | default now() |
- **IX:** `(pet_id)`, `(status, ciclo_ref)`

### `itens_receita_casa`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| receita_casa_id | bigint | not null, FK→receitas_casa (on delete cascade) |
| ingrediente_id | bigint | not null, FK→ingredientes |
| gramatura_g | numeric(10,3) | not null, CK > 0 |
- **UQ:** `(receita_casa_id, ingrediente_id)`

### `itens_receita_personalizada`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| receita_personalizada_id | bigint | not null, FK→receitas_personalizadas (on delete cascade) |
| ingrediente_id | bigint | not null, FK→ingredientes |
| gramatura_g | numeric(10,3) | not null, CK > 0 |
- **UQ:** `(receita_personalizada_id, ingrediente_id)`

---

## Módulo Estoque

### `saldo_estoque` (derivado — mantido por trigger)
| Campo | Tipo | Regras |
|---|---|---|
| produto_id | bigint | PK, FK→produtos |
| saldo_fisico | int | not null, default 0, CK ≥ 0 |
| atualizado_em | timestamptz | default now() |

### `movimentos_estoque` (livro-razão — fonte da verdade)
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| produto_id | bigint | not null, FK→produtos |
| tipo | varchar(10) | not null, CK ∈ {entrada, saida, ajuste} |
| quantidade | int | not null, CK ≠ 0 |
| origem | varchar(15) | not null, CK ∈ {producao, entrega, manual} |
| referencia_id | bigint | nullable (id da ordem/entrega) |
| usuario_id | bigint | FK→usuarios (nullable) |
| criado_em | timestamptz | default now() |
- **IX:** `(produto_id, criado_em)`

### `reservas_estoque`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| produto_id | bigint | not null, FK→produtos |
| entrega_id | bigint | not null, FK→entregas (on delete cascade) |
| quantidade | int | not null, CK > 0 |
| status | varchar(12) | not null, default `ativa`, CK ∈ {ativa, liberada, consumida} |
| criado_em | timestamptz | default now() |
- **UQ:** `(entrega_id, produto_id)`
- **IX:** `(produto_id, status)`, `(entrega_id)`

---

## Módulo Produção

### `ordens_producao`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| tipo | varchar(15) | not null, CK ∈ {casa, personalizada} |
| data_planejada | date | not null |
| status | varchar(15) | not null, default `planejada`, CK ∈ {planejada, em_producao, concluida, cancelada} |
| criado_em | timestamptz | default now() |
- **IX:** `(data_planejada, status)`

### `itens_ordem_producao`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| ordem_id | bigint | not null, FK→ordens_producao (on delete cascade) |
| produto_id | bigint | FK→produtos (nullable) |
| receita_personalizada_id | bigint | FK→receitas_personalizadas (nullable) |
| quantidade | int | not null, CK > 0 |
- **CK exclusividade:** produto XOR personalizada

---

## Módulo Entregas

### `entregas`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| pet_id | bigint | not null, FK→pets (on delete restrict) |
| data_prevista | date | not null |
| data_confirmada | date | nullable |
| status | varchar(15) | not null, default `agendada`, CK ∈ {agendada, confirmada, cancelada} |
| tem_pendencia | boolean | not null, default false |
| motivo_cancelamento | varchar(255) | nullable |
| criado_em | timestamptz | default now() |
| atualizado_em | timestamptz | default now() |
- **UQ parcial:** `(pet_id, data_prevista) WHERE status <> 'cancelada'`
- **IX:** `(data_prevista, status)`, `(pet_id, status)`

### `itens_entrega` (snapshot imutável)
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| entrega_id | bigint | not null, FK→entregas (on delete cascade) |
| tipo | varchar(15) | not null, CK ∈ {casa, personalizada} |
| produto_id | bigint | FK→produtos (nullable, on delete set null) |
| receita_personalizada_id | bigint | FK→receitas_personalizadas (nullable, on delete set null) |
| descricao_snapshot | varchar(160) | not null |
| quantidade_pacotes | int | not null, CK > 0 |

---

## Módulo Calendário & Configuração

### `calendario_operacional`
| Campo | Tipo | Regras |
|---|---|---|
| data | date | PK |
| eh_dia_producao | boolean | not null, default true |
| eh_dia_entrega | boolean | not null, default true |
| descricao | varchar(120) | |

### `configuracoes`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| horizonte_producao_dias | int | not null, default 14, CK > 0 |
| horizonte_reserva_dias | int | not null, default 30, CK ≥ horizonte_producao_dias |
| usa_estoque_minimo | boolean | not null, default true |
| politica_alocacao | varchar(20) | not null, default `fifo_urgencia` |
| atualizado_em | timestamptz | default now() |

---

## Módulo Eventos

### `eventos_dominio`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| tipo | varchar(40) | not null |
| entidade | varchar(40) | not null |
| entidade_id | bigint | not null |
| dados | jsonb | |
| usuario_id | bigint | FK→usuarios (nullable) |
| criado_em | timestamptz | default now() |
- **IX:** `(entidade, entidade_id)`, `(tipo, criado_em)`, GIN `(dados)` (se consultar payload)

---

## Índices recomendados (consolidado)

| Tabela | Índice | Serve a |
|---|---|---|
| pets | `(proxima_data_entrega)` | gerar próximas entregas |
| pets | `(cliente_id)` | listar pets do cliente |
| entregas | `(data_prevista, status)` | janela de planejamento/entrega |
| entregas | `(pet_id, status)` | histórico do pet |
| reservas_estoque | `(produto_id, status)` | cálculo de disponível |
| reservas_estoque | `(entrega_id)` | liberar reservas na confirmação |
| movimentos_estoque | `(produto_id, criado_em)` | extrato/saldo |
| receitas_personalizadas | `(status, ciclo_ref)` | lista "produzir/pronta" |
| receitas_personalizadas | `(pet_id)` | personalizadas do pet |
| produtos | `(receita_casa_id)` | produtos da receita |
| ordens_producao | `(data_planejada, status)` | agenda de produção |
| eventos_dominio | `(entidade, entidade_id)` | timeline |
| planos_alimentares | UQ parcial `(pet_id) WHERE vigente` | plano único vigente |
| entregas | UQ parcial `(pet_id, data_prevista) WHERE status<>cancelada` | não duplicar entrega |

---

## Regras de integridade (resumo)

1. Plano único vigente por pet (UQ parcial).
2. Item de plano: produto XOR personalizada (CK).
3. Saldo nunca negativo (CK + trigger rejeita movimento inválido).
4. Reserva única por `(entrega, produto)` (UQ); soma de reservas ativas ≤ saldo (validado na aplicação/trigger).
5. Receita personalizada sempre tem dono (`pet_id NOT NULL`).
6. Frequência personalizada exige `frequencia_dias` (CK).
7. Entregas confirmadas imutáveis (regra de aplicação + snapshot).
8. `on delete restrict` em cliente→pet, produto→ordem, receita→produto; usar `ativo=false`.
9. `horizonte_reserva ≥ horizonte_producao` (CK).

---

## Fluxos de atualização automática

| Gatilho | Mecanismo | Ação |
|---|---|---|
| Insert em `movimentos_estoque` | **Trigger DB** | recalcula `saldo_estoque`; rejeita se resultaria < 0 |
| Update em tabela com `atualizado_em` | **Trigger DB** `set_updated_at` | mantém timestamp |
| Confirmação de entrega | **Transação na aplicação (EF)** | baixa estoque + consome reservas; personalizadas → produzir + ciclo; recalcula `proxima_data_entrega`; grava evento |
| Cancelamento de entrega | **Aplicação** | reservas ativas → liberada; sem recálculo |
| Job diário | **Aplicação (worker)** | materializa entregas; roda planejamento/reservas |
| "Disponível" | **View** `vw_estoque_disponivel` | `saldo_fisico − Σ reservas ativas` |

> Invariantes de dado puro (saldo, timestamps) ficam em **trigger**; regras de
> processo (confirmação, planejamento) ficam na **aplicação** (testáveis,
> transacionais, geram eventos). A mesma regra não é duplicada nos dois lugares.
