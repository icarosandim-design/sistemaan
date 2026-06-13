# 22 — Módulo de Produção

> Status: **proposta para aprovação**. Nenhum código/migration/tela ainda.
> Conecta Entregas, Estoque, Receitas (Casa/Personalizada), Ingredientes, Produto
> Acabado e Prontidão. Reaproveita os padrões já construídos (snapshot, máquina de
> status, ledger de estoque, `OrdemProducaoId` já previsto na movimentação).

## 0. Princípios
- **Conexão por id** (`IngredienteId`, `ItemEstoqueId`, `EntregaItemId`, `ReceitaId`, `TamanhoPacoteId`) — nunca por nome.
- **Snapshot**: a ficha guarda o retrato da receita/ingredientes no momento em que entra na produção (editar a receita depois não muda uma produção já montada).
- **Demanda é consulta** (calculada de Entregas + Estoque ao vivo); só vira dado salvo quando o operador inclui na Ordem do dia.
- **Uma Ordem de Produção por dia** (igual "uma entrega por cliente/dia").
- **Planejado = previsão; baixa real = na finalização** (quantidade informada pela cozinha).

## 1. Decisões aprovadas
1. Submenus: **Planejar produção · Produção do dia · Cozinha · Histórico**. O Planejar pode montar **vários dias à frente** e **editar** (remover/adicionar cliente).
2. Pesagem cru/cozido **consolidada por ingrediente** (você cozinha tudo junto); **envase por ficha**.
3. Produto acabado da Casa entra **sem custo** nesta fase (custo 0; custo vem em fase futura).
4. **Autorizada** a migration em Entregas para guardar a **prontidão** da personalizada (`entrega_itens`).
5. Falta de estoque na finalização → **alerta + exige entrada/ajuste** do ingrediente (não bloqueia o resto). O estoque nunca fica negativo.
6. Ordem = **dia em que se cozinha**; o operador pode **olhar entregas mais à frente** para adiantar.
7. Impressão **separada**: Mapa de Produção e Fichas Técnicas (e Resumo final), cada um por conta.

---

## 2. Entidades

### 2.1 `OrdemProducao` (uma por dia)
- Id, **Data** (DateOnly, **única**), Status (enum §3)
- Observacoes
- Finalização: FinalizadaEm (timestamp?), FinalizadaPor (string?), TudoProduzido (bool?), ObservacoesFinalizacao
- Auditoria (CreatedAt/UpdatedAt)
- Coleções: `Fichas`, `Consumos`

### 2.2 `FichaProducao` (por receita/pet) — filha de OrdemProducao (cascade)
- Id, OrdemProducaoId (FK cascade)
- Tipo (TipoReceita: Casa | Personalizada)
- **Personalizada** (id-refs): EntregaId, EntregaPetId, EntregaItemId, PetId, ClienteId (FK Restrict)
- **Casa**: ReceitaId, TamanhoPacoteId, ItemEstoqueId (produto acabado de destino) (FK Restrict)
- Snapshot: ClienteNome, PetNome, ReceitaCodigo, ReceitaNome, DataEntrega (DateOnly?)
- Planejado: QuantidadePacotes, PesoPacoteGramas, QuantidadeTotalGramas (bacia)
- Status (enum §3), MotivoNaoFeita
- Real (na conclusão): QuantidadePacotesReal, PesoEnvasadoGramas, ConcluidaEm, ConcluidaPor, Observacoes
- Filhos: `Ingredientes` (FichaProducaoIngrediente)

### 2.3 `FichaProducaoIngrediente` (snapshot) — filha de FichaProducao (cascade)
- Id, FichaProducaoId, IngredienteId (FK Restrict)
- Snapshot: IngredienteNome, Categoria, GramasCozidas, Coeficiente
- (espelha `EntregaItemIngrediente`; para personalizada vem **direto desse snapshot da entrega**; para Casa vem da `Receita.Itens` rateado pelos pacotes/peso)

### 2.4 `ConsumoIngredienteProducao` (consolidado por ingrediente) — filho de OrdemProducao (cascade)
- Id, OrdemProducaoId, IngredienteId (FK Restrict), IngredienteNome (snapshot)
- ItemEstoqueId (long?, insumo vinculado) + ItemEstoqueNome (snapshot) — **null = pendência** "sem item vinculado"
- Coeficiente (snapshot), UnidadeEstoque (snapshot, p/ conversão)
- Planejado: PlanejadoCozidoGramas, PlanejadoCruGramas (estimado via coeficiente)
- Real (finalização): RealCruGramas, RealCozidoGramas
- BaixaRealizada (bool), Observacao

### 2.5 Prontidão na Entrega — **migration aditiva em `entrega_itens`**
- StatusPreparo (enum `StatusPreparoPersonalizada` — já existe: NaoPronta/ParcialmentePronta/Pronta, default NaoPronta)
- PacotesProntos (int?), PreparadoEm (timestamp?), PreparadoPor (string?)
- A conclusão da ficha personalizada **escreve aqui**; a tela de Entregas passa a **ler** (substitui o mock).

### 2.6 (opcional) `ProducaoHistorico` (append-only)
- Id, OrdemProducaoId, Quando, Usuario, Evento, StatusDe?, StatusPara? — para auditoria das transições. Pode ficar para fase posterior.

---

## 3. Enums
- `StatusOrdemProducao`: **Planejada · EmAndamento · Finalizada**
- `StatusFichaProducao`: **Pendente · EmPreparo · Produzida · Envasada · Conferida · NaoFeita**
  - Mapa da fila da cozinha: A fazer=Pendente · Em preparo=EmPreparo · Pronta p/ envase=Produzida · Envasando=Envasada · Concluída=Conferida · Não feita=NaoFeita
- Reuso: `StatusPreparoPersonalizada`, `TipoReceita`, `TipoMovimentacao.SaidaProducao/EntradaProducao`, `OrigemLote.Producao`.

---

## 4. Relacionamentos
```
OrdemProducao 1—N FichaProducao 1—N FichaProducaoIngrediente
OrdemProducao 1—N ConsumoIngredienteProducao
FichaProducao(Personalizada) —→ Entrega / EntregaPet / EntregaItem / Pet / Cliente   (Restrict)
FichaProducao(Casa)          —→ Receita / TamanhoPacote / ItemEstoque(acabado)        (Restrict)
ConsumoIngredienteProducao   —→ Ingrediente (Restrict) · ItemEstoque(insumo, opcional)
Finalização escreve: MovimentacaoEstoque (OrdemProducaoId já existe) + EntregaItem.StatusPreparo
```
Agregado interno (Ordem→Fichas→Ingredientes, Ordem→Consumos) = **Cascade**; FKs externas = **Restrict**.

---

## 5. Fluxo de planejamento
1. Operador abre **Planejar**, escolhe a janela: Hoje · Amanhã · 3 dias · 7 dias · Próxima semana · (e pode espiar mais à frente).
2. Sistema consulta **Entregas ativas** (status ≠ Cancelada/Reagendada) na janela.
3. **Demanda calculada (read):**
   - **Personalizadas:** cada `EntregaItem` tipo Personalizada com `StatusPreparo ≠ Pronta`, ordenado por **data de entrega** (mais próxima primeiro), depois Parcialmente prontas. Topo da lista.
   - **Casa:** soma de pacotes necessários por **(receita, tamanho)** na janela (de `EntregaItemPacote`) **×** saldo do `ItemEstoque(ProdutoAcabadoCasa)`. **Falta = máx(0, necessário − saldo)**. Sugere produção **só quando falta > 0**.
4. **Prioridade da tela:** (1) entregas mais próximas → (2) personalizadas não prontas → (3) personalizadas parciais → (4) Casa com falta → (5) itens incluídos manualmente.
5. Operador **seleciona** o que produzir → cria/edita a **Ordem do dia** escolhido (padrão hoje). Cada seleção vira `FichaProducao` (1 por `EntregaItem` personalizada; Casa por receita+tamanho+qtd a produzir). Snapshot dos ingredientes.
6. **Edição** livre até finalizar: adicionar/remover fichas (remover cliente por algum motivo, adicionar outro). Recalcula a consolidação.

## 6. Consolidação de ingredientes (cozido × cru)
- Para cada ingrediente, soma `GramasCozidas` de **todas** as fichas → **PlanejadoCozido**.
- **PlanejadoCru = PlanejadoCozido ÷ Coeficiente** (coeficiente = cozido÷cru; "sem conversão" = 1).
- Resolve `ItemEstoque` por `IngredienteId` → **saldo** (convertido p/ a unidade do item) → **falta = máx(0, cru − saldo)**.
- Ingrediente **sem item vinculado** → pendência "Ingrediente sem item de estoque vinculado".
- Gera a **lista consolidada para a cozinha**: ingrediente · cozido · cru · saldo · alerta de falta (destaque vermelho/ícone).

## 7. Saídas para a cozinha
- **Mapa de produção (consolidado):** `ConsumoIngredienteProducao` (cozido, cru, saldo, alerta). Para separar/cozinhar.
- **Fichas técnicas (por receita/pet):** cliente · pet · receita (código/nome) · data de entrega · total da bacia · qtd de pacotes · peso por pacote · ingredientes + quantidades · observações/restrições. Para montagem e envase.

## 8. Tela da Cozinha (fila)
- Fichas agrupadas por status. A tela principal mostra só **pendentes/em andamento** (Pendente/EmPreparo/Produzida/Envasada).
- Ao concluir (Conferida) ou marcar Não feita, a ficha **sai da lista principal** e vai para aba **Concluídas / Não feitas** (não some).
- Cada avanço de status registra data/hora + usuário. **Envase exige** informar QuantidadePacotesReal e PesoEnvasado antes de ir para Envasada/Conferida (pesagem obrigatória).

## 9. Finalização da produção do dia
Ao finalizar, o operador responde/registra:
- Tudo previsto foi produzido? Quais **não feitas** e **motivo**.
- **Pesagem real consolidada por ingrediente:** RealCru, RealCozido (→ rendimento/perdas).
- Sobras e perdas (por ingrediente, quando houver).
Então o sistema executa, de forma **idempotente** (Status → Finalizada trava tudo):
1. **Baixa de insumos**: para cada `ConsumoIngrediente` com item vinculado e RealCru > 0 → `SaidaProducao` (FIFO, `OrdemProducaoId`), na unidade do item.
   - Se RealCru > saldo (opção A): **alerta** e pede **entrada/ajuste** daquele ingrediente; baixa fica pendente até resolver (não trava as demais).
2. **Produto acabado (Casa)**: para cada ficha Casa Conferida com PacotesReal > 0 → `EntradaProducao` no `ItemEstoque(ProdutoAcabadoCasa)`, quantidade = PacotesReal, **custo 0** (decisão 3).
3. **Prontidão (Personalizada)**: para cada ficha Personalizada Conferida → `EntregaItem.StatusPreparo = Pronta` (Parcial se PacotesReal < planejado), com PacotesProntos/PreparadoEm/Por.
4. Gera **Resumo final** (planejado × real, perdas, sobras, não feitas, motivos, ingredientes reais).

## 10. Rendimento e perdas
Por `ConsumoIngrediente`: PlanejadoCru, RealCru, PlanejadoCozido, RealCozido → **perda esperada** (cru − cozido esperado) × **perda real** (RealCru − RealCozido) e **diferença planejado×real**. Base para, no futuro, **refinar os coeficientes** dos ingredientes.

## 11. Integração com Entregas (leitura — substitui os mocks)
- **Casa:** necessário do dia (`EntregaItemPacote`) × saldo do `ItemEstoque(acabado)` → ok/falta.
- **Personalizada:** `EntregaItem.StatusPreparo` → pronta / não pronta / parcial.
> A troca dos mocks da tela de Entregas é uma etapa própria (consome esses dados); a Produção/Estoque é quem os alimenta.

## 12. Impressão / PDF
1. **Mapa de produção do dia** (consolidado de ingredientes + alertas).
2. **Fichas técnicas** (uma por receita/pet).
3. **Resumo final** (planejado×produzido, perdas, sobras, não feitas, motivos).
- **v1:** páginas **print-friendly** no frontend (botões separados p/ Mapa e Fichas — não dependem de TV) + "imprimir/Salvar PDF" do navegador. PDF gerado no backend pode vir depois.

## 13. Endpoints (proposta)
| Método | Rota | Uso |
|---|---|---|
| GET | `/api/producao/demanda?inicio=&fim=` | Demanda calculada (personalizadas pendentes + Casa com falta) |
| GET | `/api/producao?data=` | Ordem do dia (ou 404 se não existe) |
| POST | `/api/producao` | Cria/abre a Ordem do dia |
| GET | `/api/producao/{id}` | Ordem completa (fichas + consolidado + alertas) |
| POST | `/api/producao/{id}/fichas` | Adiciona fichas (seleção do planejamento) |
| DELETE | `/api/producao/fichas/{fichaId}` | Remove ficha (antes de finalizar) |
| PUT | `/api/producao/fichas/{fichaId}/status` | Avança status (cozinha) |
| PUT | `/api/producao/fichas/{fichaId}/concluir` | PacotesReal + envasado + obs |
| PUT | `/api/producao/{id}/consumo` | Pesagem real por ingrediente (cru/cozido) |
| POST | `/api/producao/{id}/finalizar` | Baixa insumos + acabado + prontidão + resumo |
| GET | `/api/producao` | Histórico de ordens |
| GET | `/api/producao/{id}/resumo` | Resumo final (para tela/PDF) |

## 14. Telas
- **Planejar produção** (demanda por janela, seleção, montar/editar ordem; janela ampliável).
- **Produção do dia** (painel estilo Central: status, total/pendentes/concluídas de fichas, consolidado, alertas, imprimir, finalizar).
- **Cozinha** (fila por status; principal = pendentes/andamento; aba concluídas/não feitas).
- **Histórico** (ordens passadas + resumo + reimpressão).
- **Views de impressão:** Mapa de produção · Fichas técnicas · Resumo final.

## 15. Riscos técnicos
1. **Migration em Entregas** (`entrega_itens` prontidão) — aditiva, autorizada; e migrations do módulo (escritas à mão + snapshot, com o `Ignore(PendingModelChangesWarning)` já ativo).
2. **Conversão de unidade** (g ↔ unidade do item, ex. kg) e **cru↔cozido** — precisão decimal; converter na borda da baixa.
3. **"Uma por dia"** — índice único em `data`; concorrência tratada.
4. **Edição × finalização** — Ordem editável até Finalizada; depois travada; baixa **idempotente** (`BaixaRealizada`/Status).
5. **Entrega reagendada/cancelada após a ficha criada** — ficha pode ficar "órfã"; avisar no painel (não baixa prontidão de entrega inexistente).
6. **Falta de estoque na baixa** (opção A) — fluxo de resolver com entrada/ajuste; baixa pendente sinalizada.
7. **Snapshot vs real** — fichas/consumos guardam snapshot; relatórios usam o real informado.
8. **Produto acabado custo 0** nesta fase (combinado).

## 16. Plano de implementação por fases
- **Fase 1 — Modelagem** (este documento). 
- **Fase 2 — Backend planejamento + ordem:** entidades OrdemProducao/Ficha/FichaIngrediente/Consumo + enums + migration (produção) + migration prontidão em entregas; demanda (read), criar/editar ordem, consolidação. Validar build/migration/endpoints.
- **Fase 3 — Backend execução/finalização:** status de fichas, pesagem real, finalização (baixa de insumos, produto acabado, prontidão), resumo. Validar.
- **Fase 4 — Frontend Planejar + Produção do dia.**
- **Fase 5 — Frontend Cozinha (fila).**
- **Fase 6 — Impressões** (Mapa, Fichas, Resumo).
- **Fase 7 — Integração com Entregas** (substituir mocks de estoque/prontidão) + histórico/relatório de rendimento.

## 17. Fora de escopo desta v1
Reserva de estoque; custo do produto acabado; refino automático de coeficientes; financeiro; compras automáticas; multi-cozinha. (Ficam para depois.)
