# Diagnóstico Pré-Financeiro e Performance — Sistema Principal

> Documento gerado por análise **estática** do repositório (branch `claude/jolly-pascal-uz20jt`) em 2026-06-16.
> Objetivo: descrever o estado real do sistema principal para validar o prompt do futuro **módulo financeiro**.
> **Natureza:** somente diagnóstico. Nada foi implementado, corrigido, migrado, populado ou apagado. O ambiente de teste **não** foi analisado, usado nem alterado.

---

## 1. Resumo executivo

- **Estado geral:** sistema maduro e coeso, em **Clean Architecture .NET 9 + Angular 19**, com ~40 entidades, 28 controllers, 27 migrations e ~70 índices. O operacional (clientes, pets, planos, receitas, estoque, produção, entregas, rotas, central, relatórios) está **funcional e usando dados reais** (sem mocks ativos relevantes).
- **Estável?** Sim, do ponto de vista de arquitetura e build. O **frontend compila** (medido aqui). O **backend compila e sobe via Docker** (confirmado em sessão anterior pela API do ambiente de teste subindo e aplicando migrations). Não há módulo financeiro hoje.
- **Pronto para receber o financeiro?** **Parcialmente.** A base de dados operacional é boa (há valor recorrente no cliente, fornecedor com campos de pagamento, entrada de estoque com valor/frete). Porém **faltam dados financeiros estruturados** em 3 pontos críticos: (a) **Pedido PJ não grava preço** (`ValorTotal`/`PrecoUnitario` existem mas nunca são preenchidos); (b) **Venda Avulsa PF não tem entidade** — valor fica em texto livre na observação da entrega; (c) **Entrada de estoque não tem nota fiscal/vencimento/parcelamento**. Esses pontos precisam de ajuste **antes ou junto** do financeiro para evitar contas "fantasma" sem valor.
- **Principais riscos:** (1) endpoints sem paginação com `Include` profundos (Entregas, Produção/Demanda, Pets, Rotas) que podem ficar lentos quando a base crescer; (2) ausência de valor em Pedido PJ e Venda Avulsa (impede Conta a Receber automática confiável); (3) relatórios carregam o período inteiro em memória (aceitável p/ ~150 clientes, frágil além disso).
- **Principais gargalos:** `GET /entregas`, `GET /producao/demanda`, `GET /pets`, `GET /rotas/{id}`, `GET /central/resumo` (todos com includes aninhados de coleção e/ou agregação em memória).
- **Ajustes recomendados antes do financeiro:** ver §13. Em resumo: definir **valor real em Pedido PJ e Venda Avulsa**, decidir o modelo de **Conta a Receber/Pagar**, e adicionar alguns **índices compostos** (data+status).
- **O prompt financeiro está adequado?** O escopo descrito (assinatura→cobrança, venda avulsa→a receber, pedido PJ→a receber, entrada de estoque→a pagar, NF/boleto/conciliação) é **coerente, porém grande**. Recomendação: **dividir em fases** (ver §8 e §15). A Fase 1 deve focar em Contas a Receber/Pagar manuais + geração a partir do que já tem valor real (assinatura), deixando NF-e/boleto/conciliação para fases seguintes.

---

## 2. Confirmação do ambiente analisado

**Ambiente analisado:** apenas o **código-fonte do repositório** (que é a fonte da verdade para o ambiente principal e o de teste — ambos rodam o mesmo código; só o banco difere). A análise foi feita na cópia do repositório no ambiente de execução em nuvem.

**Ambiente principal (`docker-compose.yml`):**

| Item | Valor |
|---|---|
| Containers | `sistemaan-db`, `sistemaan-api`, `sistemaan-web` |
| Banco | PostgreSQL 16 — database `sistemaan`, user `sistemaan` |
| Volume | `pgdata` |
| Portas | DB `5432`, API `8080`, Web `4200` |
| Connection string | `Host=db;Port=5432;Database=sistemaan;Username=sistemaan;Password=***` |
| Seed de demonstração | **Desligado** (não há `Seed__DemoData` no compose principal → o `DemoDataSeeder` nunca roda no original) |

**Ambiente de teste (`docker-compose.test.yml`) — apenas identificado, NÃO usado:**

| Item | Valor |
|---|---|
| Project name | `sistemaan-test` |
| Containers | `sistemaan-test-db`, `sistemaan-test-api`, `sistemaan-test-web` |
| Volume | `pgdata_test` |
| Portas | DB `5433`, API `8081`, Web `4201` |
| Seed de demonstração | **Ligado** (`Seed__DemoData: "true"`) |

**Como foi garantido que o ambiente de teste não foi tocado:** nenhum comando `docker`, `psql`, `curl` ou de runtime foi executado contra qualquer ambiente em execução. A análise foi 100% estática (leitura de arquivos + 1 build de frontend isolado na pasta `frontend/`). Não foi usado `-p sistemaan-test`, nem `down -v`, nem qualquer comando destrutivo.

> **Limitação importante:** por estar no ambiente em nuvem, **não tenho acesso aos containers/banco em execução na sua máquina**. Portanto, **contagens de registros vivas e tempos de resposta da API não puderam ser coletados** — forneço os comandos/SQL seguros para você rodar localmente (§9 e §10) e não invento números.

---

## 3. Arquitetura geral

- **Backend:** .NET 9, **Clean Architecture** em 4 projetos:
  - `SistemaAN.Domain` — entidades, enums, regras de invariância (factories estáticas, setters privados, base `Entity`/`AuditableEntity`).
  - `SistemaAN.Application` — services por módulo, DTOs (records), interfaces, validações (`ValidationException`), `IApplicationDbContext`.
  - `SistemaAN.Infrastructure` — EF Core 9 + Npgsql, `ApplicationDbContext`, configurations (`IEntityTypeConfiguration`), migrations escritas à mão, seeders, segurança (JWT, Pbkdf2).
  - `SistemaAN.Api` — controllers REST, Swagger, health check, CORS, Serilog, tratamento global de erros (ProblemDetails).
- **Banco:** PostgreSQL 16, convenção **snake_case** (`UseSnakeCaseNamingConvention`), enums persistidos como string (`HasConversion<string>`). Migrations aplicadas no startup (`db.Database.MigrateAsync()`), seguidas dos seeders. O warning `PendingModelChangesWarning` é **ignorado** de propósito (migrations e snapshot são mantidos à mão neste projeto — não há `dotnet ef` no fluxo).
- **Frontend:** Angular 19 standalone, **signals**, Angular Material 19, control-flow (`@if/@for/@switch`), `inject()`, guards (`authGuard` + `papelGuard`). Gráficos do dashboard são **SVG escritos à mão** (sem biblioteca de chart). Em produção `apiUrl: '/api'` (relativo; nginx faz proxy para a API).
- **Docker:** 3 serviços (db/api/web). API aplica migrations + seeders no boot. Build do `web` faz `npm run build` dentro da imagem.
- **Autenticação:** JWT (access + refresh), claims de papel (`RoleClaimType`), senha com Pbkdf2. Interceptor no frontend injeta o token e faz refresh em 401.
- **Permissões:** 3 papéis fixos — **Administrador**, **Operador**, **Cozinha** (`PapeisDoSistema`). Autorização por `[Authorize(Roles=...)]` no backend **e** por `papeis` no menu/rotas do frontend (defesa em profundidade).
- **Padrões técnicos:** DTOs como `record`; services `Scoped`; CancellationToken propagado; soft-delete/inativação via flag `Ativo`; snapshots imutáveis em Entrega/Pedido (guardam nome/itens no momento da geração).

---

## 4. Estado atual dos módulos

| Módulo | Backend | Frontend | API real? | Mock? | Status | Riscos | Observações |
|---|---|---|---|---|---|---|---|
| Login/Autenticação | ✅ | ✅ | ✅ | Não | OK | Baixo | JWT + refresh + Pbkdf2 |
| Usuários e permissões | ✅ | ✅ (SO_ADMIN) | ✅ | Não | OK | Baixo | 3 papéis fixos |
| Clientes PF | ✅ | ✅ | ✅ | Não | OK | Médio | lista sem paginação |
| Clientes PJ | ✅ | ✅ | ✅ | Não | OK | Médio | busca sem debounce |
| Pets (+ raça/doenças) | ✅ | ✅ | ✅ | Não | OK | Alto | `ListarTodos` carrega planos+receitas+entregas em memória |
| Planos alimentares | ✅ | ✅ | ✅ | Não | OK | Baixo | 1 plano ativo por pet |
| Receitas da Casa | ✅ | ✅ | ✅ | Não | OK | Baixo | custo/kg calculado |
| Receitas Personalizadas | ✅ | ✅ | ✅ | Não | OK | Baixo | por pet |
| Ingredientes | ✅ | ✅ | ✅ | Não | OK | Baixo | custo atual/kg + coeficiente |
| Categorias (escopo) | ✅ | ✅ | ✅ | Não | OK | Baixo | unificadas (Alimento/Material/Ambos) |
| Tabela de Consumo | ✅ | ✅ | ✅ | Não | OK | Baixo | faixas de peso |
| Frequência de Entrega | ✅ | ✅ | ✅ | Não | OK | Baixo | cadastro |
| Tamanhos de Pacote | ✅ | ✅ | ✅ | Não | OK | Baixo | cadastro |
| Entregas | ✅ | ✅ | ✅ | Comentário obsoleto | OK | **Alto** | `GET /entregas` sem paginação + includes profundos |
| Venda Avulsa PF | ✅ (service) | ✅ | ✅ | Não | **Parcial** | **Alto p/ financeiro** | sem entidade; valor em texto livre |
| Pedidos PJ | ✅ | ✅ | ✅ | Não | **Parcial** | **Alto p/ financeiro** | `ValorTotal`/`PrecoUnitario` nunca preenchidos |
| Produção | ✅ | ✅ | ✅ | `.mock-banner` CSS | OK | **Alto** | `GET /producao/demanda` includes profundos |
| Cozinha | ✅ | ✅ | ✅ | Não | OK | Baixo | visão do dia |
| Rendimentos e Perdas | ✅ | ✅ | ✅ | Não | OK | Médio | depende de produção finalizada |
| Estoque | ✅ | ✅ | ✅ | Não | OK | Médio | entradas/lotes/movimentações; movimentações **paginadas** |
| Produto Acabado | ✅ | ✅ | ✅ | Não | OK | Baixo | `ItemEstoque` tipo ProdutoAcabadoCasa |
| Rotas | ✅ | ✅ | ✅ | Não | OK | **Alto** | `GET /rotas/{id}` N+1 por parada |
| Central Operacional | ✅ | ✅ | ✅ | `.mock` CSS | OK | Médio | agrega ordens+consumos+estoque |
| Relatórios + Dashboard | ✅ | ✅ (SO_ADMIN) | ✅ | Não | OK | Médio | full-load + agregação em memória |
| Cadastros auxiliares (raças, doenças, origens de venda, motivos de cancelamento) | ✅ | ✅ | ✅ | Não | OK | Baixo | CRUD simples |
| **Financeiro** | ❌ | ❌ | — | — | **Inexistente** | — | nenhuma entidade Conta/Pagar/Receber/Fatura |

---

## 5. Mapa das principais entidades e tabelas

**Identidade:** `Usuario` (papeis N:N `Papel`), `RefreshToken`.

**Clientes/Pets:**
- `Cliente` (PF e PJ no mesmo registro, via `Natureza`) → 1:1 opcional `ClientePj` (dados jurídicos/entrega PJ).
- `Pet` → FK `ClienteId`, FK `RacaId` (cadastro `Raca`), N:N `Doenca` via `PetDoenca`.
- `PlanoAlimentar` (1:1 com `Pet`) → `PlanoItemReceita` → `PlanoItemPacote`.

**Catálogo/Receitas:**
- `CategoriaIngrediente` (com `Escopo`) → `Ingrediente` (CustoAtualKg, CoeficienteConversao).
- `Receita` (Casa/Personalizada) → `ItemReceita` (gramas cozidas). `TamanhoPacote`, `FaixaConsumo`, `FrequenciaEntrega`, `OrigemVenda`, `MotivoCancelamento` (cadastros).

**Vendas/Entregas:**
- `Pedido` (PJ) → `PedidoItem`; FK opcional `EntregaId`.
- `Entrega` → `EntregaPet` → `EntregaItem` → (`EntregaItemPacote` [Casa] | `EntregaItemIngrediente` [Personalizada]); + `EntregaHistorico`. FK opcional `PedidoId` (PJ).
- **Venda Avulsa:** sem entidade — gera uma `Entrega` PF avulsa (valor em `Entrega.ObservacoesInternas`).

**Estoque/Produção:**
- `ItemEstoque` (Insumo | ProdutoAcabadoCasa) → `LoteEstoque`, `MovimentacaoEstoque`, `EntradaEstoque`, `AjusteEstoque`. `Fornecedor`.
- `OrdemProducao` → `FichaProducao` → `FichaProducaoIngrediente`; + `ConsumoIngredienteProducao` (planejado x real, sobra/perda).

**Rotas:** `Rota` → `RotaParada` (FK `EntregaId`).

---

## 6. Fluxos operacionais atuais

- **Cliente PF → Pet → Plano → Entrega:** cadastra-se o cliente (com frequência + primeira entrega + valor recorrente), o pet, e o plano alimentar. Ao salvar cliente/pet/plano, o sistema **regenera entregas futuras** (`RegerarFuturasDoClienteAsync`, best-effort). A geração (`GerarAsync`) cria entregas **somente futuras** a partir da próxima data do ciclo até o horizonte.
- **Cliente PJ → Pedido PJ → Entrega:** cria-se o cliente PJ, depois um `Pedido` (rascunho) com itens (receita Casa + tamanho + quantidade). Ao **confirmar**, gera-se a `Entrega` vinculada (`PedidoId`). Cancelar o pedido cancela a entrega vinculada elegível.
- **Venda Avulsa PF → Entrega:** fluxo único que cria diretamente uma `Entrega` PF (sem recorrência); valor/forma de pagamento ficam em texto na observação interna.
- **Produção → Estoque:** `OrdemProducao` do dia recebe fichas (Casa por receita+tamanho; Personalizada por entrega/pet). Ao **finalizar**, baixa insumos do estoque (FIFO por lote), registra consumo real (planejado x real, sobra/perda) e dá entrada no **produto acabado**.
- **Estoque → Entregas:** ao concluir uma entrega Casa, baixa o produto acabado correspondente (bloqueia se não houver produto acabado cadastrado/saldo).
- **Rotas → Entregas:** planeja-se a rota de um dia, adicionando entregas disponíveis (Programada/ConfirmadaCliente), com ordem arrastável e cálculo de kg total.
- **Receitas Personalizadas → Prontidão:** itens personalizados de entrega têm status de preparo (NaoPronta/Parcial/Pronta) e contagem de pacotes prontos.
- **Central Operacional → dados reais:** KPIs do dia/mês (PF ativos, pets, comida cozida, recorrência, custo de produção), entregas 7 dias, produção do dia, estoque e alertas — tudo via `GET /central/resumo`.
- **Custo médio (WAPC):** atualizado **na entrada** de estoque: `CustoMedio = (saldo×médioAtual + qtd×custoUnit) / novoSaldo`. `LoteEstoque` guarda o custo por lote.

---

## 7. Preparação para o módulo financeiro

### 7.1 Assinaturas/recorrência PF
- **Onde está a recorrência hoje:** **no próprio `Cliente`**, não há entidade de assinatura/contrato. Campos existentes: `TipoCliente` (Assinante/Avulso), `ValorRecorrenteMensal` (decimal), `FormaPagamento` (Cartao/Pix/Dinheiro/Outro), `DiaCobranca` (1–31), `StatusFinanceiro` (EmDia/Pendente/Inadimplente), `Ativo`, `MotivoCancelamento`+`DataCancelamento`+`MotivoCancelamentoId`+`ObservacaoCancelamento`+`UsuarioCancelamentoId`.
- **O que NÃO existe:** **data de início da assinatura** (só `PrimeiraEntrega`, que é de agenda), **pausa/retomada**, **histórico de reajuste**.
- **Suficiente para gerar cobranças até dezembro?** Há valor + dia de cobrança + forma de pagamento, então dá para **gerar parcelas mensais previstas**. Mas como tudo está "achatado" no cliente, **falta uma entidade de assinatura/contrato** para versionar valor (reajuste), registrar início/fim/pausa e amarrar as cobranças.
- **Recomendação:** criar **`AssinaturaFinanceira`** (ou `ContratoAssinatura`) 1:1 (ou 1:N histórico) com `Cliente`, contendo `ValorMensal`, `DiaVencimento`, `FormaPagamento`, `DataInicio`, `Status`, `DataCancelamento`. As **cobranças mensais** (`ContaReceber`) referenciam a assinatura. Isso **não toca** plano alimentar nem entregas (que continuam operacionais). O `Cliente.ValorRecorrenteMensal` vira a "semente" do valor da assinatura.

### 7.2 Venda Avulsa PF
- **Entidade própria?** **Não.** Gera uma `Entrega` PF; valor/forma de pagamento ficam em **texto livre** (`Entrega.ObservacoesInternas`).
- **Tem valor estruturado?** Não (só texto). Tem cliente/pet vinculado (via entrega) e data (DataPrevista).
- **Dá para gerar Conta a Receber automática?** **Não de forma confiável** hoje (valor não é estruturado). **Risco de duplicidade baixo** (1 venda = 1 entrega), mas o valor precisaria ser parseado de texto — frágil.
- **Recomendação:** adicionar **valor estruturado** à venda avulsa (campo na entrega avulsa ou uma entidade `VendaAvulsa` leve) para gerar `ContaReceber` direta.

### 7.3 Pedido PJ
- **Modelagem:** `Pedido` (Status Rascunho/Confirmado/Cancelado) → `PedidoItem` (receita+tamanho+qtd). Campos `Pedido.ValorTotal` e `PedidoItem.PrecoUnitario` **existem (nullable) mas NUNCA são preenchidos** pela aplicação.
- **Entrega vinculada:** sim (`Pedido.EntregaId`). Cancelamento do pedido cancela a entrega elegível.
- **Parcelamento:** não existe.
- **Suficiente para Conta a Receber?** **Não** — falta preço. Confirmar pedido sem valor geraria conta zerada.
- **Cancelamento → cancelar Conta a Receber futura:** o hook existe (cancelar pedido já cancela entrega); bastaria estender para cancelar a `ContaReceber` vinculada.
- **Risco de duplicidade:** médio — é preciso que a `ContaReceber` seja gerada **uma vez** na confirmação (idempotente por `PedidoId`).
- **Recomendação:** **preencher preço no Pedido PJ** (preço unitário por item → `ValorTotal`) antes/junto do financeiro.

### 7.4 Entradas de estoque / compras / notas fiscais
- **Modelagem:** `EntradaEstoque` tem `ValorUnitario`, `Frete` (separado, **não** compõe custo hoje), `ValorTotal` (sem frete), `Quantidade`, `DataCompra`, `DataEntrada`, `Validade`, `LoteCodigo`, `FornecedorId`.
- **O que NÃO existe:** **número da nota fiscal**, **chave de acesso (NF-e)**, **vencimento de pagamento**, **forma de pagamento**, **boleto/linha digitável**, **parcelamento**.
- **Fornecedor já tem (preparado p/ futuro):** `PrazoPagamentoDias`, `FormaPagamentoPreferida`, `ChavePix`, `DadosBancarios`.
- **Conta a Pagar automática a partir da entrada?** Parcialmente: há valor + fornecedor + data. **Falta** vencimento, forma de pagamento e dados de NF/boleto para uma AP completa.
- **Recomendação:** criar **`ContaPagar`** gerada a partir da entrada/compra (valor = `ValorTotal` + `Frete`), com vencimento derivado de `Fornecedor.PrazoPagamentoDias`. Campos de NF-e/boleto podem entrar como **opcionais** numa fase posterior.

### 7.5 Estoque, produção e custo
- **Custo médio:** WAPC atualizado na **entrada** (`ItemEstoque.RegistrarEntrada`). `LoteEstoque.CustoUnitario` guarda custo por lote (FIFO). **Não há tabela de histórico** de custo médio mês a mês.
- **Movimentações:** `MovimentacaoEstoque` (append-only) registra entrada/saída com saldo antes/depois, custo unitário, tipo e vínculos (entrada/ajuste/ordem/entrega).
- **Produção baixa insumos:** na finalização, por FIFO; registra consumo real (planejado x real, sobra/perda).
- **Produto acabado entra no estoque:** sim, na finalização.
- **Dados suficientes para relatórios de custo?** Sim para custo gerencial (custo médio, perdas, evolução de preço de compra via entradas). Não há "custo real por lote consumido" detalhado por entrega.
- **Fronteira financeiro × gerencial (correta e importante):**
  - **Compra de insumo → Conta a Pagar** (despesa financeira real).
  - **Pagamento da conta → saída financeira** (caixa/banco).
  - **Consumo do insumo na produção → NÃO gera nova despesa financeira** (já foi paga na compra); apenas **alimenta custo/margem/relatórios gerenciais**.

### 7.6 Cancelamentos
- **Cliente/assinatura:** cancelamento com **motivo padronizado** (`MotivoCancelamentoId`, FK ao cadastro `MotivoCancelamento`) + `DataCancelamento` + `UsuarioCancelamentoId` + `ObservacaoCancelamento`. ✅ Pronto para relatório.
- **Pet:** apenas `Ativo` (sem motivo/data).
- **Entrega / Ficha de produção:** motivo é **texto livre**.
- **Cadastro de Motivos de Cancelamento:** **já existe** (criado recentemente).

### 7.7 Relatórios — viabilidade com dados atuais
| Relatório | Viável hoje? | Observação |
|---|---|---|
| Vendas | ✅ (kg real; R$ só recorrência) | R$ por venda PJ/avulsa depende dos ajustes 7.2/7.3 |
| Cancelamentos | ✅ | motivo padronizado no cliente |
| Produção planejado × real | ✅ (se houver produções finalizadas) | depende de produção finalizada com pesagem real |
| Perdas de ingredientes | ✅ (idem) | custo estimado pelo custo médio |
| Evolução de custo dos insumos | ✅ | a partir das entradas de estoque |
| Custo médio | ✅ | custo médio atual por item |
| Estoque e produto acabado | ✅ | saldo/mínimo/valor estimado |
| Dashboard com gráficos | ✅ | SVG próprio, sem dependência |

> Já existe um **módulo de Relatórios + Dashboard** implementado (somente Administrador) cobrindo esses itens.

---

## 8. Validação do escopo financeiro planejado

> Observação: o **texto exato do "prompt financeiro"** não me foi fornecido nesta tarefa. A validação abaixo é feita contra o **escopo financeiro descrito no próprio pedido de diagnóstico** (assinatura→cobrança, venda avulsa→a receber, pedido PJ→a receber, entrada→a pagar, NF/boleto/conciliação/NF-e emitida).

- **Adequado ao estado atual?** O conceito sim. Mas há **pré-condições de dados** (preço em Pedido PJ e Venda Avulsa) sem as quais a geração automática de Conta a Receber fica frágil.
- **Está grande demais?** **Sim**, se tentar entregar tudo (AR + AP + assinatura versionada + NF-e + boleto + conciliação bancária) de uma vez. Risco alto de regressão e de baixa testabilidade.
- **Seguro?** Em fases, sim. A grande maioria do financeiro é **aditiva** (novas tabelas/telas) e não precisa quebrar operacional.
- **Dividir em fases menores?** **Recomendado.** Ver §15.
- **O que pode ser feito agora:** Contas a Receber/Pagar (entidades, CRUD, baixa/pagamento, status, vencimento), geração de cobrança da **assinatura** (valor já existe), geração de Conta a Pagar a partir da **entrada de estoque**.
- **O que deve ficar para depois:** NF-e emitida, importação de boleto/linha digitável, conciliação bancária, parcelamento avançado, histórico de reajuste de assinatura.
- **Campos/entidades a ajustar antes:** preço em `Pedido`/`PedidoItem`; valor estruturado na Venda Avulsa; vencimento/forma de pagamento na entrada de estoque (ou derivar do fornecedor).
- **Risco de entidade duplicada:** **médio.** Já há `MotivoCancelamento`, `OrigemVenda`, `Fornecedor` com campos de pagamento. O prompt financeiro **não deve recriar** esses; deve **reusar**. Não criar "AssinaturaFinanceira" que duplique o que já está no `Cliente` sem migrar o valor de forma controlada.
- **Risco de quebrar módulos existentes:** **baixo**, desde que o financeiro seja aditivo e que a geração automática seja **idempotente** (1 conta por origem) e **opcional** (não bloquear o fluxo operacional se faltar valor).
- **Risco de performance:** médio — relatórios financeiros e listagens de contas precisarão de **paginação + filtros obrigatórios de período** desde o início (não repetir o padrão "carrega tudo" de algumas telas atuais).
- **Recomendação final:** ajustar as pré-condições de dados (preço PJ/avulsa) e então implementar a **Fase 1 financeira** (Contas a Receber/Pagar + geração da assinatura/entrada), com paginação e índices desde o início.

---

## 9. Diagnóstico de performance

### Backend/API
- **Endpoints críticos / gargalos:**
  - `GET /entregas` — **sem paginação** + `Include(Pets→Itens→Pacotes/Ingredientes)`: risco de explosão cartesiana (SingleQuery) em períodos grandes.
  - `GET /producao/demanda` — mesmos includes profundos sobre todo o intervalo.
  - `GET /pets` (`ListarTodos`) — carrega pets + planos + receitas + entregas e agrega em memória.
  - `GET /rotas/{id}` — **N+1**: para cada parada carrega a entrega com includes profundos.
  - `GET /central/resumo` — várias consultas pesadas (ordens+consumos+itens) para os KPIs.
  - Relatórios (`/relatorios/*`) — **full-load do período em memória** + agregação (intencional p/ ~150 clientes; frágil acima disso). Já têm paginação na tabela de detalhe.
- **N+1:** `Rotas.ObterAsync` (por parada). Geração/regeração de entregas itera clientes (aceitável, é operação de escrita pontual).
- **Paginação:** existe em `GET /estoque/movimentacoes` e nos relatórios detalhados. **Falta** em `GET /entregas`, `GET /pets`, `GET /estoque/entradas`, `GET /producao`, `GET /clientes`.
- **Filtros:** vários filtros são aplicados em memória após carregar tudo (clientes, itens de estoque). Endpoints de período (entregas/produção/relatórios) deveriam exigir **período obrigatório**.
- **Payloads:** entregas/demanda retornam grafo aninhado grande.
- **Recomendações:** paginar listagens transacionais; usar `AsSplitQuery()` onde há múltiplos `Include` de coleção; projetar para DTO em vez de carregar o grafo inteiro quando possível; tornar período obrigatório nos endpoints de data.

### Banco de dados
- **Índices existentes:** ~70 (cobertura boa de FKs e de colunas-chave). Destaques úteis já presentes: `ix_entregas_cliente_data`, `ix_entregas_data`, `ix_entregas_status`, `ix_pedidos_*`, `ix_mov_estoque_data/tipo/item`, `ix_ordens_producao_data` (único), `ix_rotas_data/status`.
- **Índices compostos recomendados (NÃO criar agora):**
  - `entregas (data_prevista, status)` — muitas consultas filtram data **e** status.
  - `pedidos (cliente_id, status)` e/ou `(status, data_entrega)`.
  - `movimentacoes_estoque (item_estoque_id, data_hora)` e `(tipo, data_hora)`.
  - `consumo_ingrediente_producao (ingrediente_id)` — relatórios de perdas/produção filtram por ingrediente.
  - Para o **financeiro futuro:** `contas (status, vencimento)`, `contas (cliente_id/fornecedor_id, vencimento)`, índices por origem (`assinatura_id`, `pedido_id`, `entrada_estoque_id`) para idempotência.
- **Tabelas que vão crescer muito:** `entregas`, `entrega_pets`, `entrega_itens`, `entrega_item_pacotes`, `movimentacoes_estoque`, `entrega_historico`, e (futuro) tabelas financeiras. Com 3 meses já há milhares de entregas no teste; em produção crescem continuamente.
- **Migrations:** 27 aplicadas no startup; snapshot mantido à mão; `PendingModelChangesWarning` ignorado de propósito.

### Frontend
- **Bundle / build:** `ng build` ~23,5s; **bundle inicial 1,74 MB raw / ~308 kB transferência** (main ~1,52 MB). **Rotas NÃO são lazy-loaded** — todos os componentes são importados em `app.routes.ts`, então tudo cai no `main`. Oportunidade clara de `loadComponent` para reduzir o carregamento inicial.
- **Telas pesadas / sem paginação:** Entregas, Clientes, Itens de Estoque, Compras carregam tudo e filtram no cliente.
- **Sem debounce:** busca de **Clientes PJ** (dispara a cada tecla) e filtros de data do **Dashboard**.
- **Chamadas paralelas sem `forkJoin`:** Movimentações, Compras, Relatórios disparam vários `subscribe` no `ngOnInit` sem coordenação/erro unificado.
- **Charts:** SVG próprios (sem dependência) — leves.
- **apiUrl:** prod `'/api'` (relativo) ✅.

---

## 10. Métricas coletadas

| Métrica | Valor | Observação |
|---|---|---|
| Build frontend (`ng build`) | ~23,5 s | medido nesta análise |
| Build frontend (parede, c/ npm) | ~36 s | inclui startup do npm |
| Bundle inicial (raw / transfer) | 1,74 MB / ~308 kB | sem lazy-loading de rotas |
| Maior chunk | `main` ~1,52 MB | todos os componentes eager |
| Build backend | **não medido aqui** | `dotnet` indisponível no ambiente cloud; compila via Docker (confirmado em sessão anterior) |
| Status da API (runtime) | **não medido aqui** | sem acesso aos containers em execução |
| Tempo de resposta de GETs | **não medido aqui** | rodar localmente (ver abaixo) |
| Contagens de registros | **não disponíveis** | sem acesso ao banco; SQL abaixo |
| Limitações do ambiente | cloud, sem dotnet/docker/psql ao banco do usuário | análise estática + build de frontend |

**SQL seguro para você rodar localmente (somente leitura) no banco principal:**
```sql
-- Rodar no banco PRINCIPAL: docker exec sistemaan-db psql -U sistemaan -d sistemaan -c "<SQL>"
SELECT
 (SELECT count(*) FROM clientes)                       AS clientes,
 (SELECT count(*) FROM clientes WHERE natureza='PessoaJuridica') AS clientes_pj,
 (SELECT count(*) FROM pets)                           AS pets,
 (SELECT count(*) FROM receitas WHERE tipo='Casa')     AS receitas_casa,
 (SELECT count(*) FROM receitas WHERE tipo='Personalizada') AS receitas_pers,
 (SELECT count(*) FROM ingredientes)                   AS ingredientes,
 (SELECT count(*) FROM entregas)                       AS entregas,
 (SELECT count(*) FROM pedidos)                        AS pedidos,
 (SELECT count(*) FROM ordens_producao)                AS producoes,
 (SELECT count(*) FROM fichas_producao)                AS fichas,
 (SELECT count(*) FROM consumo_ingrediente_producao)   AS consumo_producao,
 (SELECT count(*) FROM itens_estoque)                  AS itens_estoque,
 (SELECT count(*) FROM movimentacoes_estoque)          AS mov_estoque,
 (SELECT count(*) FROM entradas_estoque)               AS entradas_estoque,
 (SELECT count(*) FROM rotas)                          AS rotas,
 (SELECT count(*) FROM usuarios)                       AS usuarios;
```
> Lembrete: como o compose principal **não** liga `Seed__DemoData`, espera-se que o banco principal contenha apenas os **cadastros reais** que você inseriu (mais os seeds de catálogo padrão), e não a massa de demonstração.

---

## 11. Mocks e dados fictícios encontrados

| Arquivo | Tipo | Impacto | Precisa corrigir? | Observação |
|---|---|---|---|---|
| `features/entregas/entregas.component.ts:267` | Comentário "Mock já traz o operacional embutido" | Nenhum | Opcional | **Comentário obsoleto** — a tela usa API real (`service.listar()`); só o texto ficou |
| `features/central/central-operacional.component.scss:30` | Classe CSS `.mock` | Nenhum | Opcional | CSS sem uso ativo relevante |
| `features/receitas/...scss`, `features/ingredientes/...scss` | Classe CSS `.mock` | Nenhum | Opcional | idem |
| `features/producao/planejar.component.scss:3`, `producao-dia.component.scss:3` | `.mock-banner` | Baixo | Verificar template | conferir se algum banner "demo" ainda aparece na tela |
| `features/producao/planejar.component.ts:16-17` | Constantes `FATOR_CRU_ESTIMADO=1.8`, `CUSTO_KG_CRU_ESTIMADO=22` | Baixo | Não (documentado) | estimativa só para exibição no planejamento (não persistida) |

**Classificação geral:** não há **mock ativo perigoso** nem dados fictícios sendo servidos como reais no código do ambiente principal. Os achados são **inofensivos/legados** (comentário e CSS) — limpeza cosmética opcional. (A massa fictícia existe **apenas** no `DemoDataSeeder`, que **só roda no ambiente de teste**.)

---

## 12. Riscos críticos antes do financeiro

**Crítico**
1. **Pedido PJ sem valor.** Impacto: Conta a Receber de PJ não confiável. Onde: `Pedido.ValorTotal`/`PedidoItem.PrecoUnitario` nunca preenchidos. Recomendação: preencher preço antes/junto do financeiro. 
2. **Venda Avulsa sem valor estruturado.** Impacto: AR de avulsa depende de parse de texto. Onde: `VendaAvulsaService` (valor em `ObservacoesInternas`). Recomendação: valor estruturado.

**Alto**
3. **Endpoints sem paginação + includes profundos** (`/entregas`, `/producao/demanda`, `/pets`, `/rotas/{id}`). Impacto: lentidão/memória conforme a base cresce. Recomendação: paginar, `AsSplitQuery`, projeção, período obrigatório.
4. **Geração automática precisa ser idempotente.** Impacto: risco de contas duplicadas. Recomendação: 1 conta por origem (assinatura/mês, pedido, entrada) com índice único.

**Médio**
5. **Relatórios carregam o período em memória.** Impacto: ok hoje, frágil ao crescer. Recomendação: agregação no banco em fase futura.
6. **Frontend sem lazy-loading** (bundle único grande). Impacto: carregamento inicial. Recomendação: `loadComponent`.
7. **Assinatura "achatada" no cliente** (sem início/pausa/reajuste). Impacto: limita o financeiro de recorrência. Recomendação: entidade de assinatura.

**Baixo**
8. Busca sem debounce (Clientes PJ, Dashboard). 9. `subscribe` sem `forkJoin` no `ngOnInit`. 10. Comentários/CSS "mock" obsoletos.

---

## 13. Ajustes recomendados antes do financeiro

**1) Obrigatórios (pré-condição p/ AR confiável)**
- Preencher **preço no Pedido PJ** (preço unitário por item → `ValorTotal`).
- Adicionar **valor estruturado na Venda Avulsa PF**.

**2) Recomendados (qualidade/preparação)**
- Definir o **modelo de Conta a Receber/Pagar** (e se haverá `AssinaturaFinanceira`).
- Em `EntradaEstoque`: prever **vencimento/forma de pagamento** (ou derivar do `Fornecedor`).
- Índices compostos `entregas(data_prevista,status)` e equivalentes (apenas planejar).
- Tornar período obrigatório nos endpoints de data.

**3) Podem ficar para depois**
- Lazy-loading do frontend; paginação das listas restantes; debounce; `forkJoin`.
- NF-e emitida, boleto/linha digitável, conciliação bancária, parcelamento avançado, histórico de reajuste.
- Limpeza de comentários/CSS "mock".

---

## 14. Recomendações de performance

- **Índices (recomendar, não criar):** `entregas(data_prevista,status)`, `pedidos(cliente_id,status)`, `movimentacoes_estoque(item_estoque_id,data_hora)` e `(tipo,data_hora)`, `consumo_ingrediente_producao(ingrediente_id)`; e, no financeiro, `contas(status,vencimento)` + índices por origem.
- **Paginação:** `/entregas`, `/pets`, `/estoque/entradas`, `/producao`, `/clientes`, e todas as futuras listagens financeiras.
- **Filtros obrigatórios:** período em `/entregas`, `/producao/demanda`, relatórios.
- **Backend:** `AsSplitQuery()` nos endpoints com múltiplos `Include` de coleção; projeção para DTO; evitar carregar o grafo inteiro.
- **Frontend:** lazy-load de rotas (`loadComponent`); debounce nas buscas; `forkJoin` no `ngOnInit`; paginação nas tabelas grandes.

---

## 15. Conclusão

- **Posso seguir para o financeiro agora?** Sim, **mas com ajuste de pré-condições** (preço em Pedido PJ e Venda Avulsa) e **em fases**. O sistema é estável e aditivo o suficiente para receber o financeiro sem quebrar o operacional, desde que a geração automática seja idempotente e opcional.
- **Devo ajustar algo antes?** Sim: valor real em PJ/avulsa; decisão do modelo de contas; alguns índices/paginação planejados.
- **Dividir em fases menores?** **Sim, fortemente recomendado.**
- **Fase 1 ideal do financeiro (considerando o sistema real):**
  1. **Entidades** `ContaReceber` e `ContaPagar` (valor, vencimento, status, origem polimórfica: assinatura/pedido/venda-avulsa/entrada-estoque, cliente/fornecedor), com **índice único por origem** (idempotência).
  2. **Geração automática:** assinatura PF (valor já existe) → cobranças mensais previstas; **entrada de estoque** → Conta a Pagar (valor + frete; vencimento via fornecedor).
  3. **Pré-condição:** preço em Pedido PJ e Venda Avulsa → então gerar AR desses também.
  4. **Telas:** lista de Contas a Receber/Pagar com **paginação + filtro de período obrigatório**, baixa/pagamento (data + forma), status (Aberta/Paga/Vencida/Cancelada).
  5. **Relatórios financeiros básicos:** a receber/a pagar por período, fluxo de caixa simples — reusando o módulo de Relatórios existente.
  - **Fases seguintes:** assinatura versionada (reajuste/pausa), NF-e/boleto, conciliação bancária, parcelamento.

> Este relatório foi gerado por análise estática. As contagens de registros e os tempos de resposta de runtime devem ser coletados localmente (§9/§10) para complementar o quadro.
