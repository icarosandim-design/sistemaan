# 14. Módulo Tabela de Consumo

Faixas de peso → gramas/dia recomendadas. Base para, futuramente, sugerir o
consumo diário de um pet a partir do peso (em **Pets** e **Plano Alimentar**).

## Modelo de dados

### `faixas_consumo`
| Campo | Tipo | Regras |
|---|---|---|
| id | bigint | PK |
| peso_inicial | numeric(6,2) | not null |
| peso_final | numeric(6,2) | not null |
| gramas_por_dia | int | not null |
| ativo | boolean | not null, default true |
| criado_em / atualizado_em | timestamptz | auditoria |
- **CK:** `peso_inicial < peso_final`, `gramas_por_dia > 0`.
- **IX:** `(peso_inicial)`.

## Regras de negócio
- Peso inicial **menor** que o peso final.
- Faixa é **fechada**: aplica-se quando `peso ∈ [peso_inicial, peso_final]`
  (vale o início, o meio **e** o final — ex.: 5 kg pertence à faixa 3–5).
- **Faixas ativas não podem se sobrepor nem se encostar**: a próxima começa
  **após** o final da anterior (ex.: 3–5, depois **6**–8). Conflito quando
  `a.inicial <= b.final E b.inicial <= a.final` (faixas inativas podem conflitar).
- Status ativo/inativo (inativação no lugar de exclusão).
- Consulta: qual faixa **ativa** se aplica a um peso.

## Endpoints (exigem JWT)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/faixas-consumo` | Lista todas as faixas |
| GET | `/api/faixas-consumo/{id}` | Obtém uma faixa |
| GET | `/api/faixas-consumo/aplica?peso={kg}` | Faixa ativa aplicável (204 se nenhuma) |
| POST | `/api/faixas-consumo` | Cria (valida ordem, gramas > 0, sobreposição) |
| PUT | `/api/faixas-consumo/{id}` | Atualiza (inclui inativação via `ativo`) |

## Seed inicial
3–5 → 200 · 6–8 → 290 · 9–11 → 370 · 12–14 → 440 (apenas na 1ª execução).

## Tela
- **Tabela** (lista única, header fixo): Peso inicial · Peso final · Gramas/dia · Status, com ordenação e filtro por status.
- **Ferramenta "Consultar peso"**: informa um peso e mostra a faixa/gramas aplicável (usa `/aplica`).
- **Dialog criar/editar** com validação de ordem (inicial < final); erros de
  sobreposição vindos da API são exibidos via snackbar.
- Tudo no Design System (Angular Material, paleta, densidade compacta).

## Preparação para uso futuro
- A consulta `ConsultarPorPesoAsync(peso)` / `GET /aplica` é o ponto de extensão
  para **Pets** (sugerir gramas/dia ao informar o peso) e **Plano Alimentar**
  (compor pacotes). **Não** implementados nesta fase.

## Validação (resumo do que foi testado end-to-end)
Build limpo, migration aplicada, seed (4 faixas); listar; consulta 6,5 → 290 e
5 → faixa 5–8 (semiaberta); peso fora → 204; criar sobreposta → 400; inicial ≥
final → 400; inativar; criar sobre faixa **inativa** → permitido.
