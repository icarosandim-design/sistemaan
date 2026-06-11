# 5. Fluxos operacionais

## F1 — Cadastro e plano
1. Cadastra-se o Cliente.
2. Cadastra-se o Pet (frequência + primeira data de entrega).
3. Monta-se o Plano Alimentar vigente com itens de Casa (Produtos) e/ou Personalizados (Receitas Personalizadas).
4. Para itens personalizados, cria-se a Receita Personalizada do pet com sua ficha técnica (gramaturas específicas).

## F2 — Geração automática das próximas entregas (job diário)
Para cada pet ativo:
```
enquanto proxima_data_entrega ≤ hoje + horizonte_reserva_dias
       e não existe entrega materializada para essa data:
    criar Entrega (status = agendada)
    criar ItemEntrega = snapshot do plano vigente
    criar ReservaEstoque para itens de casa (disponível permitindo)
```
- A `proxima_data_entrega` **não** avança aqui — só na confirmação (R10).
- A unicidade parcial `(pet_id, data_prevista)` impede duplicidade.

## F3 — Planejamento de produção (job diário, janela de produção)
Roda sobre `[hoje, hoje + horizonte_producao_dias]`:
```
1. Coletar entregas da janela (agendadas).
2. Trilha CASA: somar demanda por produto.
       projeção = saldo_fisico − demanda_janela
       se projeção < estoque_minimo:
           a_produzir = estoque_minimo + demanda_janela − saldo_fisico
           arredondar a_produzir ao lote_producao
           → OrdemProducao (tipo = casa)
3. Trilha PERSONALIZADA: cada receita personalizada com status=produzir
   e ciclo na janela → OrdemProducao (tipo = personalizada).
4. Consolidar listas do painel:
   - "O que produzir" = déficits de casa + personalizadas em produzir
   - "Reservado"      = reservas ativas por produto/entrega
```

## F4 — Produção da Casa (lote)
1. Equipe produz o lote.
2. Registra `MovimentoEstoque (entrada)` → trigger atualiza `SaldoEstoque`.
3. Reservas pendentes passam a estar cobertas pelo saldo.

## F5 — Produção Personalizada (sob demanda)
1. Item aparece na lista de produção (status `produzir`).
2. Equipe produz e marca `pronta_para_entrega`.
3. (Opcional) registra consumo real de ingredientes via fator de correção.

## F6 — Entrega
1. Roteiro do dia mostra entregas com `data_prevista = hoje` e status `agendada`.
2. **Confirmação** (transacional):
   - **Guard:** todas as personalizadas do pet devem estar `pronta_para_entrega`; senão, sinaliza pendência e bloqueia.
   - Itens de Casa → reserva vira `consumida` + `MovimentoEstoque (saida)` (baixa física).
   - Itens Personalizados → status volta a `produzir` e `ciclo_ref` avança.
   - Recalcula `proxima_data_entrega` (R8/R9).
   - Grava `ItemEntrega` snapshot (já materializado) e `EventoDominio`.
3. **Cancelamento:** libera reservas; não recalcula data.

## F7 — Painel operacional (consulta contínua)
Responde, por leitura agregada:

| Pergunta | Fonte |
|---|---|
| O que produzir? | Personalizadas `produzir` + produtos com déficit vs. mínimo |
| O que já está pronto? | Personalizadas `pronta_para_entrega` + saldo físico dos produtos |
| O que entregar? | Entregas `agendada` na janela |
| O que tem em estoque? | `saldo_estoque.saldo_fisico` por produto |
| O que está reservado? | `reservas_estoque (status=ativa)` por produto/entrega |
