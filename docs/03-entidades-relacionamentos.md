# 3. Entidades e relacionamentos

## Entidades principais

### Clientes & Pets
- **Cliente** — dados cadastrais + financeiros básicos.
- **Pet** — pertence a um cliente; tem frequência e próxima data de entrega.
- **PlanoAlimentar** — plano vigente de um pet (histórico mantido).
- **ItemPlanoAlimentar** — uma linha do plano: aponta para um **Produto** (Casa) ou uma **Receita Personalizada**, com quantidade de pacotes.

### Catálogo / Receitas
- **Ingrediente**
- **FatorCorrecao** — fator de perda/rendimento do ingrediente (versionado por data).
- **ReceitaCasa** — padronizada, sem dono.
- **Produto** — unidade entregável/estocável de uma Receita da Casa (ex.: Frango 250g). Uma receita gera N produtos.
- **ReceitaPersonalizada** — pertence a **um** pet; tem status próprio e ciclo.
- **ItemReceitaCasa** / **ItemReceitaPersonalizada** — ficha técnica (ingrediente + gramatura) de cada tipo de receita.

### Estoque
- **SaldoEstoque** — saldo físico atual por produto (derivado dos movimentos).
- **MovimentoEstoque** — livro-razão de entradas/saídas/ajustes.
- **ReservaEstoque** — quantidade comprometida para uma entrega futura.

### Produção
- **OrdemProducao** — o que produzir (lote para casa, sob demanda para personalizada).
- **ItemOrdemProducao** — linhas da ordem (produto ou receita personalizada).

### Entregas
- **Entrega** — agendada para um pet, com data prevista.
- **ItemEntrega** — snapshot imutável do que foi/será entregue.
- **CalendarioOperacional** — dias de produção e de entrega (feriados/folgas).

### Transversais
- **Usuario**, **Papel** — identidade e RBAC.
- **Configuracao** — horizontes e políticas operacionais.
- **EventoDominio** — timeline de fatos relevantes (auditoria).

## Relacionamentos

```
Cliente 1 ──< Pet
                │
                ├── 1 ──< ReceitaPersonalizada   (dono: 1 pet; status próprio; sob demanda)
                │           └─ 1 ──< ItemReceitaPersonalizada (gramatura do pet) >── Ingrediente
                │
                ├── 1 PlanoAlimentar (vigente) 1 ──< ItemPlano
                │                                      ├──> Produto                (se Casa)
                │                                      └──> ReceitaPersonalizada   (se Personalizada)
                │
                └── 1 ──< Entrega 1 ──< ItemEntrega (snapshot)

ReceitaCasa 1 ──< Produto 1 ── SaldoEstoque
                      │         1 ──< MovimentoEstoque
                      │         1 ──< ReservaEstoque >── Entrega
ReceitaCasa 1 ──< ItemReceitaCasa >── Ingrediente 1 ── FatorCorrecao

OrdemProducao 1 ──< ItemOrdemProducao ──> Produto | ReceitaPersonalizada
Entrega / OrdemProducao ──respeitam──> CalendarioOperacional
```

## Cardinalidades-chave

| Relação | Cardinalidade | Observação |
|---|---|---|
| Cliente → Pet | 1 : N | |
| Pet → ReceitaPersonalizada | 1 : N | personalizada tem dono único |
| Pet → PlanoAlimentar | 1 : N (1 vigente) | unicidade parcial garante 1 vigente |
| PlanoAlimentar → ItemPlano | 1 : N | |
| ItemPlano → Produto **ou** ReceitaPersonalizada | N : 1 (exclusivo) | exatamente uma das duas FKs |
| ReceitaCasa → Produto | 1 : N | gramaturas/embalagens diferentes |
| Produto → ReservaEstoque | 1 : N | |
| Entrega → ReservaEstoque | 1 : N | uma reserva por produto por entrega |
| Pet → Entrega | 1 : N | materializadas dentro do horizonte |

## Decisões de relacionamento

- **Receita personalizada por pet** (não compartilhada) — simplifica o status e a ficha técnica individualizada.
- **Receita da Casa separada de Produto** — uma receita pode ter várias embalagens; o estoque vive no Produto.
- **Item de plano polimórfico via duas FKs exclusivas** (Produto XOR Personalizada) — preserva integridade referencial real.
- **Ficha técnica em duas tabelas** (casa/personalizada) — FK real para cada tipo de receita.
