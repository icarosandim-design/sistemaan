-- =============================================================================
-- LIMPEZA DE DADOS OPERACIONAIS / DE TESTE — SistemaAN / Nuri Pet
-- -----------------------------------------------------------------------------
-- ⚠️  OPERAÇÃO DESTRUTIVA. FAÇA BACKUP ANTES DE EXECUTAR.
--     (ver comando de pg_dump nas instruções entregues no chat)
--
-- O QUE FAZ:
--   • Remove os dados OPERACIONAIS/de teste (clientes, pets, planos, entregas,
--     pedidos, produção, rotas, estoque operacional, receitas PERSONALIZADAS).
--   • Remove APENAS o Tamanho de Pacote de 400g.
--
-- O QUE PRESERVA (cadastros reais):
--   • Receitas da Casa (tipo='Casa') e seus itens
--   • Ingredientes, Categorias, Tabela de Consumo, Frequências de Entrega
--   • Tamanhos de Pacote (exceto 400g)
--   • Usuários, Papéis, vínculo usuário×papel
--   • Fornecedores (NÃO estão no escopo de limpeza — preservados)
--
-- SEGURANÇA:
--   • Roda em TRANSAÇÃO única (BEGIN/COMMIT). Qualquer erro → ROLLBACK total,
--     o banco NÃO fica parcialmente limpo.
--   • Não usa DROP/TRUNCATE. Apenas DELETE filtrado, na ordem das FKs.
--
-- COMO EXECUTAR (Docker):
--   docker exec -i sistemaan-db psql -U sistemaan -d sistemaan -v ON_ERROR_STOP=1 \
--     < scripts/limpeza-dados-operacionais.sql
-- =============================================================================

SET search_path TO public;

\echo '==================== CONTAGEM ANTES ===================='
SELECT tabela, total FROM (
  SELECT 1 ord, 'clientes'                 tabela, count(*) total FROM clientes
  UNION ALL SELECT 2,  'cliente_pj',               count(*) FROM cliente_pj
  UNION ALL SELECT 3,  'pets',                     count(*) FROM pets
  UNION ALL SELECT 4,  'planos_alimentares',       count(*) FROM planos_alimentares
  UNION ALL SELECT 5,  'receitas_PERSONALIZADA',   count(*) FROM receitas WHERE tipo = 'Personalizada'
  UNION ALL SELECT 6,  'entregas',                 count(*) FROM entregas
  UNION ALL SELECT 7,  'pedidos',                  count(*) FROM pedidos
  UNION ALL SELECT 8,  'ordens_producao',          count(*) FROM ordens_producao
  UNION ALL SELECT 9,  'fichas_producao',          count(*) FROM fichas_producao
  UNION ALL SELECT 10, 'rotas',                    count(*) FROM rotas
  UNION ALL SELECT 11, 'itens_estoque',            count(*) FROM itens_estoque
  UNION ALL SELECT 12, 'movimentacoes_estoque',    count(*) FROM movimentacoes_estoque
  UNION ALL SELECT 13, 'refresh_tokens',           count(*) FROM refresh_tokens
  UNION ALL SELECT 50, 'PRESERVA receitas_CASA',   count(*) FROM receitas WHERE tipo = 'Casa'
  UNION ALL SELECT 51, 'PRESERVA ingredientes',    count(*) FROM ingredientes
  UNION ALL SELECT 52, 'PRESERVA categorias',      count(*) FROM categorias_ingredientes
  UNION ALL SELECT 53, 'PRESERVA faixas_consumo',  count(*) FROM faixas_consumo
  UNION ALL SELECT 54, 'PRESERVA frequencias',     count(*) FROM frequencias_entrega
  UNION ALL SELECT 55, 'tamanhos_pacote (total)',  count(*) FROM tamanhos_pacote
  UNION ALL SELECT 56, 'tamanho_400g (a remover)', count(*) FROM tamanhos_pacote WHERE peso_gramas = 400
  UNION ALL SELECT 57, 'PRESERVA usuarios',        count(*) FROM usuarios
  UNION ALL SELECT 58, 'PRESERVA papeis',          count(*) FROM papeis
  UNION ALL SELECT 59, 'PRESERVA fornecedores',    count(*) FROM fornecedores
) q ORDER BY ord;

BEGIN;

-- 1) Sessões / tokens
DELETE FROM refresh_tokens;

-- 2) Rotas (paradas → rotas)
DELETE FROM rota_paradas;
DELETE FROM rotas;

-- 3) Produção (consumos/ingredientes/fichas → ordens)
DELETE FROM consumo_ingrediente_producao;
DELETE FROM ficha_producao_ingredientes;
DELETE FROM fichas_producao;
DELETE FROM ordens_producao;

-- 4) Entregas (filhas → mãe). Inclui Venda Avulsa PF (é uma Entrega).
DELETE FROM entrega_item_ingredientes;
DELETE FROM entrega_item_pacotes;
DELETE FROM entrega_itens;
DELETE FROM entrega_pets;
DELETE FROM entrega_historico;
DELETE FROM entregas;

-- 5) Pedidos PJ (itens → pedidos)
DELETE FROM pedido_itens;
DELETE FROM pedidos;

-- 6) Planos alimentares (filhas → mãe)
DELETE FROM plano_item_pacotes;
DELETE FROM plano_itens_receita;
DELETE FROM planos_alimentares;

-- 7) Receitas PERSONALIZADAS (preserva as da Casa).
--    Remove primeiro os itens das personalizadas, depois as receitas personalizadas.
DELETE FROM itens_receita
  WHERE receita_id IN (SELECT id FROM receitas WHERE tipo = 'Personalizada');
DELETE FROM receitas WHERE tipo = 'Personalizada';

-- 8) Pets (após receitas personalizadas, que referenciam pet com Restrict)
DELETE FROM pets;

-- 9) Clientes (PJ 1:1 e base). Entregas/pedidos já removidos (FK Restrict).
DELETE FROM cliente_pj;
DELETE FROM clientes;

-- 10) Estoque operacional (movimentações/entradas/ajustes/lotes → itens).
--     Ingredientes e Categorias NÃO são tocados.
DELETE FROM movimentacoes_estoque;
DELETE FROM ajustes_estoque;
DELETE FROM entradas_estoque;
DELETE FROM lotes_estoque;
DELETE FROM itens_estoque;

-- 11) Remover APENAS o Tamanho de Pacote de 400g (os demais ficam).
DELETE FROM tamanhos_pacote WHERE peso_gramas = 400;

\echo '==================== CONTAGEM DEPOIS (dentro da transação) ===================='
SELECT tabela, total FROM (
  SELECT 1 ord, 'clientes'                 tabela, count(*) total FROM clientes
  UNION ALL SELECT 2,  'cliente_pj',               count(*) FROM cliente_pj
  UNION ALL SELECT 3,  'pets',                     count(*) FROM pets
  UNION ALL SELECT 4,  'planos_alimentares',       count(*) FROM planos_alimentares
  UNION ALL SELECT 5,  'receitas_PERSONALIZADA',   count(*) FROM receitas WHERE tipo = 'Personalizada'
  UNION ALL SELECT 6,  'entregas',                 count(*) FROM entregas
  UNION ALL SELECT 7,  'pedidos',                  count(*) FROM pedidos
  UNION ALL SELECT 8,  'ordens_producao',          count(*) FROM ordens_producao
  UNION ALL SELECT 9,  'fichas_producao',          count(*) FROM fichas_producao
  UNION ALL SELECT 10, 'rotas',                    count(*) FROM rotas
  UNION ALL SELECT 11, 'itens_estoque',            count(*) FROM itens_estoque
  UNION ALL SELECT 12, 'movimentacoes_estoque',    count(*) FROM movimentacoes_estoque
  UNION ALL SELECT 13, 'refresh_tokens',           count(*) FROM refresh_tokens
  UNION ALL SELECT 50, 'PRESERVA receitas_CASA',   count(*) FROM receitas WHERE tipo = 'Casa'
  UNION ALL SELECT 51, 'PRESERVA ingredientes',    count(*) FROM ingredientes
  UNION ALL SELECT 52, 'PRESERVA categorias',      count(*) FROM categorias_ingredientes
  UNION ALL SELECT 53, 'PRESERVA faixas_consumo',  count(*) FROM faixas_consumo
  UNION ALL SELECT 54, 'PRESERVA frequencias',     count(*) FROM frequencias_entrega
  UNION ALL SELECT 55, 'tamanhos_pacote (total)',  count(*) FROM tamanhos_pacote
  UNION ALL SELECT 56, 'tamanho_400g (deve ser 0)',count(*) FROM tamanhos_pacote WHERE peso_gramas = 400
  UNION ALL SELECT 57, 'PRESERVA usuarios',        count(*) FROM usuarios
  UNION ALL SELECT 58, 'PRESERVA papeis',          count(*) FROM papeis
  UNION ALL SELECT 59, 'PRESERVA fornecedores',    count(*) FROM fornecedores
) q ORDER BY ord;

-- Se a contagem acima estiver correta, o COMMIT abaixo grava as alterações.
-- Se algo deu errado, troque COMMIT por ROLLBACK e me avise.
COMMIT;

\echo '==================== LIMPEZA CONCLUÍDA ===================='
