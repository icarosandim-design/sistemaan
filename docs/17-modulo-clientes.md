# 17. Módulo Clientes (Fase 1)

Cadastro de clientes com dados cadastrais, endereço estruturado, CPF, origem da
venda, financeiro básico embutido e **cancelamento com motivo**.

## Entidade `Cliente`
Cadastrais: nome, **cpf** (11 dígitos, único quando informado), telefone, email,
**origemVenda**, observações.
Endereço: **rua, numero, complemento, cep, bairro, cidade, estado (UF)**.
Situação: ativo, **motivoCancelamento**, **dataCancelamento**.
Financeiro básico: tipoCliente (Assinante/Avulso), formaPagamento (Cartão/Pix/
Dinheiro/Outro), diaCobranca (1–31), valorRecorrenteMensal, statusFinanceiro
(EmDia/Pendente/Inadimplente), observacoesFinanceiras.

Tabela `clientes`: checks dia (1–31) e valor ≥ 0; índice único parcial em CPF
(quando não nulo). Migrations: `AddClientes` e `AddClienteEnderecoCpfCancelamento`.

## Endpoints (exigem JWT)
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/clientes` | Lista |
| GET | `/api/clientes/{id}` | Obtém |
| POST | `/api/clientes` | Cria (valida nome, CPF 11 dígitos/único, dia 1–31) |
| PUT | `/api/clientes/{id}` | Atualiza |
| PUT | `/api/clientes/{id}/cancelar` | Cancela (body `{ motivo }`) |
| PUT | `/api/clientes/{id}/reativar` | Reativa |

> O **cancelamento substitui o "inativar"**: registra motivo + data e marca
> ativo=false. Reativar limpa o cancelamento.

## Tela
Lista (nome, CPF, telefone, cidade/UF, tipo, financeiro, situação) com busca e
filtro; dialog com abas **Dados** (cadastrais + endereço estruturado) e
**Financeiro**; ações **editar**, **cancelar** (com motivo) e **reativar**.
Tudo no Design System. Em **Clientes** no menu.

## Não incluído (futuro)
Pets, Plano Alimentar, Wizard unificado, cobrança automática, financeiro avançado.
