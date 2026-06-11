# 13. Módulo de Ingredientes

Base para Receitas (Casa e Personalizadas), cálculo de custo, planejamento de
produção, planejamento de compras e controle de estoque futuro.

## Objetivo
Cadastrar e gerenciar todos os ingredientes, permitindo calcular o custo das
receitas e converter entre peso **cru** e peso **cozido**.

## Entidades

### CategoriaIngrediente
Tabela (não enum) para permitir novas categorias sem migração.
Seed: Proteína, Carboidrato, Vegetal, Óleo, Suplemento, Tempero.

### Ingrediente
- nome, categoria, **status** (ativo/inativo)
- **tipo_conversao**: `perda` | `ganho` | `sem_conversao`
- **coeficiente_conversao** = rendimento = **cozido ÷ cru**
- **custo_atual_kg** (R$/kg)

> **Escopo desta fase:** o **histórico de custos não será implementado agora**.
> O custo é **informado manualmente**. A entidade `HistoricoCustoIngrediente`
> permanece **prevista** no modelo (ver "Preparação para evolução"), mas não é
> construída nesta fase.

## Conversão (regra central)
Um único coeficiente (rendimento = cozido ÷ cru) cobre os três casos:

| Tipo | Coeficiente | Exemplo |
|---|---|---|
| Perda | < 1 | Batata-doce 0,55 → 1 kg cru = 550 g cozido |
| Ganho | > 1 | Arroz 3,00 → 333 g cru = 1 kg cozido |
| Sem conversão | = 1 | Sal, óleos, suplementos |

Fórmulas:
- `cozido = cru × coeficiente`
- `cru = cozido ÷ coeficiente`

No cadastro, a equipe **digita o fator de correção em %** (a perda ou o ganho de
peso no preparo) e o sistema converte para o coeficiente interno:
- **Perda X%** → coeficiente = 1 − X/100 (batata-doce 45% → 0,55 → 1 kg cru = 550 g cozido)
- **Ganho X%** → coeficiente = 1 + X/100 (arroz 200% → 3,00 → 333 g cru = 1 kg cozido)
- **Sem conversão** → 0% → coeficiente = 1

A pré-visualização (cru↔cozido) e o custo real são calculados automaticamente.

## Custos
- `custo_atual_kg` no ingrediente — **informado manualmente** nesta fase (preço do insumo **cru**).
- **Custo/kg real (cozido)** = `custo_atual_kg ÷ coeficiente` (derivado, não armazenado):
  reflete que para render 1 kg cozido pode ser necessário mais (perda) ou menos
  (ganho) de insumo cru. Ex.: batata-doce R$ 6,50/kg cru ÷ 0,55 = **R$ 11,82/kg cozido**.
- O rendimento é exibido em **percentual** (cozido ÷ cru × 100): perda 55%, ganho 300%.
- Custo de item de receita: `(gramas_cru ÷ 1000) × custo_atual_kg` (ficha em
  cozido é convertida para cru antes). Receita = soma dos itens.

## Preparação para evolução futura (NÃO implementar agora)
O foco atual é o **cálculo correto do custo de receitas e produção**. O domínio
foi modelado para integrar, mais adiante, sem refatoração disruptiva:

| Evolução futura | Como o modelo já está preparado |
|---|---|
| **Fornecedores** | `Ingrediente` referenciável por `fornecedor_id`/tabela `fornecedores` sem alterar o consumo atual. |
| **Compras** | Entradas de compra podem alimentar custo e estoque; o ingrediente tem `id` estável e custo numérico isolado. |
| **Estoque de matéria-prima** | Saldo de insumo cru vive em tabela própria ligada ao ingrediente; não conflita com o estoque de produto acabado. |
| **Financeiro** | Custos em `numeric` e por ingrediente, prontos para compor custo de produção e margem. |
| **Atualização automática de custos** | `custo_atual_kg` é um único ponto de verdade; uma **fonte do custo** (manual hoje; compra/integração depois) e o `HistoricoCustoIngrediente` podem ser adicionados sem quebrar quem consome o custo. |

**Decisões que mantêm o caminho aberto:**
- Custo como propriedade única e numérica do ingrediente (não espalhado).
- Conversão (rendimento) separada do custo — cada um evolui isolado.
- Ingrediente com identidade estável e desacoplado de Compras/Estoque/Financeiro.
- Categoria já em tabela; mesma estratégia servirá para Fornecedor.
- Nenhuma regra atual pressupõe ausência de histórico/fornecedor/estoque — só
  não os utiliza ainda.

## Decisões arquiteturais
1. Categoria como tabela (extensível).
2. `tipo_conversao` como enum (conjunto estável) + coeficiente numérico.
3. Coeficiente = rendimento (cozido ÷ cru) — uma grandeza para os 3 casos e ambas as direções.
4. Base em **kg/massa** para custo e conversão (volume fica como extensão futura).
5. Histórico de custo dedicado (auditoria de preço).
6. Substitui a antiga `fatores_correcao` do modelo inicial.

## Tela
- **Tabela** densa em **lista única**: Nome · Categoria · Correção · Custo/kg cru · Custo/kg real · Status.
- **Filtros**: nome, categoria, status. **Ordenação**: nome, categoria, custo.
- **Dialog criar/editar** com fator de correção (%) e pré-visualização da conversão + custo real em tempo real.
- **Excluir** (com confirmação) além de inativar.
- Tudo no Design System (Angular Material, paleta, densidade compacta).

## Backend (implementado — persistência real)
- **Entidades:** `CategoriaIngrediente`, `Ingrediente` (Clean Architecture).
- **EF Core + PostgreSQL:** configs com `snake_case`, FK `ingrediente → categoria`
  (on delete restrict), índice único em `nome`, `tipo_conversao` como string
  (check), precisão `coeficiente (8,4)` e `custo (12,2)`, checks `coeficiente > 0`
  e `custo >= 0`.
- **Migration:** `AddCatalogIngredientes`.
- **Seed:** 6 categorias + ingredientes de exemplo (apenas na 1ª execução).

### Endpoints (todos exigem JWT)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/categorias-ingredientes` | Lista categorias ativas |
| GET | `/api/ingredientes` | Lista ingredientes |
| GET | `/api/ingredientes/{id}` | Obtém um ingrediente |
| POST | `/api/ingredientes` | Cria (valida nome, coeficiente > 0, custo ≥ 0, categoria existente) |
| PUT | `/api/ingredientes/{id}` | Atualiza |
| DELETE | `/api/ingredientes/{id}` | Exclui |

Validado de ponta a ponta: build limpo, migration aplicada, seed, e o fluxo
listar/criar/atualizar/excluir + 401 (sem token) e 400 (validação) com os status
esperados. A tela consome esses endpoints (sem mock).
