# 🚀 Guia de Instalação - Orama ERP

## Pré-requisitos

### 1. .NET 8 SDK
Baixe e instale o .NET 8 SDK:
- **Site oficial:** https://dotnet.microsoft.com/download/dotnet/8.0
- **Versão:** .NET 8.0 (LTS)
- **Tipo:** SDK (não apenas Runtime)

**Verificar instalação:**
```bash
dotnet --version
```

### 2. PostgreSQL
Baixe e instale o PostgreSQL:
- **Site oficial:** https://www.postgresql.org/download/
- **Versão recomendada:** 12 ou superior
- **Configuração padrão:**
  - Usuário: `postgres`
  - Senha: `postgres` (ou defina sua própria)
  - Porta: `5432`

### 3. IDE/Editor (Opcional)
- **Visual Studio 2022** (Community, Professional ou Enterprise)
- **Visual Studio Code** com extensão C#
- **JetBrains Rider**

## 📋 Passo a Passo

### 1. Clone o Repositório
```bash
git clone [url-do-repositorio]
cd orama-erp
```

### 2. Configure o Banco de Dados

#### Criar o banco de dados:
```sql
-- Conecte no PostgreSQL como usuário postgres
CREATE DATABASE orama_erp_dev;
CREATE DATABASE orama_erp; -- Para produção
```

#### Configurar connection string:
Edite o arquivo `src/Orama.Web/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=orama_erp_dev;Username=postgres;Password=SUA_SENHA_AQUI"
  }
}
```

### 3. Restaurar Pacotes
```bash
dotnet restore
```

### 4. Executar Migrations
```bash
cd src/Orama.Web
dotnet ef database update
```

### 5. Executar o Projeto
```bash
dotnet run
```

O sistema estará disponível em:
- **HTTPS:** https://localhost:5001
- **HTTP:** http://localhost:5000

## 🔐 Acesso Inicial

**Acesso inicial:** configure/provisione uma conta administrativa individual. O projeto não inclui credenciais padrão.

## 🛠️ Comandos Úteis

### Entity Framework
```bash
# Adicionar nova migration
dotnet ef migrations add NomeDaMigration

# Atualizar banco de dados
dotnet ef database update

# Reverter migration
dotnet ef database update MigrationAnterior

# Remover última migration
dotnet ef migrations remove
```

### Build e Publicação
```bash
# Build do projeto
dotnet build

# Build para produção
dotnet build --configuration Release

# Publicar aplicação
dotnet publish --configuration Release --output ./publish
```

## 🐛 Solução de Problemas

### Erro de Connection String
- Verifique se o PostgreSQL está rodando
- Confirme usuário, senha e nome do banco
- Teste a conexão com pgAdmin ou outro cliente

### Erro de Migrations
```bash
# Limpar e recriar migrations
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Erro de Pacotes
```bash
# Limpar cache do NuGet
dotnet nuget locals all --clear

# Restaurar pacotes
dotnet restore --force
```

## 📁 Estrutura do Projeto

```
orama-erp/
├── src/
│   ├── Orama.Web/              # Interface web (MVC)
│   ├── Orama.Application/      # Serviços de aplicação
│   ├── Orama.Domain/          # Entidades e regras de negócio
│   ├── Orama.Infra.Data/      # Acesso a dados (EF Core)
│   └── Orama.Infra.CrossCutting/ # Serviços externos
├── README.md
├── INSTALACAO.md
└── Orama.sln
```

## 🎯 Próximos Passos

Após a instalação bem-sucedida:

1. **Explorar o Dashboard** - Familiarize-se com a interface
2. **Módulo de Segurança** - Configure usuários e permissões
3. **Cadastros Básicos** - Clientes, fornecedores, produtos
4. **Módulo Financeiro** - Contas a receber/pagar
5. **Vendas e Compras** - Operações comerciais
6. **Estoque** - Controle de inventário
7. **Fiscal** - Parametrização tributária
8. **NFe** - Emissão de notas fiscais

## 📞 Suporte

Em caso de dúvidas:
- **Email:** suporte@orama.com.br
- **Documentação:** Consulte o README.md
- **Issues:** Abra uma issue no repositório Git
