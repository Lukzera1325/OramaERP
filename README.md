# Orama ERP

Sistema ERP web desenvolvido em ASP.NET Core MVC para o mercado brasileiro.

## 🏛️ Tema Visual
Inspirado na beleza clássica da Grécia, especialmente Santorini, com cores predominantes branco e azul, proporcionando uma interface limpa, moderna e elegante.

## 🛠️ Stack Tecnológica
- **.NET 8** (LTS)
- **ASP.NET Core MVC**
- **Entity Framework Core** (Code-First com Migrations)
- **PostgreSQL** (via Npgsql)
- **HTML5, CSS3, JavaScript**
- **XML** (para NFe e integrações fiscais)
- **JSON** (APIs REST)
- **WebServices/REST APIs** (SOA/REST)
- **Bootstrap** para responsividade
- **Git** para versionamento

## 🏗️ Arquitetura
Solução organizada em camadas (Clean Architecture simplificada):

- **Orama.Web** - Interface web (MVC, Views, Controllers, ViewModels)
- **Orama.Application** - Serviços de aplicação, DTOs, casos de uso
- **Orama.Domain** - Entidades de domínio, interfaces, regras de negócio
- **Orama.Infra.Data** - EF Core, DbContext, Migrations, Repositórios
- **Orama.Infra.CrossCutting** - Serviços externos (CNPJ, CEP, NFe)

## 📋 Módulos Principais

### 🔐 Segurança e Controle de Acesso
- **Usuários** - Cadastro e autenticação
- **Perfis/Grupos** - Administrador, Financeiro, Vendas, Compras, Estoque, Fiscal
- **Permissões Granulares** - Controle por módulo e função (Incluir, Alterar, Excluir, Visualizar)
- **Sistema Maleável** - Configuração flexível de permissões por perfil

### 📊 Cadastros Básicos
- **Clientes** - Dados completos com integração CNPJ automática
- **Fornecedores** - Cadastro similar aos clientes
- **Produtos** - Código, descrição, NCM, preços, estoque
- **NCM** - Nomenclatura Comum do Mercosul
- **Natureza de Operação** - Venda, Compra, Devolução, Transferência
- **CFOP** - Código Fiscal de Operações e Prestações
- **Bancos e Contas Bancárias**
- **Formas de Pagamento**

### 💰 Financeiro Completo
- **Contas a Receber** - Títulos, baixas, situações
- **Contas a Pagar** - Despesas, pagamentos
- **Fluxo de Caixa** - Projeções e saldos
- **Contas Bancárias** - Movimentações e saldos

### 🛒 Vendas
- **Pedidos de Venda** - Cabeçalho e itens
- **Faturamento** - Geração automática de títulos e baixa de estoque
- **Relatórios** - Vendas por período, cliente, produto

### 📦 Compras
- **Pedidos de Compra** - Solicitações aos fornecedores
- **Entrada de Notas** - Registro de NF de compra
- **Integração** - Atualização automática de estoque e contas a pagar

### 📋 Estoque/Almoxarifado
- **Movimentações** - Entrada, saída, transferências, ajustes
- **Posição de Estoque** - Saldos por produto
- **Inventário** - Contagem e ajustes

### 🧾 Fiscal e Tributário
- **Parametrização Inteligente** - Regras por Natureza de Operação + NCM + CFOP
- **Impostos** - ICMS, IPI, PIS, COFINS com CST/CSOSN
- **Configuração por UF** - Origem/destino e regime tributário

### 📄 Emissão de NFe
- **Certificados Digitais** - Suporte A1/A3
- **Ambiente de Homologação** - Testes com SEFAZ
- **Geração de XML** - Layout oficial da NFe
- **Assinatura Digital** - Certificado do emitente
- **Comunicação SEFAZ** - Envio, consulta, protocolo

## 🌐 Integrações Externas
- **Consulta CNPJ** - Preenchimento automático via API da Receita Federal
- **Consulta CEP** - Busca automática de endereços
- **APIs REST** - Estrutura preparada para múltiplos provedores

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- PostgreSQL 12+
- Visual Studio 2022 ou VS Code
- Git

### Configuração Inicial
1. Clone o repositório: `git clone [url-do-repo]`
2. Configure PostgreSQL e crie o banco `orama_erp`
3. Ajuste a connection string no `appsettings.json`
4. Execute as migrations: `dotnet ef database update`
5. Execute o projeto: `dotnet run --project Orama.Web`
6. Acesse: `https://localhost:5001`

### Usuário Padrão
- **Login:** admin@orama.com.br
- **Senha:** Admin@123

## 📝 Objetivo e Metodologia

### Propósito
- **Estudo Didático** - Preparação para vaga de emprego
- **Transição Tecnológica** - De Windows Forms para Web
- **Código Limpo** - Simplicidade e organização
- **Base Comercial** - Estrutura para futura comercialização

### Metodologia de Desenvolvimento
1. **Iterativo e Incremental** - Módulos implementados gradualmente
2. **Código Comentado** - Explicações didáticas nos pontos importantes
3. **Boas Práticas** - SOLID, Clean Code, padrões de mercado
4. **Versionamento** - Commits organizados por funcionalidade

## 🎯 Roadmap de Desenvolvimento

### Fase 1 - Fundação
- [x] Estrutura da solução
- [x] Configuração EF Core + PostgreSQL
- [x] Layout base inspirado em Santorini
- [x] Autenticação básica

### Fase 2 - Segurança
- [ ] Sistema de usuários
- [ ] Perfis e permissões
- [ ] Controle de acesso granular

### Fase 3 - Cadastros
- [ ] Clientes com integração CNPJ
- [ ] Fornecedores
- [ ] Produtos, NCM, CFOP
- [ ] Natureza de operação

### Fase 4 - Financeiro
- [ ] Contas a receber/pagar
- [ ] Fluxo de caixa
- [ ] Contas bancárias

### Fase 5 - Operacional
- [ ] Vendas e faturamento
- [ ] Compras e entrada de notas
- [ ] Controle de estoque

### Fase 6 - Fiscal
- [ ] Parametrização tributária
- [ ] Cálculo de impostos
- [ ] Emissão de NFe

## 🤝 Contribuição
Projeto didático em desenvolvimento. Sugestões e melhorias são bem-vindas!

## 📄 Licença
Projeto de estudo - Todos os direitos reservados.