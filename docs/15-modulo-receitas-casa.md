# 15. Módulo Receitas da Casa

Ficha técnica operacional padrão da comida, **base 1 kg cozido**. Reusa o módulo
de Ingredientes; não cria cadastro paralelo.

## Entidades
- **Receita** (unificada — suporta `personalizada`+`pet_id` no futuro): codigo,
  nome, tipo (`Casa`|`Personalizada`), pet_id (nulo), observacoes, ativo.
- **ItemReceita** (ficha técnica): ingrediente_id + **gramas (cozidas)**.

## Tabelas (migration `AddReceitas`)
- `receitas`: tipo (string), codigo, nome, observacoes, pet_id (nulo), ativo,
  auditoria. **UQ** `(tipo, codigo)`.
- `itens_receita`: receita_id (FK cascade), ingrediente_id (FK restrict),
  gramas (CK > 0). **IX** `(receita_id)`.

## Regras
- Ingredientes vêm do módulo Ingredientes.
- Cada item: ingrediente + gramas cozidas.
- **Soma deve ser exatamente 1.000 g** (rendimento = 1.000 g quando válida).
- Código único.

## Cálculo de custo (gerencial)
Por item: `custo = (gramas_cozidas ÷ 1000) × (custo/kg cru ÷ rendimento do ingrediente)`
(= custo real/kg cozido do ingrediente). Receita: `custo_total = Σ`. Como a base
é 1 kg, **custo por kg cozido = custo_total**. O backend devolve `rendimento`,
`custoTotal` e `custoPorKgCozido` já calculados.

## Endpoints (exigem JWT)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/receitas-casa` | Lista (com rendimento/custo) |
| GET | `/api/receitas-casa/{id}` | Obtém |
| POST | `/api/receitas-casa` | Cria (valida soma = 1.000, código único, ingredientes existentes) |
| PUT | `/api/receitas-casa/{id}` | Atualiza (substitui a ficha) |
| PUT | `/api/receitas-casa/{id}/status` | Inativa/reativa (`{ ativo }`) |

## Tela
Mesma experiência aprovada: lista (código, nome, base 1 kg, custo/kg cozido,
status) com busca e filtro; dialog com editor de ficha técnica (gramas cozidas),
total em tempo real (faltam/excede/válida), custo gerencial discreto. Consome a
API real (sem mock); ingredientes vêm de `GET /api/ingredientes`.

## Não incluído (futuro)
Personalizadas, pacotes 250g/500g, estoque, produção, ordem de produção, entregas.
