# 6. Máquina de estados

Duas máquinas de estado governam a operação. Elas se acoplam num único momento:
a **confirmação da entrega**.

## 6.1. Entrega

```
   AGENDADA ──────────────► CONFIRMADA   [terminal]
      │   guard:                 ▲
      │   • todas personalizadas │
      │     do pet = PRONTA      │
      │   • casa coberta por     │
      │     estoque/ reserva     │
      │                          │
      ├── se personalizada NÃO pronta → bloqueia + sinaliza (tem_pendencia)
      │
      └──────────► CANCELADA   [terminal]
```

| Estado | Significado |
|---|---|
| **agendada** | Entrega prevista; itens de casa reservados; personalizadas em produção. |
| **confirmada** | Entregue. Dispara baixa de estoque, reset de personalizada e recálculo de data. Terminal. |
| **cancelada** | Não ocorrerá; libera reservas; não recalcula data. Terminal. |

> Alerta paralelo (não-bloqueante de entrega): quando a demanda da janela faz um
> produto cair abaixo do `estoque_minimo`, o produto entra na lista
> "o que produzir". A filosofia "não pode faltar" trata isso **antes** da entrega.

### Transições e efeitos

| De → Para | Guard | Efeitos |
|---|---|---|
| (job) → agendada | há disponível para reservar | cria reservas; eventos `EntregaAgendada`, `ReservaCriada` |
| agendada → confirmada | todas personalizadas `pronta` | baixa estoque + consome reservas; personalizadas → `produzir` (+ novo ciclo); recalcula `proxima_data_entrega`; eventos `EntregaConfirmada`, `ProximaDataRecalculada` |
| agendada → cancelada | permissão Operação/Admin | libera reservas; evento `EntregaCancelada` |

Regra de integridade: a confirmação é **transacional** — todos os efeitos ou nenhum.

## 6.2. Receita Personalizada

```
        ┌─────────────────────────────────┐
        │                                 │
        ▼                                 │
    PRODUZIR ───(equipe finaliza)──► PRONTA_PARA_ENTREGA
        ▲                                 │
        │   (entrega do pet confirmada:    │
        │    reset + ciclo_ref avança)     │
        └─────────────────────────────────┘
```

| Estado | Significado |
|---|---|
| **produzir** | Precisa ser fabricada para o ciclo atual. Aparece em "o que produzir". |
| **pronta_para_entrega** | Fabricada, aguardando a entrega do pet. Aparece em "o que está pronto". |

### Transições

| De → Para | Gatilho | Efeitos |
|---|---|---|
| produzir → pronta_para_entrega | equipe marca pronta | evento `StatusPersonalizadaAlterado` |
| pronta_para_entrega → produzir | **confirmação da entrega do pet** | `ciclo_ref` avança; evento `StatusPersonalizadaAlterado` (motivo: novo ciclo) |

Notas:
- A personalizada **não** passa por estoque/reserva.
- `ciclo_ref` permite distinguir "pronta do ciclo passado" de "produzir do próximo".
- Pet/receita desativada (`ativa=false`) some das listas; não há estado "cancelada".
