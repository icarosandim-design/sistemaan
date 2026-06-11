# 9. Riscos e premissas

## Premissas assumidas (decisões validadas com o negócio)

1. **Receita personalizada pertence a um único pet** (sem compartilhamento entre pets).
2. **Financeiro é apenas cadastral** nesta fase (sem contas a receber/pagamentos).
3. **Estoque controla apenas quantidade** (sem lote, validade ou FEFO).
4. **Filosofia "não pode faltar"**: estoque mínimo previne falta de Casa;
   personalizada deve estar pronta antes da entrega (senão o sistema sinaliza e bloqueia).
5. **Horizontes:** produção 14 dias, reserva 30 dias (configuráveis).
6. **Base de recálculo de data:** data prevista (não a data real da entrega).
7. **Volume pequeno** (~150 clientes): o gargalo é a modelagem, não a performance.

## Riscos e mitigações

| # | Risco | Impacto | Mitigação |
|---|---|---|---|
| 1 | Consistência entre estoque físico e reservas ("disponível = físico − reservado") | Alto | Reserva como entidade explícita; saldo via trigger; disponível via view; confirmação transacional. |
| 2 | Granularidade do ciclo da personalizada (status por receita/pet/ciclo) | Alto | Resolvido: status na receita personalizada (1 por pet) + `ciclo_ref`. |
| 3 | Recálculo de datas e calendário (atrasos, feriados, fuso) | Médio-alto | Base na data prevista; calendário operacional; `timestamptz`/UTC. |
| 4 | Snapshot vs. plano vivo (mudança de plano reescrevendo histórico) | Médio | `itens_entrega` imutável após confirmação. |
| 5 | Migração das planilhas atuais (dados sujos, históricos) | Médio | Importação única planejada + validação; não subestimar. |
| 6 | Escopo financeiro crescer para contas a receber | Médio | Escopo "básico" travado; financeiro pleno como fase futura. |
| 7 | Concorrência (dois operadores confirmando ao mesmo tempo) | Baixo-médio | Controle otimista (rowversion) em saldo/status. |
| 8 | Job diário falhar (entregas não materializadas / produção não planejada) | Médio | Job idempotente; reexecutável; monitoramento e log de eventos. |
| 9 | Estoque mínimo mal calibrado (sobra ou falta) | Médio | Configurável por produto; revisar com dados reais de consumo. |
| 10 | Perecibilidade ignorada no futuro (se exigirem validade) | Baixo (fase 1) | Estrutura permite evoluir `movimentos_estoque` com lote/validade depois. |

## Pontos a revisar com dados reais (pós-implantação)

- Calibragem de `estoque_minimo` e `lote_producao` por produto.
- Ajuste fino dos horizontes de produção e reserva.
- Necessidade (ou não) de rastreio de lote/validade.
- Necessidade (ou não) de evoluir o financeiro.
