# 20 — Módulo de Estoque

> Status: **modelagem em validação**. Nenhum código/migration/endpoint/tela ainda.
> O Estoque é a **fonte de verdade do saldo físico** e substituirá, no futuro, os
> mocks da tela de Entregas (estoque das Receitas da Casa e prontidão das Personalizadas).

## 1. Princípios da modelagem

1. **Saldo nunca é editado direto.** Todo saldo é resultado de **movimentações**
   (livro-razão append-only). Editar saldo = registrar movimentação.
2. **Rastreabilidade por lote.** Validade, custo de compra e PEPS/FIFO vivem no **lote**.
3. **Não duplicar Ingredientes.** Item de estoque alimentar **referencia** o Ingrediente
   existente (não recria nome/categoria/coeficiente).
4. **Três controles distintos, infraestrutura comum onde fizer sentido:**
   - **Insumos** e **Produto Acabado da Casa** → têm *saldo corrente* → usam Item + Lote + Movimentação.
   - **Personalizada** → **não é estoque geral**; é **prontidão por entrega/pet** (estado de preparo).
5. Convenções do projeto: `AuditableEntity`, enums `HasConversion<string>`, snake_case,
   agregado interno Cascade / FKs externas Restrict, nomes de FK/índice curtos (<63).

---

## 2. Os três controles (visão de topo)

| Controle | Identidade | Unidade típica | Lote/Validade | Saldo | Origem entrada | Origem saída |
|---|---|---|---|---|---|---|
| **1. Insumos** | `IngredienteId?` (alimentar) **ou** item livre (embalagem/etiqueta/limpeza) | kg, g, un, L… | Sim | Geral | Compra | Produção, perda, descarte, ajuste |
| **2. Produto Acabado — Casa** | `ReceitaId` + `TamanhoPacoteId` | pacote | Sim (lote de produção) | **Geral** (qualquer cliente da receita) | Produção (envase) | Baixa por entrega, perda, ajuste |
| **3. Personalizada** | `EntregaItem` (pet + entrega específica) | pacote | — | **Reservado** à entrega | Produção marca pronta | Entrega |

> **Regra de ouro:** Personalizada **nunca** entra no saldo geral. Ela pertence a um
> pet/entrega; o que controlamos é o **status de preparo** (`NaoPronta / ParcialmentePronta / Pronta`).

---

## 3. Entidades

### 3.1 `ItemEstoque` (mestre — itens com saldo corrente)
Cobre **Insumos** e **Produto Acabado da Casa** (diferenciados por `Tipo`).

- `Id`
- `Tipo` (enum `TipoItemEstoque`: `Insumo`, `ProdutoAcabadoCasa`)
- `Nome` (ex.: "Frango peito cru", "Pacote 500g", "Etiqueta padrão")
- `Categoria` (enum `CategoriaEstoque` — §4.2)
- `UnidadeMedida` (enum `UnidadeMedida` — §4.3)
- **Vínculos (mutuamente exclusivos por Tipo):**
  - Insumo alimentar: `IngredienteId?` (FK Restrict, **opcional**)
  - Produto acabado casa: `ReceitaId?` + `TamanhoPacoteId?` (FK Restrict)
- `QuantidadeAtual` (decimal — **cache** do saldo, projeção das movimentações)
- `QuantidadeMinima` (decimal — estoque mínimo)
- `CustoMedio` (decimal — custo médio ponderado móvel, §9)
- `FornecedorPrincipalId?` (FK Restrict, opcional)
- `LocalArmazenamento?` (texto curto; pode virar cadastro depois)
- `ControlaValidade` (bool — embalagem/etiqueta normalmente não)
- `Ativo` (bool — soft delete)
- `Observacoes?`
- Auditoria (`CreatedAt`/`UpdatedAt`)
- `RowVersion` (concorrência otimista — §13)

**Invariantes:**
- `Insumo` ⇒ `ReceitaId`/`TamanhoPacoteId` nulos. `IngredienteId` opcional.
- `ProdutoAcabadoCasa` ⇒ `ReceitaId` + `TamanhoPacoteId` obrigatórios, `UnidadeMedida = Pacote`, `IngredienteId` nulo.
- Unicidade: produto acabado é único por `(ReceitaId, TamanhoPacoteId)`.

### 3.2 `LoteEstoque`
- `Id`, `ItemEstoqueId` (FK cascade dentro do agregado de saldo? **Restrict** — ver §13)
- `Codigo` (número/código do lote; pode ser gerado se não informado)
- `DataEntrada` (date)
- `Validade?` (date)
- `QuantidadeInicial` (decimal)
- `QuantidadeAtual` (decimal — cache)
- `CustoUnitario` (decimal — custo de **aquisição/produção** deste lote)
- `FornecedorId?` (FK Restrict)
- `Status` (enum `StatusLote`: `Ativo`, `Esgotado`, `Vencido`, `Bloqueado`)
- `OrigemTipo` (`Compra` | `Producao`) + `OrigemId?` (EntradaEstoque/OrdemProducao)
- Auditoria

**Regras:** validade por lote; PEPS/FIFO consome o lote mais antigo **não vencido** primeiro;
lote zera ⇒ `Esgotado`; passou da validade ⇒ `Vencido` (bloqueia consumo).

### 3.3 `MovimentacaoEstoque` (livro-razão, append-only)
**Fonte de verdade do saldo.** Nunca se edita/deleta.

- `Id`, `ItemEstoqueId` (FK Restrict), `LoteEstoqueId?` (FK Restrict)
- `Tipo` (enum `TipoMovimentacao` — §4.4)
- `Quantidade` (decimal — **sempre positiva**; o sinal vem do `Tipo`)
- `Sentido` (`Entrada` | `Saida`) — derivável do tipo, persistido p/ relatório
- `SaldoAnteriorItem`, `SaldoPosteriorItem` (decimal — foto no momento)
- `CustoUnitario`, `ValorTotal` (decimal)
- `Motivo?` (texto) e `MotivoCodigo?` (enum `MotivoSaida`, quando saída)
- `Usuario` (string)
- `DataHora` (timestamp)
- `Observacao?`
- **Referências opcionais de origem:** `EntradaEstoqueId?`, `OrdemProducaoId?`, `EntregaId?`, `AjusteId?`
  (todas **sem FK rígida agora**; viram FK quando o módulo correspondente existir — padrão usado em Entregas com `EntregadorId`).

### 3.4 `EntradaEstoque` (cabeçalho de compra) — *recomendado*
Agrupa uma compra (uma nota/fornecedor/data) com vários itens; cada item gera **lote + movimentação**.

- `Id`, `FornecedorId?`, `NumeroNota?`, `DataCompra`, `DataEntrada`, `ValorTotal`, `Usuario`, `Observacoes?`
- `Itens`: `EntradaEstoqueItem` → `ItemEstoqueId`, `Quantidade`, `UnidadeMedida`, `ValorUnitario`, `ValorTotal`, `Validade?`, `Lote?`, `LocalArmazenamento?`

> Pode começar **simplificado** (entrada item-a-item gerando lote+movimentação) e só
> depois agrupar em `EntradaEstoque`. Ver fases (§14).

### 3.5 `AjusteEstoque` (cabeçalho de ajuste/inventário) — *opcional na fase 1*
- `Id`, `ItemEstoqueId`, `LoteEstoqueId?`, `QuantidadeAnterior`, `QuantidadeNova`, `Diferenca`,
  `Motivo` (obrigatório), `Usuario`, `DataHora`, `Observacao?`
- Cada ajuste **gera** uma `MovimentacaoEstoque` (`AjustePositivo`/`AjusteNegativo`).
- **Nunca** alterar saldo sem este registro.

### 3.6 `Fornecedor` (cadastro de apoio — novo)
- `Id`, `Nome`, `Documento?` (CNPJ/CPF), `Telefone?`, `Email?`, `Observacoes?`, `Ativo`
- Mínimo viável; pode crescer depois (endereço, contato, condições).

### 3.7 Prontidão da Personalizada (não é estoque) — modelagem de preparo
A prontidão pertence à **Produção**, mas precisamos do gancho desde já:

- Acrescentar a `EntregaItem` (Personalizada) e/ou `EntregaPet`:
  - `StatusPreparo` (enum `StatusPreparoPersonalizada`: `NaoPronta`, `ParcialmentePronta`, `Pronta`)
  - `PacotesProntos?` (int — habilita "parcialmente pronta")
  - `PreparadoEm?`, `PreparadoPor?`
- Setado pela Produção (fase futura). Por ora, a tela de Entregas **lê** esse status
  (substituindo `prontaMock`). Enquanto Produção não existe, fica `NaoPronta` por padrão.

> **Por que não em ItemEstoque?** Porque a comida personalizada é única por pet/entrega —
> não há "saldo geral" que outro cliente possa consumir. Misturar quebraria o conceito.

---

## 4. Enums

### 4.1 `TipoItemEstoque`
`Insumo` · `ProdutoAcabadoCasa`

### 4.2 `CategoriaEstoque`
`Proteinas` · `Carboidratos` · `Legumes` · `Visceras` · `Suplementos` · `Embalagens` ·
`Etiquetas` · `MateriaisLimpeza` · `MateriaisAuxiliares` · `ProdutoAcabado` · `Outros`

> Decisão aberta: **enum** (simples, lista fixa) **vs cadastro** (como `CategoriaIngrediente`).
> Recomendo **enum** agora; migrar para cadastro só se precisarem criar categorias livres.

### 4.3 `UnidadeMedida`
`Kg` · `G` · `Unidade` · `Pacote` · `Caixa` · `Litro` · `Ml` · `Outro`

### 4.4 `TipoMovimentacao`
- Entradas: `EntradaCompra`, `EntradaProducao` (produto acabado envasado), `AjustePositivo`, `TransferenciaEntrada`
- Saídas: `SaidaProducao` (consumo de insumo), `Descarte`, `Perda`, `Vencimento`,
  `AjusteNegativo`, `TransferenciaSaida`, `ConsumoInterno`, `BaixaEntrega`

### 4.5 `MotivoSaida` (detalhe opcional)
`Producao` · `Descarte` · `Perda` · `Vencimento` · `AjusteManual` · `Transferencia` · `ConsumoInterno` · `Outro`

### 4.6 `StatusLote`
`Ativo` · `Esgotado` · `Vencido` · `Bloqueado`

### 4.7 `StatusPreparoPersonalizada`
`NaoPronta` · `ParcialmentePronta` · `Pronta`

---

## 5. Relacionamentos

```
Fornecedor 1—N ItemEstoque (principal)
Fornecedor 1—N LoteEstoque
Ingrediente 1—N ItemEstoque        (item alimentar referencia o ingrediente)
Receita + TamanhoPacote 1—1 ItemEstoque(ProdutoAcabadoCasa)
ItemEstoque 1—N LoteEstoque
ItemEstoque 1—N MovimentacaoEstoque
LoteEstoque 1—N MovimentacaoEstoque
EntradaEstoque 1—N EntradaEstoqueItem (gera Lote + Movimentação)
EntregaItem(Personalizada) 1—1 StatusPreparo   (gancho de Produção)
```
- FKs para módulos existentes (Ingrediente/Receita/TamanhoPacote/Fornecedor) = **Restrict**.
- `Movimentacao`/`Lote` referenciam `ItemEstoque` = **Restrict** (não cascade: histórico imutável).

---

## 6. Integração com Ingredientes (sem duplicação)

- `ItemEstoque.IngredienteId` (FK opcional). O **Ingrediente** continua dono de:
  nome de referência, categoria alimentar, coeficiente de conversão (cozido÷cru), **custo de referência** (`CustoAtualKg`).
- O **ItemEstoque** passa a ser dono de: **saldo físico, lotes, validade, custo médio real, movimentações**.
- A Produção, ao consumir um ingrediente de uma receita, **resolve** para o(s) `ItemEstoque` vinculado(s) e dá baixa.

> **Decisão aberta (cardinalidade):** 1 Ingrediente → **1** ItemEstoque (simples, recomendado)
> ou **N** (ex.: "Frango peito cru" e "Frango peito congelado")? Se N, precisamos de uma
> regra de qual item consumir na produção (item primário ou escolha explícita). Recomendo
> **1:1 agora**, deixando o schema (FK em ItemEstoque) já pronto para 1:N depois.

---

## 7. Regras — Insumos
- Trabalham com **lote, validade, fornecedor, custo médio, mínimo**.
- Entrada por **compra** (gera lote). Saídas por **produção/perda/descarte/vencimento/ajuste/transferência**.
- Consumo segue **PEPS/FIFO** (lote mais antigo não vencido primeiro).
- Embalagens/etiquetas/limpeza: insumo **sem** `IngredienteId`; `ControlaValidade` geralmente `false`.

## 8. Regras — Lote e validade
- Cada **compra** gera um lote com validade própria.
- Saldo do item = Σ saldo dos lotes ativos.
- **FIFO:** ao dar saída, consome do lote mais antigo (`DataEntrada`/`Validade`) com saldo > 0 e **não vencido**.
- Uma saída pode **atravessar vários lotes** ⇒ gera **uma `Movimentacao` por lote** tocado (preserva rastreio e custo por lote).
- Alertas: itens **próximos do vencimento** (janela configurável, ex.: 7/15 dias) e **vencidos** (bloqueiam consumo → exigem descarte/ajuste).

## 9. Regras — Custo médio
**Recomendação: custo médio ponderado móvel** (atualizado a cada entrada de compra):

```
custoMedioNovo = (saldoQtd × custoMedioAtual + entradaQtd × custoEntrada) ÷ (saldoQtd + entradaQtd)
```
- Saídas **não** alteram o custo médio (apenas reduzem saldo).
- O **custo por lote** é mantido em paralelo (permite valorização FIFO real quando necessário).
- Usos futuros: custo real de receita, custo de produção, margem, previsto × real.
- Precisão sugerida: quantidades `decimal(18,3)`, custos `decimal(18,4)`, valores `decimal(18,2)`.

## 10. Regras — Entrada
1. Valida item ativo, quantidade > 0, custo ≥ 0.
2. Cria/atualiza **lote** (com validade/fornecedor/custo).
3. Gera **Movimentacao** `EntradaCompra` (saldo anterior→posterior).
4. Atualiza `LoteEstoque.QuantidadeAtual`, `ItemEstoque.QuantidadeAtual` e `ItemEstoque.CustoMedio` — **tudo na mesma transação**.

## 11. Regras — Saída
1. Valida saldo suficiente (política de saldo negativo — §13).
2. Resolve lotes por **FIFO** (exceto saída de lote específico em descarte/vencimento).
3. Gera **uma ou mais** `Movimentacao` (por lote), com `Tipo`/`MotivoCodigo`, usuário e data.
4. Atualiza saldos de lote e item na mesma transação.
5. Saída de **produção** referenciará `OrdemProducaoId` no futuro (campo já previsto).

## 12. Regras — Movimentação e ajuste manual
- **Toda** alteração de saldo gera movimentação (não há saldo "editável").
- Ajuste manual exige **motivo** e registra `QuantidadeAnterior/Nova/Diferenca` + usuário + data.
- Movimentação é **append-only**: correções = nova movimentação (estorno), nunca update/delete.

## 13. Estoque mínimo e alertas
- `QuantidadeMinima` por item. Saldo < mínimo ⇒ **alerta de compra**.
- Consultas/painéis previstos: **abaixo do mínimo**, **próximos do vencimento**, **zerados**, **saldo crítico**.

## 14. Produto Acabado — Receita da Casa
- Item por `(Receita, TamanhoPacote)`, `UnidadeMedida = Pacote`.
- **Entrada por produção** (`EntradaProducao`) cria lote com validade do envase (Produção, futuro).
- **Saldo geral**: pacotes de "Frango 500g" atendem qualquer cliente daquela receita.
- **Baixa por entrega** (`BaixaEntrega`, futuro) quando a entrega é concluída (idempotente).

## 15. Personalizada — prontidão
- Não há saldo geral. Controla-se `StatusPreparo` por `EntregaItem` (§3.7).
- Produção marca `Pronta`/`ParcialmentePronta` (com `PacotesProntos`).
- Tela de Entregas lê esse status (substitui `prontaMock`).

## 16. Como a tela de Entregas vai consultar (substituindo os mocks)
Read model dedicado (sem expor o ledger), ex.: `IEstoqueConsultaService`:

- **Casa (por dia):** necessário por `(receita, tamanho)` (de `EntregaItemPacote`) × disponível
  (`ItemEstoque.QuantidadeAtual` do produto acabado) ⇒ situação **ok / faltam N**.
  - Substitui `estoqueMockPacotes` e o resumo "Resumo do dia — Receitas da Casa".
- **Personalizada:** `StatusPreparo` por entrega/pet ⇒ **Pronta / Não pronta / Parcialmente**.
  - Substitui `prontaMock` e o resumo "Resumo do dia — Personalizadas".
- O **bloqueio de avanço de status** da entrega (hoje no frontend) passa a ter base real e
  deve ser replicado no backend (`EntregaService.MudarStatusAsync`) quando estes dados existirem.

> **Importante (estoque geral × dia):** o estoque da Casa é um **pool**. Se vários dias
> consomem o mesmo pool, a "falta" precisa considerar reservas/sequência. Sugestão inicial:
> comparar **necessário do dia × saldo atual** (simples) e evoluir para **reserva/alocação**
> por data quando houver Produção planejada. Decisão a alinhar (§18).

## 17. Conexão futura com Produção (preparar, não implementar)
- Antes de produzir: **verificar disponibilidade** de insumos (FIFO simulada).
- Ao produzir: **baixa de insumos** (`SaidaProducao`) + **entrada de produto acabado** (`EntradaProducao`).
- Produção informa **peso cru, peso cozido, peso envasado** ⇒ rendimento real, perdas, sobras,
  impacto no custo. Campos de origem (`OrdemProducaoId`) já previstos na Movimentação/Lote.

## 18. Cuidados técnicos antes de implementar
1. **Ledger imutável + saldo cacheado**: movimentação é verdade; `QuantidadeAtual` é projeção
   atualizada **na mesma transação**. Invariante: `SaldoPosterior == cache`.
2. **Transações atômicas** para entrada/saída/ajuste (movimentação + lote + item juntos).
3. **Concorrência otimista** (`RowVersion`) em `ItemEstoque`/`LoteEstoque` p/ evitar saldo negativo em corrida.
4. **Política de saldo negativo**: bloquear (recomendado) ou permitir com alerta — **decidir**.
5. **Unidade canônica**: definir como tratar kg↔g (recomendo guardar na `UnidadeMedida` do item e
   converter na borda da entrada; alimentar internamente pode padronizar em **g**).
6. **Idempotência** de baixa por entrega/produção (evitar baixa dupla).
7. **Precisão decimal** consistente (§9).
8. **Soft delete** (`Ativo`); proibir excluir item/lote com movimentação.
9. Convenções EF do projeto: enums `HasConversion<string>`, FK/índices curtos, snake_case,
   `ApplyConfigurationsFromAssembly`, migration + ModelSnapshot + Designer escritos à mão.
10. Vínculos a módulos inexistentes (Produção) ficam como **id sem FK** até o módulo existir
    (mesmo padrão de `EntregadorId` em Entregas).

## 19. Fases de implementação sugeridas
- **Fase 1 — Cadastro base:** `Fornecedor`, `ItemEstoque` (Insumo + ProdutoAcabadoCasa), enums,
  migration. Saldo só leitura (zero). CRUD + tela de listagem/cadastro.
- **Fase 2 — Entradas/Compras:** `LoteEstoque`, `MovimentacaoEstoque`, `EntradaEstoque`,
  custo médio, atualização de saldo. Tela de entrada.
- **Fase 3 — Saídas/Ajustes/Mínimo:** saídas (perda/descarte/ajuste/transferência), FIFO,
  ajuste manual com histórico, alertas (mínimo/vencimento/zerado).
- **Fase 4 — Integração Entregas (Casa):** read model necessário × disponível; substitui mock de estoque.
- **Fase 5 — Prontidão Personalizada:** `StatusPreparo` em `EntregaItem` + leitura na tela;
  substitui mock de prontidão (sem Produção completa ainda).
- **Fase 6 — Produção:** baixa de insumos + entrada de produto acabado + rendimento/perdas.
- **Fase 7 — Avançado:** baixa automática por entrega, relatórios, custo real de receita, previsão de compras.

## 20. Decisões abertas (preciso da sua escolha)
1. **Custo médio:** confirma **ponderado móvel** (§9)?
2. **Ingrediente ↔ ItemEstoque:** **1:1** (recomendado) ou **1:N**?
3. **Categoria de estoque:** **enum** (recomendado) ou **cadastro** editável?
4. **Saldo negativo:** **bloquear** (recomendado) ou permitir com alerta?
5. **Entrada:** começar **simplificada** (item-a-item) ou já com **cabeçalho de compra** (`EntradaEstoque`)?
6. **Estoque da Casa × dia:** começar com **necessário do dia × saldo atual** (simples) e evoluir p/ reserva?
7. **Fornecedor:** cadastro mínimo agora ou só um campo texto por enquanto?
