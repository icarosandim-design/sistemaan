# 11. Módulo de autenticação

Autenticação por **JWT** com **refresh token rotativo**, papéis (roles) e usuário
ativo/inativo. Nesta fase há apenas o papel **Administrador**, mas o modelo já
suporta múltiplos papéis.

## Modelo de dados

| Tabela | Campos principais | Observações |
|---|---|---|
| `usuarios` | id, nome, email (UQ), senha_hash, ativo, created_at, updated_at | Hash PBKDF2 (HMAC-SHA256) |
| `papeis` | id, nome (UQ), descricao | Ex.: "Administrador" |
| `usuarios_papeis` | usuario_id (FK), papeis_id (FK) | Junção N:N |
| `refresh_tokens` | id, token (UQ), usuario_id (FK), expires_at, created_at, revoked_at | Rotação + revogação (logout) |

Índices: `ix_usuarios_email` (único), `ix_papeis_nome` (único),
`ix_refresh_tokens_token` (único), `ix_refresh_tokens_usuario_id`.
FKs com `on delete cascade` de `refresh_tokens`/`usuarios_papeis` para `usuarios`.

### Relacionamentos
```
Usuario 1 ──< RefreshToken
Usuario >──< Papel        (via usuarios_papeis)
```

## Endpoints

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/api/auth/login` | anônimo | Autentica (email+senha) → access + refresh token |
| POST | `/api/auth/refresh` | anônimo | Renova tokens a partir de um refresh válido (rotação) |
| POST | `/api/auth/logout` | autenticado | Revoga o refresh token informado |
| GET | `/api/auth/me` | autenticado | Dados do usuário a partir das claims |
| GET | `/api/auth/admin-check` | papel Administrador | Exemplo de autorização por papel |

### Respostas de erro (ProblemDetails / RFC 7807)
| Situação | HTTP |
|---|---|
| Credenciais inválidas | 401 |
| Refresh inválido/expirado/revogado | 401 |
| Usuário inativo | 403 |
| Sem token em rota protegida | 401 |
| Sem papel exigido | 403 |

## Fluxo de autenticação

```
LOGIN ─► valida senha (PBKDF2) ─► verifica ativo ─► emite:
          access token (JWT, exp. 15 min)   +   refresh token (exp. 7 dias, persistido)

Requisições ─► Authorization: Bearer <access token> ─► [Authorize]/[Authorize(Roles=...)]

REFRESH ─► valida refresh ativo ─► REVOGA o usado (rotação) ─► emite novos tokens
LOGOUT  ─► revoga o refresh token ─► refresh subsequente falha (401)
```

### Claims do access token
`sub` (id), `email`, `nome`, `jti`, `role` (um por papel). O JwtBearer usa
`RoleClaimType = "role"` e `NameClaimType = "sub"`.

## Configuração

`appsettings` / variáveis de ambiente:

| Chave | Default (dev) | Descrição |
|---|---|---|
| `Jwt:Issuer` | SistemaAN | Emissor |
| `Jwt:Audience` | SistemaAN.Client | Audiência |
| `Jwt:SecretKey` | (env) | Chave de assinatura (≥ 32 chars) |
| `Jwt:AccessTokenExpirationMinutes` | 15 | Validade do access token |
| `Jwt:RefreshTokenExpirationDays` | 7 | Validade do refresh token |
| `Seed:AdminEmail` | admin@sistemaan.local | E-mail do admin inicial |
| `Seed:AdminPassword` | Admin@123 | Senha do admin inicial |

## Seed inicial

Na inicialização, a aplicação aplica as migrations pendentes e executa um seed
**idempotente** que garante o papel "Administrador" e um usuário administrador
(credenciais configuráveis). Trocar a senha padrão em produção.

## Decisões

- **Refresh token rotativo + persistido:** cada refresh emite um novo e revoga o
  anterior; logout revoga explicitamente. Permite invalidar sessões.
- **PBKDF2 nativo** (sem dependências externas), comparação em tempo constante.
- **N:N usuário↔papel desde já**, mesmo com um único papel, para evoluir sem migração estrutural.
- **Autorização por papel** via `[Authorize(Roles = ...)]`; políticas mais finas
  (permissions) podem ser adicionadas depois sem reescrever o modelo.
