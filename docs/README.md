# 📚 DOCUMENTAÇÃO ERP ORAMA

**Sistema ERP Industrial**
**Versão**: 1.0
**Status**: Em auditoria de prontidão; não considerar liberado para produção sem concluir os bloqueios listados em [AUDIT.md](AUDIT.md).
**Última revisão**: 2026-10-01

---

## 🎯 **VISÃO GERAL**

O **ERP Orama** é um sistema empresarial completo desenvolvido em **.NET 8** com **arquitetura enterprise** que oferece gestão integrada para empresas industriais.

### **📊 Escopo**
- O repositório contém módulos web e um aplicativo mobile.
- A compilação Release passou nesta revisão, com quatro avisos de nulabilidade.
- A cobertura funcional e a prontidão de produção ainda não foram certificadas.

### **🏗️ Arquitetura Enterprise**
- **Domain Driven Design** (DDD)
- **Clean Architecture** em camadas
- **Multi-tenant** preparado
- **APIs REST** documentadas

### **📱 Mobile**
- Aplicativo .NET MAUI; login autenticado depende de API configurada e ainda requer validação em dispositivo.

---

## 📋 **MÓDULOS IMPLEMENTADOS**

### **Módulos presentes no repositório**
- **Segurança** - Login, usuários, perfis, permissões
- **Cadastros** - Clientes, fornecedores, produtos, categorias
- **Financeiro** - Contas a receber/pagar, bancos, movimentações
- **Vendas** - Orçamentos, pedidos, faturamento
- **Compras** - Cotações, pedidos, recebimento
- **Estoque** - Movimentações, inventário, relatórios

### **Módulos industriais**
- **Produção** - Ordens, BOM, controle de custos
- **Controle de Custos** - Cálculo automático de custos de produção
- **Margem e Lucro** - Análise de lucratividade por venda
- **Relatórios Gerenciais** - Dashboards e análises
- **Alertas** - Sistema de alertas de margem negativa
- **Explicações** - Sistema que explica resultados financeiros

### **Módulos adicionais**
- **Mobile** - App completo com sincronização
- **APIs** - REST endpoints para integração

---

## 🚀 **COMO EXECUTAR**

### **Pré-requisitos**
- .NET 8 SDK
- Visual Studio 2022 ou VS Code
- SQLite (incluído)

### **Execução Rápida**
```bash
# Clone o repositório
git clone https://github.com/Lukzera1325/OramaERP.git
cd OramaERP

# Execute o sistema
.\executar-sistema.ps1

# Acesse: http://localhost:5000
# Configure uma conta individual antes de entrar no sistema.
```

---

## 📚 **DOCUMENTAÇÃO TÉCNICA**

### **Para Desenvolvedores**
- **[Backend](backend.md)** - Documentação completa do sistema web
- **[Mobile](mobile.md)** - Documentação do app mobile
- **[Arquitetura](arquitetura.md)** - Padrões e estrutura do código

### **Para Novos Desenvolvedores**
- **[Guia do Estagiário](guia-estagiario.md)** - Onboarding completo
- Consulte os guias arquivados em [archive](archive/) para materiais históricos.
- **[Fluxos de Negócio](fluxos-negocio.md)** - Como o sistema funciona

### Engenharia e operação
- [Auditoria](AUDIT.md)
- [Revisão de segurança](security-review.md)
- [Multi-tenancy](multi-tenancy.md)
- [Inventário de documentação](documentation-inventory.md)
- [Compose local](../docker-compose.yml)
- Documentos legados foram preservados em [archive](archive/).

---

## 🎯 **FUNCIONALIDADES PRINCIPAIS**

### **Ciclo Industrial Completo**
1. **Compra de Componentes** → Custo médio calculado
2. **Produção** → Custo de produção automático
3. **Venda** → Margem e lucro calculados
4. **Análise** → Relatórios e alertas automáticos

### **Sistema Inteligente**
- **Alertas Automáticos** - Avisa sobre vendas com prejuízo
- **Explicações Claras** - Explica por que uma venda deu lucro/prejuízo
- **Relatórios Gerenciais** - Produtos mais lucrativos, evolução de margem
- **App Mobile** - Força de vendas com sincronização offline

---

## **Pendências**
- Migrations PostgreSQL, execução de CI, cenários transacionais de vendas/estoque e validação do mobile ainda precisam ser verificados.

Consulte [AUDIT.md](AUDIT.md), [security-review.md](security-review.md) e [FINAL_REVIEW.md](FINAL_REVIEW.md) antes de qualquer liberação.
