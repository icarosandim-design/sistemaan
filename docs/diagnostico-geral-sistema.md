# Diagnóstico Geral do Sistema — SistemaAN / Nuri Pet

> **Tipo:** somente leitura/análise. Nada foi apagado, alterado ou corrigido.
> **Data:** 2026-06-16 · **Branch:** `claude/jolly-pascal-uz20jt`
> **Ambiente da análise:** nuvem (remoto), **sem `dotnet`/Docker** e **sem acesso ao banco local**.
>
> **Legenda de confiança:**
> ✅ validado no código · ⚠️ inferido do código (lógica clara) · ❓ precisa confirmar (não consegui validar no ambiente)

---

## 1. Resumo executivo

**Estado geral:** o sistema está **maduro e bem amarrado** na maior parte dos módulos. A arquitetura é limpa (Domínio / Aplicação / Infraestrutura / API no backend; Angular 19 standalone no frontend), as regras críticas de negócio (estoque comprometido/disponível, baixa por entrega, produção planejado×real, duplicidade de rotas) estão implementadas e o frontend consome a API real em **praticamente todas as telas**.

**Está estável?** Em termos de código, sim — o **frontend compila** (✅ `ng build` passou). O **backend não pôde ser compilado** neste ambiente (sem `dotnet`), então a compilação das features mais recentes precisa de confirmação ao subir a API.

**Principais riscos (resumidos):**
1. 🔴 **Migrations pendentes + auto-migrate no startup.** A API aplica migrations automaticamente (`Program.cs` → `MigrateAsync()`). Como a API não rebuildou desde o bloqueio do registry (MCR 429), várias migrations recentes provavelmente **ainda não foram aplicadas** ao banco em uso. No próximo start bem-sucedido, todas serão aplicadas de uma vez — é o ponto que mais pode "quebrar" e precisa ser observado.
2. 🔴 **Autorização por papel quase só no frontend.** A maioria dos endpoints usa apenas `[Authorize]` (qualquer usuário logado). Um usuário "Cozinha" ou "Operador" poderia chamar a API de cadastros/estoque diretamente, mesmo sem ver o menu.
3. 🟠 **Tela de Entregas ainda tem MOCK de fallback** (clientes/pets fictícios, estoque/prontidão mockados) quando há menos de 3 entregas reais.
4. 🟠 **Drift de ModelSnapshot tolerado** (`PendingModelChangesWarning` ignorado): o app sobe mesmo que o snapshot não bata com as migrations; a correção em runtime depende das migrations estarem corretas.

**Está pronto para limpar dados?** **Ainda não com segurança total.** É possível planejar a limpeza (este relatório já mapeia o que preservar), mas antes é **obrigatório**: (a) subir a API com sucesso e confirmar que as migrations aplicaram; (b) fazer **backup** do banco; (c) confirmar que os **seeders não sobrescrevem** os cadastros reais editados. Detalhes na seção 15.

---

## 2. Mapa geral dos módulos

| Módulo | Backend | Frontend | API real? | Mock? | Status | Observações |
|---|---|---|---|---|---|---|
| Login / Auth | ✅ | ✅ | Sim | Não | Pronto | JWT + refresh; Pbkdf2; bloqueia inativo |
| Central Operacional | ✅ | ✅ | **Sim** | Não | Pronto | Era mock; hoje `GET /api/central/resumo` |
| Clientes PF | ✅ | ✅ | Sim | Não | Pronto | `natureza = PessoaFisica` |
| Clientes PJ | ✅ | ✅ | Sim | Não | Pronto | Entidade única + `cliente_pj` 1:1 |
| Pets | ✅ | ✅ | Sim | Não | Pronto | Só de cliente PF |
| Planos alimentares | ✅ | ✅ | Sim | Não | Pronto | 1:1 com Pet |
| Receitas da Casa | ✅ | ✅ | Sim | Não | Pronto | Soma exata 1000g |
| Receitas Personalizadas | ✅ | ✅ | Sim | Não | Pronto | Por pet; guardadas na mesma tabela `receitas` |
| Ingredientes | ✅ | ✅ | Sim | Não | Pronto | Custo manual; coeficiente de conversão |
| Categorias | ✅ | ✅ | Sim | Não | Pronto | — |
| Tabela de Consumo | ✅ | ✅ | Sim | Não | Pronto | Faixas peso→g/dia |
| Frequências de Entrega | ✅ | ✅ | Sim | Não | Pronto | — |
| Tamanhos de Pacote | ✅ | ✅ | Sim | Não | Pronto | — |
| Entregas | ✅ | ✅ | Sim | **Sim (fallback)** | Quase pronto | Mock quando < 3 entregas reais |
| Pedidos PJ | ✅ | ✅ | Sim | Não | Pronto | Rascunho→Confirmado gera entrega |
| Produção (Planejar/Dia/Cozinha) | ✅ | ✅ | Sim | Não | Pronto | Demanda PF+PJ |
| Impressão de produção | ✅ | ✅ | Sim | Não | Pronto | Tela cheia p/ impressão |
| Rendimentos/Perdas | ✅ | ✅ | Sim | Não | Pronto | Planejado×real reais |
| Estoque (itens/3 abas) | ✅ | ✅ | Sim | Não | Pronto | Insumo / Produto acabado / Personalizadas prontas |
| Movimentações / Compras / Fornecedores | ✅ | ✅ | Sim | Não | Pronto | FIFO; frete separado do custo |
| Produto acabado | ✅ | ✅ | Sim | Não | Pronto | Comprometido/disponível dinâmico |
| Rotas / Saídas | ✅ | ✅ | Sim | Não | Pronto | Múltiplas/dia; anti-duplicidade |
| Venda avulsa PF | ✅ | ✅ | Sim | Não | Pronto | Reusa Entrega (sem nova entidade) |
| Usuários e permissões | ✅ (parcial) | ✅ | Sim | Não | ⚠️ Atenção | Papel forte no front; fraco no back |

---

## 3. Mapa das integrações

- **Cliente PF → Pet → Plano → Entrega:** ✅ Cliente PF ativo, com frequência + primeira entrega, gera entregas recorrentes (`EntregaService.GerarPara`), montando os pets/itens a partir do Plano Alimentar de cada pet.
- **Cliente PJ → Pedido PJ → Entrega:** ✅ Pedido em Rascunho **não** gera entrega; ao **Confirmar**, cria uma Entrega vinculada (container `EntregaPet` com `PetId = null`); ao **Cancelar**, cancela a entrega (se não Entregue/Cancelada).
- **Entrega → Estoque:** ✅ Entrega ativa **compromete** produto acabado por `(receita, peso)`; ao marcar **Entregue**, baixa o físico (idempotente via `EstoqueBaixado`) e bloqueia se faltar/ não houver produto acabado cadastrado.
- **Entrega → Produção:** ✅ A demanda de produção lê as entregas do período (PF + PJ).
- **Entrega → Rotas:** ✅ Rota agrupa entregas do dia; uma entrega não entra em duas rotas **ativas**.
- **Produção → Estoque:** ✅ Finalizar produção baixa **insumos** pelo **cru real** e gera **produto acabado** (Casa).
- **Produção → Produto Acabado:** ✅ Fichas Casa concluídas viram saldo de produto acabado (quando o item está cadastrado — ver risco em 4/11).
- **Receita Personalizada → Prontidão:** ✅ Ao concluir a ficha, a prontidão é gravada no `EntregaItem` (vinculada a entrega/pet); ao Entregar, a reserva é zerada.
- **Rotas → Entregas:** ✅ Despachar a rota muda só as entregas daquela rota para "Saiu para entrega".
- **Usuários → Permissões:** ⚠️ Papéis no JWT (claim `role`); aplicados **fortemente no frontend** (menu + guard), **fracamente no backend** (poucos controllers restringem por papel).
- **Central → Resumo/atalhos:** ✅ Resumo real (KPIs, entregas 7 dias, produção do dia, estoque, rotas, alertas) com atalhos para os módulos; cartões financeiros só para Administrador.

---

## 4. Backend

### 4.1 Estrutura
Clean Architecture em 4 projetos: `SistemaAN.Domain` (entidades/enums), `SistemaAN.Application` (services, DTOs, interfaces), `SistemaAN.Infrastructure` (EF Core, configurations, migrations, seeds, identity), `SistemaAN.Api` (controllers, Program.cs). `Directory.Build.props`: net9.0, Nullable on, ImplicitUsings on.

### 4.2 Entidades principais (resumo)

| Entidade | Tabela | Tipo | Preservar? | Observações |
|---|---|---|---|---|
| Cliente | `clientes` | Operacional | Não | `Natureza` PF/PJ; financeiro básico embutido |
| ClientePj | `cliente_pj` | Operacional | Não | 1:1 com Cliente (cascade) |
| Pet | `pets` | Operacional | Não | Só de cliente PF (cascade c/ cliente) |
| PlanoAlimentar (+itens/pacotes) | `planos_alimentares`, `plano_itens_receita`, `plano_item_pacotes` | Operacional | Não | 1:1 com Pet |
| Receita (Casa **e** Personalizada) | `receitas` | **Misto** | **Casa: Sim / Personalizada: Não** | ⚠️ mesma tabela guarda os dois tipos |
| ItemReceita | `itens_receita` | **Misto** | Segue a receita-mãe | itens das receitas |
| Ingrediente | `ingredientes` | Cadastro | **Sim** | custo manual |
| CategoriaIngrediente | `categorias_ingredientes` | Cadastro | **Sim** | — |
| FaixaConsumo | `faixas_consumo` | Cadastro | **Sim** | tabela de consumo |
| FrequenciaEntrega | `frequencias_entrega` | Cadastro | **Sim** | — |
| TamanhoPacote | `tamanhos_pacote` | Cadastro | **Sim** | — |
| Entrega (+pets/itens/pacotes/ingredientes/histórico) | `entregas`, `entrega_pets`, `entrega_itens`, `entrega_item_pacotes`, `entrega_item_ingredientes`, `entrega_historico` | Operacional | Não | snapshot completo do cliente |
| Pedido / PedidoItem | `pedidos`, `pedido_itens` | Operacional | Não | só Receita da Casa |
| OrdemProducao / FichaProducao / FichaIngrediente / Consumo | `ordens_producao`, `fichas_producao`, `ficha_producao_ingredientes`, `consumo_ingrediente_producao` | Operacional | Não | guarda histórico de rendimento |
| ItemEstoque | `itens_estoque` | Estoque/Cadastro | **Cuidado (C)** | Insumo + Produto acabado |
| LoteEstoque / Movimentacao / Entrada / Ajuste | `lotes_estoque`, `movimentacoes_estoque`, `entradas_estoque`, `ajustes_estoque` | Estoque/Mov. | Cuidado (C) | livro-razão append-only |
| Fornecedor | `fornecedores` | Cadastro/apoio | Cuidado (C) | — |
| Rota / RotaParada | `rotas`, `rota_paradas` | Operacional | Não | — |
| Usuario / Papel / join / RefreshToken | `usuarios`, `papeis`, `usuarios_papeis`, `refresh_tokens` | Segurança | **Sim** (refresh_tokens pode limpar) | senha Pbkdf2 |

### 4.3 Services / Controllers / Endpoints
- **23 controllers**, todos sob `[Authorize]`. Restrição por papel **apenas** em: `UsuariosController` (Administrador), `CentralController` (Administrador+Operador), `VendasAvulsasController` (Administrador+Operador) e um endpoint do `AuthController` (Administrador). ⚠️ **Demais endpoints aceitam qualquer usuário autenticado.**
- Services bem organizados por módulo, com validações via `ValidationException` (formato `errors` por campo) e exceções de domínio (`NotFoundException`, `BusinessRuleException`, `ForbiddenAccessException`).
- Tratamento de erro: middleware central (há `Middleware/` na API) converte exceções em respostas padronizadas (o frontend lê `error.errors`/`error.detail`). ❓ Confirmar cobertura de todos os tipos.
- Logs: Serilog configurado (`Serilog.AspNetCore`).

### 4.4 Regras críticas (verificação)

**Clientes PF/PJ** — ✅ PF funciona; ✅ PJ não exige pet (usa container `EntregaPet` com `PetId=null`); ✅ PJ fora da geração recorrente (`ClientesElegiveisAsync` filtra `Natureza == PessoaFisica`); ✅ entidade única com `Natureza`; ✅ `cliente_pj` 1:1 validado.

**Pets** — ✅ pertencem a cliente PF; ✅ vínculo com plano; ✅ cascade com cliente evita órfão; ✅ tela lista sem quebrar.

**Pedidos PJ** — ✅ Confirmado gera entrega; ✅ Cancelado cancela entrega vinculada; ✅ Rascunho não gera; ✅ Confirmado só edita com entrega Programada; ✅ itens só Receita da Casa.

**Entregas** — ✅ PF e PJ aparecem; ✅ snapshot com endereço/telefone/preferência; ✅ transições validadas; ✅ Entregue baixa produto acabado + zera reserva personalizada (idempotente).

**Estoque** — ✅ saldo via movimentações (cache, sem negativo); ✅ comprometido/disponível dinâmico por `(receita, peso)` e data; ✅ exemplo "30 / seg 20 / qua 30 → falta 20 na qua" confere; ✅ produto acabado não cadastrado **bloqueia** Entregue; ✅ personalizada não mistura com estoque geral; ✅ **FIFO** por validade→entrada→id; ✅ baixa duplicada impedida (`EstoqueBaixado`); ✅ frete **não** compõe custo.

**Produção** — ✅ demanda PF+PJ; ✅ não duplica `EntregaItem` entre ordens abertas; ✅ **cru real obrigatório** na finalização; ✅ cozido/sobra/perda existem (cozido não obrigatório — ⚠️); ✅ ficha "não feita" não gera produto; ✅ ficha pendente bloqueia finalização; ✅ prontidão personalizada vinculada a entrega/pet; ⚠️ **produto acabado da Casa não cadastrado é ignorado silenciosamente na finalização** (não registra pendência explícita).

**Rendimentos/perdas** — ✅ usa dados reais (consumos), planejado×real, preserva histórico nas ordens.

**Rotas** — ✅ múltiplas saídas/dia; ✅ entrega não entra em 2 rotas ativas; ✅ status; ✅ Despachar muda só as entregas da rota; ✅ Disponíveis exclui entregas em rota ativa; ⚠️ **Cancelar/Concluir rota não tem ação explícita** sobre as entregas — porém, como "Disponíveis" filtra por rota **ativa**, ao cancelar a rota as entregas voltam a ficar disponíveis automaticamente (não há "deadlock"). ❓ Confirmar a experiência (a parada antiga continua registrada na rota cancelada). ⚠️ Alerta de **preferência incompatível** é calculado no **frontend** (helper `preferenciaIncompativel`), não no DTO do backend.

**Usuários e permissões** — ✅ 3 perfis (Administrador/Operador/Cozinha); ✅ login bloqueia inativo; ✅ senha Pbkdf2 (100k iterações, salt 16B, SHA256, comparação tempo-fixo); ✅ sem exclusão física (só inativa); ✅ papéis no JWT como claim `role`; ⚠️ **autorização por papel não é aplicada na maioria dos endpoints** (risco "permissão só no front"); ⚠️ criação de usuário **não** confirma e-mail/senha por digitação dupla no backend (a coincidência é validada no frontend).

**Central** — ✅ consome API real; ✅ visual mantido; ✅ botão Nova venda; ✅ Venda avulsa PF **implementada** (não está mais "Em breve"); ✅ atalhos corretos; ✅ KPIs reais; ✅ cartões financeiros só Administrador; ✅ é resumo, não tela operacional.

---

## 5. Frontend

- **Framework:** Angular 19 standalone, signals, Material 19, control-flow (`@if/@for`).
- **Rotas:** todas protegidas por `authGuard` + `papelGuard` (lê `data.papeis`). Sem rotas quebradas; sem `routerLink` apontando para rota inexistente.
- **Menu lateral:** filtra itens por papel (`menuVisivel`). Grupos: Central, Clientes, Produção (TODOS_PERFIS), Estoque, Entregas (com submenu Planejar Rotas), Cadastros (Usuários só Admin).
- **Permissão na UI além do menu:** apenas a **Cozinha** usa `temPapel` para "somente visualização" do Operador. As demais ações dependem da rota/menu.
- **Estados loading/erro/vazio:** ✅ presentes em todas as telas listadas.
- **Telas em MOCK:** **somente Entregas** (`entregas.model.ts`): `MOCK_CLIENTES`, `MOCK_PETS` (Bidu, Thor, Luna...), `ESTOQUE_MOCK`, prontidão mock e `gerarEntregasMock()`. Ativa quando **há menos de 3 entregas reais**, exibindo um banner "dados hipotéticos (mock)". Quando o detalhe é carregado, estoque/prontidão reais (via EstoqueService / `statusPreparo`) substituem o mock.
- **Botões sem ação / "Em breve":**
  - Central: 2 botões "+N — abrir produção" (Casa/Personalizada) com `matTooltip="Em breve"` e sem `(click)`.
  - Entregas: botão "Planejar rota do dia" mostra toast "Módulo de Rotas em breve" (o módulo de Rotas já existe no menu, mas esse atalho específico não navega).
- **Stores:** Produção usa um store (`ProducaoStore`) sobre o `ProducaoService`. Demais telas usam services diretos.
- **Restante do código:** sem outros `MOCK`, `delay()` artificiais, `TODO`/`FIXME` relevantes (a busca por TODO só achou a constante `TODOS_PERFIS`).

---

## 6. Banco de dados (mapa completo)

> Tabelas confirmadas pelos `ToTable(...)` das configurations + migrations. On-delete confirmado nas configurations.

| Tabela | Tipo | Finalidade | Preservar? | Pode limpar depois? | Dependências (FK on-delete) | Risco ao limpar |
|---|---|---|---|---|---|---|
| `categorias_ingredientes` | Cadastro | Categorias de ingrediente | **Sim (A)** | Não | Ingrediente→Categoria (Restrict) | Apagar bloqueado se houver ingrediente |
| `ingredientes` | Cadastro | Ingredientes base | **Sim (A)** | Não | ItemReceita/ItemEstoque→Ingrediente (Restrict) | Bloqueado se usado |
| `receitas` | Misto | Casa (A) **e** Personalizada (B) | **Casa: A** | Personalizada: B | ItemReceita (Cascade); PlanoItemReceita (Restrict); Receita→Pet (Restrict) | ⚠️ filtrar por `tipo` |
| `itens_receita` | Misto | Itens das receitas | Segue receita | Segue receita | →Ingrediente (Restrict) | Casa preservar |
| `faixas_consumo` | Cadastro | Tabela de consumo | **Sim (A)** | Não | — | — |
| `frequencias_entrega` | Cadastro | Frequências | **Sim (A)** | Não | Cliente→Frequencia (Restrict) | — |
| `tamanhos_pacote` | Cadastro | Tamanhos de pacote | **Sim (A)** | Não | Plano/Estoque referenciam | — |
| `usuarios` | Segurança | Usuários | **Sim (A)** | Não | join `usuarios_papeis` (Cascade) | Nunca apagar |
| `papeis` | Segurança | Perfis | **Sim (A)** | Não | join (Cascade) | Nunca apagar |
| `usuarios_papeis` | Segurança/vínculo | Usuário×Papel | **Sim (A)** | Não | — | Preservar |
| `refresh_tokens` | Segurança | Tokens de sessão | Pode limpar | Sim | →Usuario (Cascade) | Baixo (re-login) |
| `clientes` | Operacional | Clientes PF/PJ | Não | **Sim (B)** | Entrega/Pedido→Cliente (Restrict); Pet/ClientePj→Cliente (Cascade) | ⚠️ apagar bloqueado se houver entrega/pedido |
| `cliente_pj` | Operacional | Dados PJ 1:1 | Não | Sim (B) | →Cliente (Cascade) | Vai junto com cliente |
| `pets` | Operacional | Pets | Não | Sim (B) | Receita(Personalizada)→Pet (Restrict) | ⚠️ apagar bloqueado se houver receita personalizada |
| `planos_alimentares` (+`plano_itens_receita`, `plano_item_pacotes`) | Operacional | Planos por pet | Não | Sim (B) | →Receita (Restrict), →Tamanho (Restrict) | Médio |
| `entregas` (+`entrega_pets`, `entrega_itens`, `entrega_item_pacotes`, `entrega_item_ingredientes`, `entrega_historico`) | Operacional | Entregas e snapshots | Não | Sim (B) | filhos Cascade; →Cliente (Restrict) | Apagar libera apagar cliente |
| `pedidos` / `pedido_itens` | Operacional | Pedidos PJ | Não | Sim (B) | filhos Cascade; →Cliente (Restrict) | Médio |
| `ordens_producao` / `fichas_producao` / `ficha_producao_ingredientes` / `consumo_ingrediente_producao` | Operacional | Produção + histórico de rendimento | Não | Sim (B) | filhos Cascade | ⚠️ apaga histórico de rendimentos |
| `rotas` / `rota_paradas` | Operacional | Rotas/saídas | Não | Sim (B) | paradas Cascade | Baixo |
| `itens_estoque` | Estoque/Cadastro | Insumos + produto acabado | **Cuidado (C)** | Depende | mov./lotes/entradas/ajustes→Item (Restrict) | ⚠️ vínculo Ingrediente↔Item; bloqueado se houver movimento |
| `lotes_estoque` / `movimentacoes_estoque` / `entradas_estoque` / `ajustes_estoque` | Estoque/Mov. | Livro-razão | Cuidado (C) | Se forem teste | →Item (Restrict) | ⚠️ limpar zera saldos/custos |
| `fornecedores` | Cadastro/apoio | Fornecedores | Cuidado (C) | Se forem teste | Item→Fornecedor | Baixo |

**Total:** ~37 tabelas de aplicação + `__EFMigrationsHistory`.

---

## 7. Tabelas que DEVEM ser preservadas (Grupo A)

- `categorias_ingredientes`, `ingredientes`
- `receitas` **(apenas linhas `tipo = Casa`)** e seus `itens_receita`
- `faixas_consumo` (tabela de consumo)
- `frequencias_entrega`
- `tamanhos_pacote`
- `usuarios`, `papeis`, `usuarios_papeis`
- (auxiliar) `__EFMigrationsHistory` — **nunca** limpar (controla migrations)

---

## 8. Tabelas candidatas à limpeza futura (Grupo B) — sem executar agora

- `clientes`, `cliente_pj`, `pets`
- `planos_alimentares`, `plano_itens_receita`, `plano_item_pacotes`
- `receitas` **(apenas `tipo = Personalizada`)** + seus `itens_receita`
- `pedidos`, `pedido_itens`
- `entregas` e todos os filhos (`entrega_pets`, `entrega_itens`, `entrega_item_pacotes`, `entrega_item_ingredientes`, `entrega_historico`)
- `ordens_producao`, `fichas_producao`, `ficha_producao_ingredientes`, `consumo_ingrediente_producao`
- `rotas`, `rota_paradas`
- `refresh_tokens` (sessões)

**Ordem de limpeza correta (por causa dos Restrict):** primeiro filhos/operacionais (entregas, pedidos, produção, rotas, planos, receitas personalizadas), depois pets, depois clientes. Caso contrário o banco bloqueia (FK Restrict).

---

## 9. Tabelas que exigem CUIDADO (Grupo C)

- `itens_estoque` — contém **dois mundos**: produto acabado (gerado pela operação, pode limpar) **e** insumos vinculados 1:1 a ingredientes (pode ser cadastro real a preservar). **Não limpar em bloco.**
- `lotes_estoque`, `movimentacoes_estoque`, `entradas_estoque`, `ajustes_estoque` — se forem só testes, podem ir; mas limpá-los **zera saldos e custo médio**. Decidir junto com `itens_estoque`.
- `fornecedores` — provavelmente cadastro real; confirmar.
- `receitas`/`itens_receita` — **mistas** (Casa preservar, Personalizada limpar): exige limpeza **filtrada por `tipo`**, nunca `TRUNCATE`.

---

## 10. Mocks e dados fictícios encontrados

| Arquivo | Mock/Dado fictício | Impacto | Precisa corrigir? |
|---|---|---|---|
| `frontend/.../entregas/entregas.model.ts` | `MOCK_CLIENTES`, `MOCK_PETS` (Bidu/Thor/Luna...), `ESTOQUE_MOCK`, `prontaMock()`, `gerarEntregasMock()` | Entregas mostra dados fictícios quando há < 3 entregas reais | **Sim, antes de uso real** (remover fallback ou baixar o limiar para 0) |
| `frontend/.../entregas/entregas.component.ts` | Lógica `usandoMock`/`MIN_REAIS = 3` | Mesma coisa | Sim |
| `frontend/.../central-operacional...scss` e `receitas/ingredientes/producao` `.scss` | classe CSS `.mock`/`.mock-banner` | Só estilo (a Central não usa mais mock; o rótulo "dados de exemplo" já foi removido) | Não (inofensivo) |
| Backend `Seed/*DataSeeder.cs` | Dados-semente de catálogo (ingredientes, faixas, frequências, tamanhos) | Rodam no startup | ❓ **Confirmar idempotência** (não devem sobrescrever cadastros editados) |
| `IdentityDataSeeder.cs` | Usuário administrador inicial | Cria admin se não existir | ❓ Confirmar que não recria/zera senha alterada |

> Não há `TODO`/`FIXME` relevantes no código. A Central **não** usa mais mock (consome API real).

---

## 11. Riscos críticos

### 🔴 Crítico
1. **Migrations pendentes aplicadas automaticamente no startup.**
   - *Onde:* `SistemaAN.Api/Program.cs` → `await db.Database.MigrateAsync()` + seeders; migrations recentes (`AddSobraPerdaConsumoProducao`, `AddUsuarioCadastroCampos`, `AddClientePjPedidos`, `AddEstoqueBaixadoEntrega`, `AddPreferenciaHorario`, `AddRotas`) provavelmente **não aplicadas** (API não rebuildou desde o bloqueio do MCR).
   - *Por que é problema:* no próximo start, todas serão aplicadas de uma vez. A `AddClientePjPedidos` é estrutural (adiciona `natureza` em `clientes`, `pedido_id` em `entregas`, torna `entrega_pets.pet_id` **nullable** com drop/recreate de FK). Se houver qualquer inconsistência com o estado atual do banco, o `MigrateAsync` falha e **a API não sobe**.
   - *Impacto operacional:* sistema fora do ar até resolver.
   - *Recomendação:* **backup antes**; subir a API uma vez em ambiente controlado e observar o log; confirmar com `dotnet ef migrations list` (no checklist da seção 14).
   - *Prioridade:* **Urgente** (é o próximo passo natural quando o Docker liberar).

2. **Autorização por papel só no frontend.**
   - *Onde:* 19 dos 23 controllers usam apenas `[Authorize]`.
   - *Por que é problema:* qualquer usuário autenticado (inclusive Cozinha) pode chamar a API de cadastros/clientes/estoque/produção diretamente, ignorando o menu.
   - *Impacto:* risco de alteração indevida por perfil sem permissão.
   - *Recomendação:* adicionar `[Authorize(Roles = ...)]` nos controllers conforme a regra de perfis (ex.: cadastros e estoque = Admin/Operador; Produção do Dia/Cozinha = todos para leitura). **Não implementado aqui (somente diagnóstico).**
   - *Prioridade:* **Alta** (antes de uso multiusuário real).

### 🟠 Alto
3. **Tela de Entregas com mock de fallback** (`< 3 entregas reais`) — pode confundir o operador com clientes/pets fictícios. *Recomendação:* baixar o limiar para 0 ou remover o fallback antes do uso real.
4. **Produto acabado não cadastrado é ignorado silenciosamente na finalização da produção** (`ProducaoService`), embora **bloqueie** na hora de Entregar. *Recomendação:* registrar pendência/aviso na finalização. (A Entrega já está protegida.)
5. **Drift de ModelSnapshot tolerado** (`PendingModelChangesWarning` ignorado em `Infrastructure/DependencyInjection.cs`). *Por quê:* o app sobe mesmo com snapshot desatualizado; se uma migration manual estiver incompleta, a coluna pode não existir em runtime. *Recomendação:* ao subir a API, validar que todas as colunas novas existem (checklist).

### 🟡 Médio
6. **`itens_estoque` mistura insumo (cadastro) e produto acabado (operacional)** — risco em limpeza (Grupo C).
7. **`receitas` mistura Casa (preservar) e Personalizada (limpar)** — limpeza precisa filtrar por `tipo`.
8. **Cozido real não obrigatório** na finalização (só o cru é) — pode deixar rendimento incompleto.
9. **Criação de usuário sem dupla confirmação no backend** (validada só no front).
10. **Backend não compilado neste ambiente** — as últimas features (Central real, Venda avulsa, Estoque 3 abas, KPIs novos) ainda **não foram compiladas/rodadas**. ❓ Confirmar build ao subir.

### 🔵 Baixo
11. Validação de e-mail simples (só formato básico).
12. `FaixaConsumo`/`TamanhoPacote` sem proteção de sobreposição/peso>0 a nível de banco (validação fica no service).
13. Cancelar/Concluir rota sem ação explícita (mitigado pelo filtro de rota ativa).
14. Botões "Em breve" na Central e atalho de rota nas Entregas (cosméticos).

---

## 12. Pendências funcionais

- Remover/ível ligar os 2 botões "Em breve" da Central (abrir produção).
- Atalho "Planejar rota do dia" nas Entregas ainda mostra toast em vez de navegar para `/rotas`.
- Mock de Entregas a desativar antes do uso real (limiar de 3).
- (Opcional) Alerta de preferência incompatível também no backend (hoje só no front).
- (Opcional) Registrar pendência quando produto acabado da Casa não está cadastrado, na finalização da produção.

## 13. Pendências técnicas

- **Autorização por papel no backend** (vários controllers).
- **Confirmar aplicação das migrations pendentes** + idempotência dos seeders.
- **Snapshot drift**: validar consistência após o próximo start.
- **Compilar/rodar o backend** para confirmar as features recentes.
- Diferença de validação entre Receita da Casa (não checa ingrediente ativo) e Personalizada (checa) — alinhar ou documentar.

---

## 14. Validação de builds e CHECKLIST para você rodar no PC

**Neste ambiente (nuvem):**
- ✅ **Frontend build:** PASSOU (`ng build` — bundle gerado em `frontend/dist/sistemaan`).
- ❌ **Backend build:** **não executado** — sem `dotnet` no ambiente.
- ❌ **API subiu / migrations aplicaram:** **não validado** — sem Docker/banco aqui.
- ❌ **Contagem de registros:** **não validado** — sem acesso ao banco.

**Checklist seguro (somente leitura) para você rodar no seu PC e me colar a saída** — assim eu completo as partes não validadas:

```bash
# 1) Build da API (confirma que o código recente compila)
docker compose build api    2>&1 | tail -30
#   (ou, se tiver .NET local: dotnet build backend/src/SistemaAN.sln)

# 2) Containers no ar?
docker compose ps

# 3) Migrations aplicadas x pendentes (precisa do dotnet-ef; só leitura)
#   dentro de backend/src/SistemaAN.Api:
dotnet ef migrations list 2>&1 | tail -40

# 4) Saúde da API e Swagger
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:8080/health
#   abrir http://localhost:8080/swagger

# 5) Contagem de registros (SOMENTE SELECT — nada destrutivo)
docker exec -it sistemaan-db psql -U sistemaan -d sistemaan -c "
SELECT 'clientes' t, count(*) FROM clientes
UNION ALL SELECT 'pets', count(*) FROM pets
UNION ALL SELECT 'receitas_casa', count(*) FROM receitas WHERE tipo='Casa'
UNION ALL SELECT 'receitas_pers', count(*) FROM receitas WHERE tipo='Personalizada'
UNION ALL SELECT 'ingredientes', count(*) FROM ingredientes
UNION ALL SELECT 'itens_estoque', count(*) FROM itens_estoque
UNION ALL SELECT 'movimentacoes', count(*) FROM movimentacoes_estoque
UNION ALL SELECT 'entregas', count(*) FROM entregas
UNION ALL SELECT 'pedidos', count(*) FROM pedidos
UNION ALL SELECT 'usuarios', count(*) FROM usuarios
ORDER BY 1;"
```

> ⚠️ Não rode `delete`, `truncate`, `drop`, `update` ou `insert`. Os comandos acima são todos leitura/observação.

---

## 15. Recomendações antes de limpar dados

**Ainda NÃO é seguro limpar.** Antes:

1. **Subir a API com sucesso** (resolver o bloqueio do registry) e confirmar que **todas as migrations aplicaram** sem erro (item crítico nº 1).
2. **Backup completo** do banco (`pg_dump`) — guardado fora do container.
3. **Confirmar idempotência dos seeders** — garantir que reiniciar a API **não sobrescreve** os cadastros reais editados (ingredientes, faixas, frequências, tamanhos, admin).
4. **Confirmar a separação por `tipo` em `receitas`** (Casa preservar / Personalizada limpar) — limpeza **filtrada**, nunca `TRUNCATE`.
5. **Decidir o destino do estoque** (Grupo C): manter insumos/itens reais e movimentações, ou zerar tudo e recadastrar saldo inicial.
6. Definir a **ordem de exclusão** respeitando as FKs Restrict (operacional → pets → clientes).

## 16. Próximos passos recomendados (ordem)

1. **Subir a API** e validar migrations + seeders (rodar o checklist da seção 14 e me enviar a saída).
2. **Fechar autorização por papel no backend** (risco nº 2) — antes de liberar para Operador/Cozinha de verdade.
3. **Desativar o mock de Entregas** (limiar 3 → 0 ou remoção do fallback).
4. **Confirmar preservação** das tabelas do Grupo A e a separação por `tipo` em `receitas`.
5. **Backup** do banco.
6. **Montar o script de limpeza** (Grupo B), em **transação**, na ordem correta de FKs — para revisão **antes** de executar.
7. **Executar a limpeza** (em transação, com backup pronto) e **validar** o sistema limpo (login, cadastros intactos, criar 1 cliente→pet→entrega ponta a ponta).

---

### Anexos — evidências principais
- Auto-migrate: `SistemaAN.Api/Program.cs` (`MigrateAsync()` + seeders).
- Snapshot drift tolerado: `SistemaAN.Infrastructure/DependencyInjection.cs` (`Ignore(PendingModelChangesWarning)`).
- Autorização: `SistemaAN.Api/Controllers/*` (só Usuarios/Central/VendasAvulsas com `Roles`).
- Senha: `SistemaAN.Infrastructure/Identity/Pbkdf2PasswordHasher.cs`.
- Estoque dinâmico: `SistemaAN.Application/Estoque/ProdutoAcabadoService.cs`.
- Mock de entregas: `frontend/src/app/features/entregas/entregas.model.ts` + `entregas.component.ts`.
- FKs on-delete: `SistemaAN.Infrastructure/Persistence/Configurations/*Configuration.cs`.
