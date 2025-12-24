-- Script de inicialização do banco de dados PostgreSQL
-- Executado automaticamente quando o container é criado

-- Criar banco de produção (além do de desenvolvimento)
CREATE DATABASE orama_erp;

-- Configurações de performance para desenvolvimento
ALTER SYSTEM SET shared_preload_libraries = 'pg_stat_statements';
ALTER SYSTEM SET log_statement = 'all';
ALTER SYSTEM SET log_duration = on;

-- Comentário informativo
COMMENT ON DATABASE orama_erp_dev IS 'Banco de desenvolvimento do Orama ERP';
COMMENT ON DATABASE orama_erp IS 'Banco de produção do Orama ERP';