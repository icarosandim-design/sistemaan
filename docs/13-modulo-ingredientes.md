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

### HistoricoCustoIngrediente
Registro automático a cada mudança de custo: data, valor anterior, novo valor, usuário.

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

No cadastro, a equipe **digita o coeficiente** e o sistema calcula/pré-visualiza
as duas direções automaticamente. `sem_conversao` força coeficiente = 1.

## Custos
- `custo_atual_kg` no ingrediente (preço do insumo **cru**).
- **Custo/kg real (cozido)** = `custo_atual_kg ÷ coeficiente` (derivado, não armazenado):
  reflete que para render 1 kg cozido pode ser necessário mais (perda) ou menos
  (ganho) de insumo cru. Ex.: batata-doce R$ 6,50/kg cru ÷ 0,55 = **R$ 11,82/kg cozido**.
- O rendimento é exibido em **percentual** (cozido ÷ cru × 100): perda 55%, ganho 300%.
- Toda alteração de custo gera registro em `historico_custos_ingredientes`.
- Custo de item de receita: `(gramas_cru ÷ 1000) × custo_atual_kg` (ficha em
  cozido é convertida para cru antes). Receita = soma dos itens.

## Decisões arquiteturais
1. Categoria como tabela (extensível).
2. `tipo_conversao` como enum (conjunto estável) + coeficiente numérico.
3. Coeficiente = rendimento (cozido ÷ cru) — uma grandeza para os 3 casos e ambas as direções.
4. Base em **kg/massa** para custo e conversão (volume fica como extensão futura).
5. Histórico de custo dedicado (auditoria de preço).
6. Substitui a antiga `fatores_correcao` do modelo inicial.

## Tela (estado atual: dados fictícios)
- **Tabela** densa: Nome · Categoria · Conversão · Coeficiente · Custo/kg · Status.
- **Filtros**: nome, categoria, status. **Ordenação**: nome, categoria, custo. **Paginação**.
- **Dialog criar/editar** com pré-visualização da conversão em tempo real.
- **Aba Histórico de Custos** no detalhe (data, valor anterior, novo valor).
- Tudo no Design System (Angular Material, paleta, densidade compacta).

> Próximo passo de backend: entidades + EF + migration + endpoints, ligando a
> tela aos dados reais (hoje a tela opera com mock em memória).
