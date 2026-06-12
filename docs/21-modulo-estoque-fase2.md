# 21 — Módulo de Estoque · Fase 2 (melhorias)

> Status: **proposta para aprovação**. Nenhum código/migration ainda.
> Evolui o Estoque existente (não recria). **Sem** Produção, Entregas, Financeiro,
> Compra multi-itens, Nota Fiscal, Reserva, Rotas nesta fase.

## 0. Princípio mantido
Conexões entre módulos **sempre por id** (`IngredienteId`, `ItemEstoqueId`, `FornecedorId`,
`ReceitaId`, `TamanhoPacoteId`, `CategoriaId`). Nome é só visual.

---

## 1. Movimentações — listagem geral (Estoque > Movimentações)
**Somente consulta.** Reusa a entidade `MovimentacaoEstoque` (não duplica nada).
O detalhe por item continua existindo.

- **Endpoint novo:** `GET /api/estoque/movimentacoes` (paginado + filtrado).
  - Query: `dataInicio`, `dataFim`, `itemEstoqueId`, `categoria`, `tipo`, `fornecedorId`,
    `loteId`, `usuario`, `motivo`, `sentido`, `pagina`, `tamanhoPagina`.
  - Retorno: `{ total, itens: MovimentacaoGeralDto[] }`.
- **DTO novo** `MovimentacaoGeralDto` (projeção com joins, sem nova tabela):
  dataHora, itemId, itemNome, categoria, tipo, sentido, quantidade, unidade, loteCodigo,
  custoUnitario, valorTotal, saldoAnterior, saldoPosterior, usuario, motivo, observacao,
  fornecedorNome (do lote, quando houver), origem (entrada/ajuste/produção/entrega — derivada
  dos ids de origem já existentes).
- Índices: já existem `ix_mov_estoque_data`, `ix_mov_estoque_item`, `ix_mov_estoque_tipo`.
  Sem migration.

## 2. Compras / Entradas (Estoque > Compras)
**Organiza as entradas que já existem.** Sem compra multi-itens agora.

- **Endpoint novo:** `GET /api/estoque/entradas` (filtrado).
  - Query: `dataInicio`, `dataFim`, `fornecedorId`, `itemEstoqueId`, `categoria`, `loteId`,
    `usuario`, `comFrete` (bool?), `valorMin`, `valorMax`.
  - Retorno: `EntradaCompraDto[]`.
- **DTO novo** `EntradaCompraDto` (join entrada + item + fornecedor + lote):
  dataCompra, dataEntrada, fornecedorNome, itemNome, categoria, quantidade, unidade,
  valorUnitarioOriginal, frete, custoUnitarioEfetivo (= `lote.CustoUnitario`), valorTotal,
  loteCodigo, validade, usuario, observacoes.
- A tela é histórico/consulta. "Nova compra" continua sendo a **Entrada** já existente
  (no detalhe do item) — agora com frete (§3). Sem migration além da do frete.

## 3. Frete na EntradaEstoque
**Decisão aprovada: frete compõe o custo (custo real/landed) por padrão.**

- **Migration `AddFreteEntradaEstoque`** — adiciona em `entradas_estoque`:
  - `frete numeric(18,2) not null default 0`
  - `frete_compoe_custo boolean not null default true`
- Semântica dos campos da entrada:
  - `ValorUnitario` = **valor unitário original** do produto (sem frete) — mantém o significado.
  - `Frete` = valor do frete da compra (novo).
  - `FreteCompoeCusto` = se o frete entra no custo (default `true`).
  - `ValorTotal` passa a ser **o total real pago** = (ValorUnitario × Qtd) + Frete.
    *(Linhas existentes têm frete 0 → ValorTotal inalterado.)*

### 3.1 Custo unitário efetivo
```
valorProdutos       = ValorUnitario × Quantidade
custoUnitarioEfetivo = FreteCompoeCusto
                       ? (valorProdutos + Frete) / Quantidade
                       : ValorUnitario
```
Exemplo (batata doce): 10 kg × R$5 = R$50 + frete R$10 = R$60 → **R$6,00/kg**.

### 3.2 Impacto no custo médio ponderado móvel
- `LoteEstoque.CustoUnitario` = **custoUnitarioEfetivo** (já guarda o efetivo).
- `item.RegistrarEntrada(qtd, custoUnitarioEfetivo)` → o **custo médio já reflete o frete**.
- `MovimentacaoEstoque` da entrada: `custoUnitario = efetivo`, `valorTotal = qtd × efetivo`.
- Nada muda na fórmula do custo médio — só passa a receber o custo efetivo.

### 3.3 Histórico preservado
- Original: `EntradaEstoque.ValorUnitario`.
- Frete: `EntradaEstoque.Frete`.
- Efetivo: `LoteEstoque.CustoUnitario` (ligado pela entrada).
Assim relatórios conseguem comparar **original × frete × efetivo** sem coluna redundante.

### 3.4 Rateio futuro (apenas preparado)
Quando existir `CompraEstoque` com vários itens, o frete será rateado **proporcional ao
valor dos itens** (ex.: 60%/40%) gerando o `custoUnitarioEfetivo` de cada `EntradaEstoque`.
Nesta fase, frete por entrada de **1 item** só.

## 4. Cadastros > Categorias (submenu)
Página única em **Cadastros > Categorias** com seções/abas por tipo:
- **Categorias de Ingredientes** → CRUD completo (§5).
- **Categorias de Estoque** → lista (somente leitura nesta fase — enum, §6).
- **Categorias de Fornecedores** → lista (somente leitura nesta fase — enum, §7).
Estrutura pronta para receber Estoque/Fornecedor como cadastro no futuro, sem retrabalho de layout.

## 5. Categorias de Ingredientes — CRUD + tela
A tabela `categorias_ingredientes` já existe (`Nome`, `Ativo`). A categoria **continua sendo
usada no cadastro de Ingredientes** (vínculo por `CategoriaId`); ingredientes existentes não quebram.

- **Backend:** adicionar à `CategoriaIngrediente` métodos `Atualizar`, `DefinirAtivo`,
  e (opcional) `Ordem`/`Descricao`. Serviço `ICategoriaIngredienteService` + endpoints:
  - `GET /api/categorias-ingredientes` (já existe — passa a incluir inativas com flag)
  - `POST /api/categorias-ingredientes`
  - `PUT /api/categorias-ingredientes/{id}`
  - `PUT /api/categorias-ingredientes/{id}/status`
- **Migration (opcional) `AddCamposCategoriaIngrediente`** — só se você quiser ordem/descrição:
  - `ordem int not null default 0`
  - `descricao varchar(255) null`
- **Regras:** nome único; ao **inativar**, bloquear se houver ingrediente ativo usando a
  categoria (mesma proteção dos outros cadastros), com mensagem específica.
- **Tela:** listar / criar / editar / ativar-inativar / (ordenar se houver `ordem`).

> Observação: suas categorias iniciais sugeridas (Proteína, Carboidrato, Vegetal, Óleo,
> Suplemento, Tempero, Outros) podem divergir das já semeadas. Não vou renomear/recriar as
> existentes para não quebrar vínculos — você ajusta pela tela (criar/editar/inativar).

## 6. Categoria de Estoque — recomendação
**Recomendo a opção 3:** manter como **enum nesta fase** e já deixar a tela
**Cadastros > Categorias** preparada para receber Estoque como cadastro depois.

Motivo: converter enum → tabela exige (a) nova tabela, (b) trocar a coluna `categoria` de
`itens_estoque` (hoje string do enum) por `categoria_id` (FK) + migração de dados, (c) ajustar
configs/serviços/telas. É uma migração de **médio risco** que não agrega valor imediato — os
valores atuais atendem. Quando você decidir, faço a migração gradual (criar tabela, semear com
os valores do enum, adicionar `categoria_id`, copiar, então aposentar o enum).

## 7. Categoria de Fornecedor — recomendação
Hoje é **enum** (`Ingredientes, Embalagens, Etiquetas, MaterialLimpeza, Servicos, Outros`).
**Recomendo manter enum nesta fase** (mesma lógica do Estoque; baixo valor em converter agora).
Exibida como lista somente-leitura na tela de Categorias. Converte junto com Estoque no futuro.

## 8. Endpoints novos (resumo)
| Método | Rota | Uso |
|---|---|---|
| GET | `/api/estoque/movimentacoes` | Livro-razão geral (paginado/filtrado) |
| GET | `/api/estoque/entradas` | Histórico de compras/entradas (filtrado) |
| POST | `/api/categorias-ingredientes` | Criar categoria |
| PUT | `/api/categorias-ingredientes/{id}` | Editar categoria |
| PUT | `/api/categorias-ingredientes/{id}/status` | Ativar/inativar |
| (alterado) POST | `/api/estoque/entradas` | passa a aceitar `frete` e `freteCompoeCusto` |

## 9. Migrations necessárias
1. **`AddFreteEntradaEstoque`** — `frete`, `frete_compoe_custo` em `entradas_estoque`. (obrigatória)
2. **`AddCamposCategoriaIngrediente`** — `ordem`, `descricao` em `categorias_ingredientes`. (opcional)
Cada uma: migration `.cs` + `.Designer.cs` + atualização do `ApplicationDbContextModelSnapshot`
(escritos à mão, como no projeto).

## 10. Riscos técnicos
- **Snapshot/migration à mão:** risco de divergência model×snapshot. Mitigado por (a) índices/
  colunas explícitos batendo nos 3 lugares e (b) o `Ignore(PendingModelChangesWarning)` já ativo
  (o schema vem da migration; startup não aborta por divergência cosmética).
- **Semântica de `ValorTotal`** na entrada: muda para "total real". Seguro porque entradas
  existentes têm `frete = 0`.
- **Inativar categoria de ingrediente em uso:** precisa de checagem para não quebrar ingredientes.
- **Listagem geral de movimentações:** paginar e filtrar por índices existentes (`data_hora`,
  `item`, `tipo`) para não pesar.
- **Não** alterar `MovimentacaoEstoque`/`LoteEstoque`/`ItemEstoque` (sem migration neles).

## 11. Plano por etapas (executar só após aprovação)
- **Etapa 1 — Backend frete:** migration `AddFreteEntradaEstoque`; `EntradaEstoque` (+frete);
  ajuste do `EstoqueMovimentacaoService.RegistrarEntradaAsync` (custo efetivo); `RegistrarEntradaRequest` (+frete).
- **Etapa 2 — Backend listagens:** `GET /estoque/movimentacoes` e `GET /estoque/entradas` (+DTOs).
- **Etapa 3 — Backend categorias de ingrediente:** CRUD + (opcional) migration de ordem/descrição + regra de inativação.
- **Etapa 4 — Frontend Estoque:** tela **Movimentações**; tela **Compras**; frete no diálogo de Entrada.
- **Etapa 5 — Frontend Cadastros > Categorias:** tela com CRUD de Categorias de Ingredientes + listas read-only de Estoque/Fornecedor.
- **Validação:** após Etapas 1–3, conferir build/migration/endpoints; depois telas (4–5).

## 12. Fora de escopo (confirmado)
Produção, Ordem de Produção, baixa por produção/entrega, integração real com Entregas, reserva,
Rotas, Contas a Pagar, Financeiro, Compra multi-itens, Nota Fiscal, previsão/planejamento de compras.
