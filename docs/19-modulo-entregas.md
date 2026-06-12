# 19 — Módulo de Entregas

> Status: **modelagem aprovada**. Fase 1 (backend núcleo) autorizada.
> A **Entrega é a demanda oficial por data** e alimentará, no futuro, Produção,
> Estoque e Central Operacional.

## 1. Conceito
- A **entrega pertence ao Cliente**: uma entrega = uma **data** para um cliente, com
  vários pets e seus itens/receitas/pacotes. **Nunca** uma entrega por pet.
- Hierarquia: `Cliente → Entrega → EntregaPet → EntregaItem → (EntregaItemPacote | EntregaItemIngrediente)`.
- A entrega guarda **snapshot** (retrato no momento da geração) + **referências** (ids).

## 2. Entidades (Fase 1)

### 2.1 `Entrega` (cabeçalho + snapshot do cliente)
- Id, ClienteId (FK Restrict), DataPrevista (date), Status (enum §5)
- Snapshot: ClienteNome, Telefone, Rua, Numero, Complemento, Cep, Bairro, Cidade,
  Estado, FrequenciaNome, DiasCiclo
- ObservacoesInternas, ObservacoesEntregador
- EntregadorId (long?, **sem FK na Fase 1** — Entregador chega na fase de Rotas)
- **Não entregue**: MotivoNaoEntrega
- **Reagendamento**: ReagendadaDeId, ReagendadaParaId, MotivoReagendamento, ReagendadaEm
- **Cancelamento**: MotivoCancelamento, CanceladaEm
- Auditoria (CreatedAt/UpdatedAt)
- **RotaId não é persistido** — a relação virá de `RotaParada` (fase de Rotas).

### 2.2 `EntregaPet`
- Id, EntregaId (FK cascade), PetId (FK Restrict)
- Snapshot: PetNome, TipoAlimentacao (Casa/Personalizada), GramasDia, QuantidadeTotalGramas

### 2.3 `EntregaItem` (por receita do pet)
- Id, EntregaPetId (FK cascade), ReceitaId (FK Restrict)
- Snapshot: ReceitaCodigo, ReceitaNome, Tipo
- **Casa**: QuantidadeCicloGramas (necessário) + filhos `EntregaItemPacote`
- **Personalizada**: TamanhoPacoteGramas + QuantidadePacotes + filhos `EntregaItemIngrediente`

### 2.4 `EntregaItemPacote` (só Casa — snapshot dos pacotes)
- Id, EntregaItemId (FK cascade), TamanhoLabel (ex.: "500 g"), PesoGramas, Quantidade
- Permite derivar o **total real enviado** = Σ(Quantidade × PesoGramas).

### 2.5 `EntregaItemIngrediente` (só Personalizada — snapshot p/ Produção)
- Id, EntregaItemId (FK cascade), IngredienteId (FK Restrict)
- Snapshot: IngredienteNome, Categoria, GramasCozidas, CustoKgCru, Coeficiente
- Permite, no futuro, calcular ingredientes da produção (cozido→cru) e custo, de forma estável.

### 2.6 `EntregaHistorico` (append-only)
- Id, EntregaId (FK cascade), Quando (timestamp), Usuario, Evento (texto), StatusDe?, StatusPara?

## 3. Relacionamentos
```
Cliente 1—N Entrega
Entrega 1—N EntregaPet 1—N EntregaItem 1—N EntregaItemPacote        (Casa)
                                       └─ 1—N EntregaItemIngrediente (Personalizada)
Entrega 1—N EntregaHistorico
```
Agregado interno = Cascade; FKs externas (Cliente/Pet/Receita/Ingrediente) = Restrict.

## 4. Geração de entregas
- **Automática** (sem botão): disparada ao salvar/alterar dados relevantes
  (cliente, pet, plano, receita personalizada, receita da casa). Best-effort.
- Horizonte padrão: **45 dias** (configurável internamente).
- Fonte: clientes **ativos** com `FrequenciaEntregaId` + `PrimeiraEntrega` e ≥1 pet ativo
  com **plano ativo**.
- Datas ancoradas em `PrimeiraEntrega`, passo `DiasCiclo`, dentro de `[hoje, hoje+45]`.
- **Idempotente**: não cria se já existe entrega para `(cliente, data)` em status ativo
  (qualquer status exceto Cancelada/Reagendada).
- Independe da confirmação da entrega anterior.
- Cada entrega copia o **snapshot** atual (cliente + pets + planos + receitas + ingredientes/pacotes).
- Endpoint manual `POST /api/entregas/gerar` permanece apenas para manutenção/admin.

## 5. Status e transições
`Programada · ConfirmadaCliente · SaiuParaEntrega · Entregue · NaoEntregue · Reagendada · Cancelada`
```
Programada → ConfirmadaCliente → SaiuParaEntrega → Entregue
        ↘ NaoEntregue / Reagendada / Cancelada
```
- **NaoEntregue / Reagendar / Cancelar exigem motivo obrigatório.**
- **Ativas** (operacional) = todas, exceto Cancelada e Reagendada.

## 6. Dois tipos de mudança de data (importante)

### 6.1 Reagendamento pontual (uma entrega)
- Muda só aquela entrega; **não** altera cadastro/frequência do cliente nem as próximas.
- Original → status **Reagendada** (motivo, usuário, data/hora, `ReagendadaParaId`).
- Cria **nova** entrega na nova data (Programada), `ReagendadaDeId` → original, snapshot copiado.
- Histórico: "Entrega reagendada pontualmente de 10/06 para 11/06".

### 6.2 Alteração da agenda futura do cliente
- Muda o **padrão** do cliente: atualiza `FrequenciaEntregaId`/`PrimeiraEntrega` no Cliente.
- **Regerar** as entregas futuras ainda **não operacionais** com base no novo padrão.
- Entregas passadas ou já operacionais **não** são alteradas.
- Histórico: "Agenda futura do cliente alterada: entregas passaram para quinta a partir de 12/06".

## 7. Entregas alteráveis automaticamente (regeração)
Pode atualizar/regerar **apenas** entregas:
- `Programada`, **não confirmadas**, **sem rota**, **não despachadas**.

Não alterar automaticamente: `ConfirmadaCliente`, `SaiuParaEntrega`, `Entregue`,
`NaoEntregue`, `Reagendada`, `Cancelada` ou em rota. (Na Fase 1 não há rota, então o
critério é status + data futura.)

> Serviço `RegerarFuturasDoClienteAsync(clienteId)` remove as `Programada` futuras
> elegíveis e recria a partir do plano/agenda atuais. Botão futuro: "Regerar próximas
> entregas". Trocar o Plano de um pet ou a agenda do cliente poderá disparar isso.

## 8. Snapshot (estável para Produção/Estoque)
- **Casa**: receita, tipo, quantidade necessária no ciclo, pacotes (peso + quantidade por
  tamanho) → total real enviado derivável.
- **Personalizada**: receita, tipo, **ingredientes do momento** (nome, gramas cozidas,
  custo/kg cru, coeficiente), tamanho do pacote, quantidade de pacotes → total enviado e
  ingredientes de produção deriváveis.
- Totais/custos continuam **derivados**, mas os **insumos do cálculo ficam gravados**.

## 9. Elegibilidade para rota (fase futura)
- Elegíveis: `Programada`, `ConfirmadaCliente`.
- Fora da montagem: `Cancelada`, `Reagendada`, `NaoEntregue`, `Entregue`.
- Calendário/lista mostram todos os status; a **rota** só trabalha com elegíveis.

## 10. Histórico (append-only)
Registrar criação, confirmação, mudanças de status, reagendamento pontual, alteração de
agenda futura, cancelamento, não entregue, entrada/saída de rota (futuro).

## 11. Endpoints (Fase 1)
- `POST /api/entregas/gerar` (horizonteDias=90) → gera as entregas.
- `GET  /api/entregas` (filtros: data, status, clienteId, bairro, cidade) → lista.
- `GET  /api/entregas/{id}` → detalhe completo (pets/itens/pacotes/ingredientes/histórico).
- `PUT  /api/entregas/{id}/status` → transições simples (confirmar, saiu, entregue).
- `PUT  /api/entregas/{id}/nao-entregue` (motivo).
- `PUT  /api/entregas/{id}/reagendar` (novaData, motivo) → **"Somente esta entrega"** (pontual).
- `PUT  /api/entregas/{id}/alterar-agenda` (novaData, frequenciaEntregaId?, motivo) → **"Esta e próximas"**
  (ajusta a agenda do cliente e regera as futuras elegíveis).
- `PUT  /api/entregas/{id}/cancelar` (motivo).
- `POST /api/clientes/{id}/regerar-entregas` → regera futuras elegíveis (manutenção).

## 12. Alimentação futura (Produção/Estoque/Central)
- **Produção** agrega entregas de um período → soma pacotes/receitas → expande em
  ingredientes (Personalizada via snapshot; Casa via ficha da receita) para a ordem.
- **Estoque** sabe o que será consumido nas próximas entregas (produto acabado).
- **Central** mostra próximas entregas + pendências.

## 13. Cuidados técnicos
- Snapshot + referência; geração idempotente (dedup `(cliente, data)` ativa).
- Transições validadas no servidor; motivos obrigatórios onde exigido.
- Nunca apagar entregas operacionais — cancelar/reagendar; histórico append-only.
- `DataPrevista` = `date`; agenda reaproveita `CalculadoraAgenda`.
- Cascade só no agregado; FKs externas Restrict.
- `EntregadorId` sem FK na Fase 1 (FK entra com o cadastro de Entregadores/Rotas).
- Índices: `(cliente_id, data_prevista)`, `data_prevista`, `status`.

## 14. Fases
1. **Backend núcleo** (autorizado): entidades, EF, migration, services, DTOs, endpoints,
   geração, snapshot, status/transições, histórico, reagendamento pontual, cancelamento,
   não entregue, estrutura p/ alteração de agenda futura e regeração.
2. Tela principal (lista + filtros) + calendário mensal.
3. Detalhe da entrega + ações de status.
4. Entregador (cadastro) + Rota (montagem manual) + despacho.
5. Imagem da rota (frontend, PNG).
6. Integração Produção/Estoque/Central.
