# 4. Regras de negócio

## Plano alimentar e itens
- **R1.** Um item de plano é sempre `(referência, quantidade_pacotes)` com quantidade > 0.
- **R2.** A referência do item é **exatamente** um Produto (Casa) **ou** uma Receita Personalizada — nunca ambos, nunca nenhum.
- **R3.** Um pet pode ter itens de Casa e Personalizados simultaneamente.
- **R4.** Existe **um único plano vigente por pet**; planos anteriores ficam como histórico (`vigente=false`).
- **R5.** Alterar o plano **não** altera entregas já confirmadas (snapshot — ver R14).

## Frequência e datas
- **R6.** `frequência ∈ {semanal(7), quinzenal(14), mensal(~30), personalizada(N dias)}`.
- **R7.** Frequência `personalizada` exige `frequencia_dias` preenchido.
- **R8.** Ao **confirmar** uma entrega: `proxima_data_entrega = data_prevista + intervalo(frequencia)`.
  - A base é a **data prevista** (não a data real), para não acumular atrasos.
- **R9.** O recálculo respeita o **calendário operacional**: datas que caem em dia sem entrega são empurradas para o próximo dia válido.
- **R10.** A `proxima_data_entrega` só avança **na confirmação** — o pet nunca "pula" uma entrega não realizada.

## Estoque (Receitas da Casa)
- **R11.** O estoque controla **apenas quantidade** (sem lote/validade).
- **R12.** `saldo_fisico` nunca é negativo; toda saída gera `MovimentoEstoque`.
- **R13.** **Disponível = saldo_fisico − reservas ativas.** Nunca calcular fora dessa fórmula.
- **R14.** Ao **agendar** uma entrega com item de casa, cria-se `ReservaEstoque` (compromisso lógico, não baixa física).
- **R15.** Ao **confirmar** a entrega, a reserva vira `consumida` e nasce o movimento de saída.
- **R16.** Ao **cancelar** a entrega, as reservas ativas viram `liberada`; não há recálculo de data.
- **R17.** Filosofia "não pode faltar": mantém-se **estoque mínimo** por produto. O planejamento repõe o mínimo antes da janela consumi-lo.

## Produção personalizada
- **R18.** Status transita apenas `produzir → pronta_para_entrega` e volta a `produzir` na confirmação da entrega.
- **R19.** O status vive na própria Receita Personalizada (1 por pet), indexado por `ciclo_ref`.
- **R20.** A personalizada **não** toca estoque nem reserva — é puxada pela próxima entrega do pet.
- **R21.** Clonar uma personalizada de um pet para outro cria uma **nova** receita personalizada (cópia da ficha que depois é ajustada). Não há compartilhamento.

## Snapshot e histórico
- **R22.** `ItemEntrega` guarda receita/produto, descrição e quantidade no momento da geração; é **imutável** após a confirmação e sobrevive a exclusões de cadastro.

## Receitas / ingredientes
- **R23.** A ficha técnica (`ItemReceita*`) define o consumo teórico de ingrediente por receita.
- **R24.** O `FatorCorrecao` é aplicado ao calcular o consumo real na produção (quantidade × fator), usando o fator vigente na data.

## Confirmação de entrega (guard)
- **R25.** Uma entrega com itens personalizados **só pode ser confirmada se todas as suas personalizadas estiverem `pronta_para_entrega`**. Caso contrário, o sistema sinaliza (`tem_pendencia=true`) e bloqueia a confirmação.
- **R26.** A confirmação é **transacional**: ou todos os efeitos ocorrem (baixa de estoque, reset de personalizada, recálculo de data, evento), ou nenhum.

## Financeiro
- **R27.** Financeiro é **básico/cadastral**: `valor_plano`, `situacao`, `obs_financeira`. Sem cobranças/pagamentos nesta fase.

## Acesso
- **R28.** Apenas usuários autenticados operam o sistema.
- **R29.** Baixa de estoque e confirmação/cancelamento de entrega exigem papel Operação ou Admin.

## Integridade estrutural
- **R30.** Cadastros com histórico (cliente, pet, receita, produto) não são apagados fisicamente — usa-se `ativo=false`.
- **R31.** `horizonte_reserva_dias ≥ horizonte_producao_dias`.
