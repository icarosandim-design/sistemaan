# 1. Visão geral do negócio

## Contexto

Empresa de **alimentação natural para cães** com aproximadamente **150 clientes
ativos**. Cada cliente pode possuir vários pets. A operação produz comida e
entrega aos clientes em uma frequência recorrente.

## Dois tipos de alimentação

| Tipo | Características | Controle |
|---|---|---|
| **Receitas da Casa** | Padronizadas, produzidas em **lote**, armazenadas em **estoque** de produto acabado. Ex.: Frango 250g, Bovina 250g. | Estoque (saldo, movimentos, reservas) |
| **Receitas Personalizadas** | Criadas para um **pet específico**, produzidas **sob demanda**, sem estoque. Ex.: VET-001. | Status (`Produzir` → `Pronta para Entrega`) |

### Esclarecimento importante sobre receitas personalizadas

Uma receita personalizada **pertence a um único pet**. Dois pets podem ter
receitas com os mesmos ingredientes, porém a **gramatura de cada ingrediente é
diferente por pet** — portanto são receitas distintas. Um mesmo pet pode ter
**mais de uma** receita personalizada.

> Isso corrige a premissa inicial de que uma personalizada poderia ser
> compartilhada entre pets. **Não há compartilhamento.**

## Plano alimentar

O plano alimentar de um pet é composto por **itens**. Cada item é uma receita
(da casa ou personalizada) com uma **quantidade de pacotes**.

Exemplos:

- **Thor:** 8 pacotes de Frango 250g + 7 pacotes de Bovina 250g (Receitas da Casa).
- **Nina:** 8 pacotes da receita VET-001 + 7 pacotes da receita VET-002 (Personalizadas).

## Entrega recorrente

Cada pet possui:

- Uma **frequência de entrega**: semanal, quinzenal, mensal ou personalizada (N dias).
- Uma **próxima data de entrega**.

Quando uma entrega é **confirmada**, o sistema **recalcula automaticamente** a
próxima data de entrega do pet. Se a alimentação for personalizada, o status da
receita volta para **"Produzir"** (preparando o próximo ciclo).

## Filosofia operacional: "não pode faltar"

A operação trabalha para **prevenir a falta antecipadamente**, não para
administrar a falta na hora da entrega:

- **Casa:** mantém-se **estoque mínimo** por produto; a produção repõe o mínimo
  antes que a demanda da janela o consuma.
- **Personalizada:** deve estar **pronta antes da entrega**. Se não estiver, o
  sistema **sinaliza** e não permite confirmar a entrega.

## O que o sistema controla

- Clientes
- Pets
- Dados financeiros **básicos** dos clientes (cadastral: valor do plano, situação, observações — sem contas a receber)
- Receitas da Casa
- Receitas Personalizadas
- Ingredientes
- Fatores de correção dos ingredientes
- Estoque de produtos acabados das Receitas da Casa
- Planejamento de produção
- Entregas
- Calendário operacional

## Escopo da primeira fase

- Financeiro **apenas cadastral** (não há módulo de cobrança/pagamento).
- Estoque controla **apenas quantidade** (sem lote/validade/FEFO).
- Foco em **corretude operacional**, não em escala (volume é pequeno).
