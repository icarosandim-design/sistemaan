# 2. Módulos do sistema

O sistema é um **monólito modular**: módulos com fronteiras claras dentro de uma
única solução .NET (ver [Decisões arquiteturais](08-decisoes-arquiteturais.md)).

| Módulo | Responsabilidade |
|---|---|
| **Clientes & Pets** | Cadastro de clientes, dados financeiros básicos, pets, planos alimentares e parâmetros de entrega. |
| **Catálogo / Receitas** | Receitas da Casa, Receitas Personalizadas, produtos acabados, ingredientes e fatores de correção. |
| **Estoque** | Saldo de produtos acabados (Casa), movimentos e reservas. Controla apenas quantidade. |
| **Produção** | Planejamento e ordens de produção (lote para casa, sob demanda para personalizadas) e status das personalizadas. |
| **Entregas** | Materialização, agendamento, confirmação, cancelamento, recálculo de próxima data e calendário operacional. |
| **Identidade & Acesso** | Usuários da equipe, autenticação JWT e papéis (Admin, Operação, Entrega). |
| **Painel Operacional** | Visões agregadas (produzir / pronto / entregar / em estoque / reservado). É **leitura** sobre os demais módulos, não uma entidade própria. |

## Fronteiras e acoplamento

O ponto mais delicado do domínio é o acoplamento entre **Estoque, Produção e
Entregas**, conectados pelo conceito de **reserva**:

```
Entrega (agendada) ──cria──► Reserva (compromisso lógico)
Planejamento ──lê──► demanda das entregas ──gera──► Produção
Entrega (confirmada) ──consome──► Reserva + baixa Estoque
```

A confirmação de entrega é o único ponto onde as três máquinas/módulos se tocam
de forma transacional (ver [Fluxos](05-fluxos-operacionais.md) e
[Máquina de estados](06-maquina-de-estados.md)).

## Papéis de acesso (RBAC enxuto)

| Papel | Pode |
|---|---|
| **Admin** | Tudo, incluindo cadastros e configurações. |
| **Operação** | Produção, estoque, confirmar/cancelar entregas, planejamento. |
| **Entrega** | Visualizar roteiro do dia e confirmar entregas. |
