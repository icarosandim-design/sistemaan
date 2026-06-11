# 16. Módulo Frequências de Entrega

Ciclos de entrega usados (futuramente) para gerar a agenda de entregas de um pet
a partir da primeira data do plano. Submenu de **Cadastros**.

## Entidade `FrequenciaEntrega`
| Campo | Observação |
|---|---|
| id | |
| nome | único |
| dias_ciclo | int? (nulo quando personalizada) |
| descricao | |
| personalizada | bool (ciclo definido por pet no futuro) |
| ativo | |

Tabela `frequencias_entrega` (migration `AddFrequenciasEntrega`): nome único;
check `personalizada = true OR (dias_ciclo IS NOT NULL AND dias_ciclo > 0)`.

## Regras
- Dias do ciclo > 0 (exceto personalizada).
- Apenas ativas devem aparecer para seleção futura.
- Personalizada preparada para uso futuro (sem lógica complexa agora).

## Serviço de cálculo de agenda (preparado — sem persistir)
`CalculadoraAgenda.Gerar(dataInicial, diasCiclo, horizonteDias)` gera as datas
futuras somando o ciclo, **independente da confirmação** de entregas anteriores.
Exemplos (validados): ciclo 7 a partir de 01/06 → 01, 08, 15, 22, 29; ciclo 14 →
01, 15, 29; ciclo 28 → 01/06, 29/06, 27/07.

> Regra de negócio (futuro): a próxima entrega existe por cálculo, não por
> confirmação da anterior. Confirmar uma entrega só muda o status dela; só é
> possível confirmar no dia; não se confirma o futuro de uma vez. Alterações
> manuais (uma entrega, ou todas a partir de uma data) ficam para o módulo Entregas.

## Endpoints (exigem JWT)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/frequencias-entrega` | Lista |
| GET | `/api/frequencias-entrega/{id}` | Obtém |
| POST | `/api/frequencias-entrega` | Cria |
| PUT | `/api/frequencias-entrega/{id}` | Atualiza |
| PUT | `/api/frequencias-entrega/{id}/status` | Inativa/reativa |
| GET | `/api/frequencias-entrega/preview-agenda?dataInicial=&diasCiclo=&horizonteDias=` | Pré-visualização da agenda (cálculo) |

## Seed inicial
Semanal (7), Quinzenal (14), Mensal operacional (28), Personalizada (manual).

## Tela
Lista (nome, ciclo, descrição, status) com filtro de status; dialog criar/editar
com toggle "Personalizada" (desabilita dias), e **pré-visualização** das próximas
entregas a partir de hoje. Inativação/reativação. Tudo no Design System.

## Não incluído (futuro)
Pets, Plano Alimentar, Entregas, Calendário real, confirmação/alteração de
entregas, geração persistida de entregas, produção, estoque.
