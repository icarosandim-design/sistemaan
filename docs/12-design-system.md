# 12. Design System oficial

> **Obrigatório.** Todas as telas, componentes e estilos do projeto devem seguir
> este Design System. Os tokens são a fonte da verdade — não usar cores/fontes
> "soltas".

## Conceito visual

- Sistema operacional moderno, estilo **ERP de gestão**.
- Visual **limpo**, baixa poluição, foco em **produtividade**.
- **Alta densidade de informação** (densidade compacta nos componentes).
- Consistência absoluta via tokens.

## Paleta (tokens `--an-*`)

| Uso | Token | Hex |
|---|---|---|
| Texto / Títulos | `--an-texto-titulo` | `#2E3A21` |
| Primária | `--an-primaria` | `#3F4F2D` |
| Hover Primária | `--an-primaria-hover` | `#7A8450` |
| Detalhes Suaves | `--an-detalhe-suave` | `#A8B17E` |
| Texto Secundário | `--an-texto-secundario` | `#5C6B3A` |
| Destaque / CTA | `--an-cta` | `#B08D57` |
| Hover CTA | `--an-cta-hover` | `#8C6B3F` |
| Fundo Principal | `--an-fundo` | `#F4EFE5` |
| Fundo Secundário | `--an-fundo-secundario` | `#E3D9C6` |
| Superfícies / Cards | `--an-superficie` | `#FFFFFF` |
| Texto Corpo | `--an-texto-corpo` | `#3A3A2E` |

Regras de uso:
- **Primária** em navegação, itens ativos, botões primários.
- **CTA (dourado)** apenas em ações de destaque (ex.: "Novo", confirmações principais).
- **Fundo Principal** no corpo; **cards/superfícies** em branco; **Fundo Secundário** para áreas de apoio (cabeçalhos de tabela, faixas).

## Tipografia

| Uso | Fonte | Token |
|---|---|---|
| Títulos | **Montserrat** | `--an-fonte-titulo` |
| Corpo / UI | **Source Sans** (Source Sans 3) | `--an-fonte-corpo` |

- Carregadas via Google Fonts no `index.html`.
- Observação: o Google Fonts hospeda a versão atual como **"Source Sans 3"**
  (sucessora do "Source Sans Pro"); o stack inclui ambos os nomes como fallback.
- `h1..h6` usam Montserrat automaticamente (estilo global).

## Biblioteca de componentes e ícones

- **Angular Material** é a biblioteca principal de componentes.
- **Material Icons** é a biblioteca de ícones (`<mat-icon>` / classe `.material-icons`).
- Tema configurado em `src/styles.scss` via `mat.theme()` (Material 3), com:
  - `primary` aproximado ao verde da marca, `tertiary` ao dourado;
  - **densidade compacta** (`density: -1`) para ERP;
  - tipografia Montserrat (brand) + Source Sans (plain);
  - **override dos system tokens** (`--mat-sys-*`) para as cores exatas da marca.

## Padrões de UI (layout e telas)

- **Topbar fixa** (identificação do usuário, ações globais, logout).
- **Menu lateral recolhível** (sidenav) com itens de navegação; item ativo em primária.
- **Dashboard baseado em cards** (KPIs e blocos de resumo).
- **Tabelas** com **filtros** e **paginação** (`mat-table` + `mat-paginator` + `mat-sort`).
- **Dialogs** (`mat-dialog`) para criação/edição rápida.
- Espaçamento e raio consistentes: `--an-raio` (10px), `--an-raio-sm` (8px).
- Sombra de card: `--an-sombra-card`.

## Tokens técnicos

Definidos em `:root` (em `src/styles.scss`):
- Paleta `--an-*`, tipografia `--an-fonte-*`, raio `--an-raio*`, sombra `--an-sombra-card`.
- Material: `--mat-sys-*` sobrescritos para a marca.

## Como aplicar (regras para novas telas)

1. Nunca hardcode cores/fontes — usar os tokens `--an-*`.
2. Componentes de formulário, tabela, diálogo, botões: **usar Angular Material**.
3. Títulos em Montserrat; texto/labels em Source Sans (herdado do global).
4. Botão primário = primária; botão de destaque/ação principal = CTA dourado.
5. Layout interno sempre dentro do shell (topbar + sidenav) — exceto telas de
   autenticação (login), que são full-screen.
6. Densidade compacta e foco em densidade de informação.

## Exceção declarada

A **Tela de Login** usa layout full-screen próprio (fora do shell de Material),
porém **segue a paleta e a tipografia** oficiais.
