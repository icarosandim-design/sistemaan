# 18 — Módulo Plano Alimentar

> Status: **modelagem aprovada** (implementação do backend autorizada).
> Escopo desta fase: Plano Alimentar + Receitas Personalizadas vinculadas ao Pet.
> **Fora do escopo agora:** Entregas, Produção, Estoque, Central real e snapshots de
> Ordem de Produção/Entrega.

## 1. Conceito

Cada **Pet** tem **um Plano Alimentar vigente** (1:1, sem histórico por enquanto;
edição sobrescreve). O plano define como o pet é atendido:

- Frequência de entrega
- Primeira data de entrega
- Gramas/dia sugeridas (retrato da Tabela de Consumo) e ajustadas
- Tipo de alimentação: **Receita da Casa** OU **Receita Personalizada** (nunca os dois)
- Receitas escolhidas, quantidades do ciclo e pacotes do ciclo

## 2. Entidades

### 2.1 `PlanoAlimentar` (cabeçalho)
| Campo | Tipo | Observações |
|---|---|---|
| Id | long | PK |
| PetId | long | **FK Pet, único** (1 plano vigente por pet) |
| FrequenciaEntregaId | long | FK FrequenciaEntrega |
| PrimeiraEntrega | date | |
| GramasDiaSugeridas | int? | retrato da Tabela de Consumo no momento |
| GramasDiaAjustadas | int? | |
| Tipo | enum `TipoReceita` (Casa\|Personalizada) | reusa o enum da Receita |
| Ativo | bool | vigente |
| CreatedAt/UpdatedAt | | auditoria |

Derivados (não persistidos): total necessário, total real enviado, diferença, custos.

### 2.2 `PlanoItemReceita` (receitas do plano)
| Campo | Tipo | Observações |
|---|---|---|
| Id | long | PK |
| PlanoAlimentarId | long | FK PlanoAlimentar (cascade dentro do agregado) |
| ReceitaId | long | FK Receita |
| QuantidadeCicloGramas | int? | **Casa**: quantidade *necessária/distribuída* da receita no ciclo |
| QuantidadePacotes | int? | **Personalizada**: nº de pacotes no ciclo (informado) |

### 2.3 `PlanoItemPacote` (pacotes — **só Receita da Casa**)
| Campo | Tipo | Observações |
|---|---|---|
| Id | long | PK |
| PlanoItemReceitaId | long | FK PlanoItemReceita (cascade) |
| TamanhoPacoteId | long | FK TamanhoPacote |
| Quantidade | int | nº de pacotes desse tamanho |

### 2.4 `Receita` (reuso) — Casa e Personalizada
- Já existe com `Tipo` (Casa\|Personalizada), `Codigo`, `Nome`, `Observacoes`, `Ativo`,
  `PetId` (nullable) e `Itens` (`ItemReceita`: ingrediente + gramas cozidas).
- **Mudança**: `PetId` passa a ser **FK real** para `Pet` (hoje é coluna solta).
- Receita Personalizada = `Tipo=Personalizada` + `PetId` obrigatório; criada/editada
  **dentro do Plano do pet**, exclusiva do pet. **Não** criar entidade paralela.

## 3. Relacionamentos
```
Pet 1—1 PlanoAlimentar              (PetId único)
PlanoAlimentar N—1 FrequenciaEntrega
PlanoAlimentar 1—N PlanoItemReceita
PlanoItemReceita N—1 Receita
PlanoItemReceita 1—N PlanoItemPacote        (só Casa)
PlanoItemPacote N—1 TamanhoPacote
Receita(Personalizada) N—1 Pet              (PetId FK)
Receita 1—N ItemReceita N—1 Ingrediente
```
Comportamento de FK: referências para Receita/TamanhoPacote/Frequência/Pet =
**Restrict** (não apagam em cascata); itens do agregado do plano = **Cascade**.

## 4. Receita da Casa no plano — necessário × real enviado

- `QuantidadeCicloGramas` = quantidade **necessária/distribuída** (referência operacional).
- `PlanoItemPacote` = pacotes **reais** que serão enviados/produzidos.
- **Total real enviado = Σ(Quantidade × TamanhoPacote.PesoGramas)** — *derivado*.
- **Diferença = total real − necessário**.

> **Regra-chave:** o que move **Entregas, Produção e Estoque** é o **total real dos
> pacotes** (ex.: 6.300g necessários → 13×500g = 6.500g → usa-se 6.500g). O necessário
> serve só para comparação operacional.

## 5. Receita Personalizada no plano

- Não usa Tamanhos de Pacote padrão.
- **Pacote = a própria receita**; tamanho = **Σ(gramas cozidas dos itens)** (derivado).
- Operador informa `QuantidadePacotes` no ciclo (distribuição livre entre receitas).
- Total no ciclo = tamanho da receita × `QuantidadePacotes`.
- Sistema calcula: peso total da receita, custo do pacote, custo/kg cozido, custo no ciclo.

Exemplo (VET-001 Scooby: batata 300g + frango 150g + cenoura 50g = 500g/pacote; 15 pacotes):
- Batata 300×15 = 4.500g · Frango 150×15 = 2.250g · Cenoura 50×15 = 750g.

## 6. Regras de validação

1. 1 plano vigente por pet (`PetId` único).
2. Tipo único — todos os itens casam com `Plano.Tipo`.
3. Casa → receitas `Tipo=Casa`; exige `QuantidadeCicloGramas`; permite `PlanoItemPacote`;
   `QuantidadePacotes` nulo; `TamanhoPacote` deve estar **ativo**.
4. Personalizada → receitas `Tipo=Personalizada` **e** `PetId = Plano.PetId`; exige
   `QuantidadePacotes > 0`; sem `PlanoItemPacote`.
5. Frequência/tamanhos/ingredientes referenciados devem estar **ativos** na seleção.
6. Quantidades > 0; gramas ≥ 0.
7. Código da Receita Personalizada **único por pet**.
8. **Bloquear inativação de itens em uso por plano vigente**:
   - Receita da Casa em plano ativo → não inativar.
   - TamanhoPacote em plano ativo → não inativar.
   - Frequência em plano ativo → não inativar.
   - Ingrediente em receita usada por plano ativo → não inativar.
   - (Futuro: regra de substituição antes de inativar.)

## 7. Inativação x exclusão
Operacionalmente trabalhamos com **inativação**, não exclusão real. Pet, Cliente,
Receita e Plano não são apagados pela tela em uso normal — são inativados para preservar
histórico futuro. Cascades existem só para integridade do agregado.

## 8. Como alimenta os módulos futuros

- **Entregas**: `PrimeiraEntrega` + `Frequencia.DiasCiclo` → agenda (`CalculadoraAgenda`).
  Cada entrega = conteúdo de 1 ciclo (os pacotes do plano). Entrega futura fará **snapshot**.
- **Produção**:
  - Personalizada: `ingrediente = ItemReceita.Gramas × QuantidadePacotes`.
  - Casa: escala a ficha (base 1.000g cozido) pela quantidade real enviada (dos pacotes)
    e soma ingredientes; multiplica pelos ciclos do horizonte. Converte cozido→cru via
    `coeficiente` do Ingrediente para compras.
- **Estoque (produto acabado)**:
  - Casa: pacotes padrão (`TamanhoPacote`) = produtos estocáveis.
  - Personalizada: pacote por pet, sob demanda (produção casada com a entrega).
- **Central Operacional**: contadores (planos vigentes, próximas entregas, produção do período).

## 9. Cuidados antes de implementar
- **Referência agora; snapshot só em Produção/Entrega** (congelar gramas/custos/pacotes lá).
- Edição sobrescreve (sem versão) enquanto não há entregas geradas.
- Bloquear inativação de dependências em uso (item 6.8).
- Unidades: gramas inteiras (cozidas); custo decimal (12,2) sempre **derivado**.
- `Receita.PetId` vira FK (cuidar de receitas personalizadas órfãs — não devem existir ainda).
- Manter a regra de ficha base 1.000g cozido para escalar a Produção corretamente.

## 10. Escopo de implementação autorizado
Entidades, configs EF, migration, services, DTOs, endpoints, validações e integração com
Receita, Pet, Frequência, Tamanho de Pacote e Ingredientes. **Sem** Entregas, Produção,
Estoque, Central real e snapshots.
