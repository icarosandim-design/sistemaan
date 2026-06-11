# Documentação Oficial — Sistema de Gestão de Alimentação Natural para Cães

> **Esta documentação é a fonte oficial de verdade do projeto.**
> Toda decisão de negócio, modelagem e arquitetura deve estar refletida aqui.
> Mudanças no sistema que contrariem este documento devem primeiro atualizar o documento.

## Stack tecnológica

| Camada | Tecnologia |
|---|---|
| Frontend | Angular |
| Backend | ASP.NET Core (.NET) |
| Banco de dados | PostgreSQL |
| ORM | Entity Framework Core |
| Autenticação | JWT (com refresh token) |
| Ambiente | Docker (dev e produção) |

## Índice

1. [Visão geral do negócio](01-visao-geral-negocio.md)
2. [Módulos do sistema](02-modulos-sistema.md)
3. [Entidades e relacionamentos](03-entidades-relacionamentos.md)
4. [Regras de negócio](04-regras-de-negocio.md)
5. [Fluxos operacionais](05-fluxos-operacionais.md)
6. [Máquina de estados](06-maquina-de-estados.md)
7. [Modelo físico do banco](07-modelo-fisico-banco.md)
8. [Decisões arquiteturais](08-decisoes-arquiteturais.md)
9. [Riscos e premissas](09-riscos-e-premissas.md)
10. [Fundação técnica (infraestrutura)](10-fundacao-tecnica.md)
11. [Módulo de autenticação](11-modulo-autenticacao.md)
12. [Design System oficial](12-design-system.md)
13. [Módulo de Ingredientes](13-modulo-ingredientes.md)
14. [Módulo Tabela de Consumo](14-modulo-tabela-consumo.md)
15. [Módulo Receitas da Casa](15-modulo-receitas-casa.md)

## Objetivo central do sistema

Substituir as planilhas operacionais e permitir que a equipe saiba, a qualquer momento:

- **O que precisa ser produzido**
- **O que já está pronto**
- **O que precisa ser entregue**
- **O que existe em estoque**
- **O que está reservado para entregas futuras**

Não é um e-commerce. É um sistema de **gestão operacional** de uma cozinha que
trabalha simultaneamente com **estoque** (Receitas da Casa) e **produção sob
demanda** (Receitas Personalizadas).
