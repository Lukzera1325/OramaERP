# 👨‍🎓 GUIA DO ESTAGIÁRIO - ERP ORAMA

**Bem-vindo ao time de desenvolvimento!**

Este guia vai te ajudar a entender o sistema e começar a contribuir rapidamente.

---

## 🎯 **O QUE É O ERP ORAMA?**

O ERP Orama é um **sistema de gestão empresarial** para indústrias. Ele controla:

- **Vendas** - Clientes, orçamentos, pedidos
- **Compras** - Fornecedores, cotações, pedidos
- **Estoque** - Produtos, movimentações, inventário
- **Produção** - Ordens de produção, custos, BOM
- **Financeiro** - Contas a receber/pagar, bancos
- **Relatórios** - Análises de lucratividade e performance

### **Por que é especial?**
- **Sistema Inteligente** - Calcula custos e margens automaticamente
- **Alertas Automáticos** - Avisa quando algo está errado
- **Explicações Claras** - Explica por que uma venda deu lucro ou prejuízo
- **App Mobile** - Força de vendas funciona offline

---

## 🏗️ **COMO O SISTEMA ESTÁ ORGANIZADO?**

### **Estrutura de Pastas**
```
src/
├── Orama.Domain/          # Regras de negócio (entidades)
├── Orama.Application/     # Casos de uso (services)
├── Orama.Infra.Data/      # Banco de dados
├── Orama.Infra.CrossCutting/ # Utilitários
└── Orama.Web/             # Interface web (controllers + views)

mobile/OramaGo/            # App mobile (.NET MAUI)
```

### **Arquitetura Simples**
```
Controller → Service → Domain → Database
    ↑         ↑         ↑
Interface  Casos de   Regras de
  Web       Uso      Negócio
```

---

## 🧠 **CONCEITOS IMPORTANTES**

### **1. Domain Driven Design (DDD)**
- **Entidades** = Objetos do mundo real (Cliente, Produto, Venda)
- **Services** = Casos de uso (CriarVenda, CalcularCusto)
- **Controllers** = Recebem requests e chamam services

### **2. Padrão do Sistema**
```csharp
// Controller (SEMPRE simples)
public async Task<IActionResult> Criar(VendaViewModel model)
{
    var sucesso = await _vendaService.CriarAsync(model);
    if (sucesso) TempData["Sucesso"] = "Venda criada!";
    return RedirectToAction("Index");
}

// Service (Coordena o caso de uso)
public async Task<bool> CriarAsync(VendaViewModel model)
{
    var venda = new Venda(model.ClienteId, model.Produtos);
    _context.Vendas.Add(venda);
    return await _context.SaveChangesAsync() > 0;
}

// Domain (Contém as regras)
public class Venda
{
    public void Faturar()
    {
        Status = StatusVenda.Faturada;
        CalcularMargemELucro(); // Regra de negócio aqui!
    }
}
```

---

## 🚀 **PRIMEIROS PASSOS**

### **1. Configure o Ambiente**
```bash
# Clone o projeto
git clone https://github.com/Lukzera1325/OramaERP.git
cd OramaERP

# Execute o sistema
.\executar-sistema.ps1

# Abra no VS Code
code .
```

### **2. Acesse o Sistema**
- **URL**: http://localhost:5000
- **Login**: solicite uma conta individual de desenvolvimento ao responsável pelo ambiente.
- **Senha**: use a credencial provisionada para você; não reutilize contas de demonstração.

### **3. Explore os Módulos**
1. **Cadastros** → Produtos, Clientes, Fornecedores
2. **Vendas** → Criar uma venda e faturar
3. **Produção** → Criar ordem de produção
4. **Relatórios** → Ver análises de lucratividade
5. **Alertas** → Ver alertas de margem negativa

---

## 📚 **FLUXOS PRINCIPAIS**

### **Fluxo de Venda**
```
1. Cliente faz pedido
2. Criar Venda (Status: Orçamento)
3. Faturar Venda
   ├── Calcula margem e lucro automaticamente
   ├── Gera alertas se margem negativa
   ├── Cria explicação do resultado
   └── Atualiza estoque
```

### **Fluxo de Produção**
```
1. Criar Ordem de Produção
2. Liberar (verifica estoque de componentes)
3. Iniciar produção
4. Finalizar
   ├── Calcula custo de produção
   ├── Consome componentes do estoque
   └── Adiciona produto acabado ao estoque
```

### **Fluxo de Compra**
```
1. Criar Pedido de Compra
2. Receber produtos
   ├── Atualiza custo médio dos produtos
   └── Adiciona ao estoque
```

---

## 🔧 **TAREFAS COMUNS**

### **Adicionar um Campo Novo**

**1. Na Entidade (Domain)**
```csharp
// src/Orama.Domain/Entities/Produto.cs
public class Produto
{
    public string NovoCampo { get; set; } // Adicionar aqui
}
```

**2. No ViewModel (Web)**
```csharp
// src/Orama.Web/Models/ProdutoViewModel.cs
public class ProdutoViewModel
{
    public string NovoCampo { get; set; } // Adicionar aqui
}
```

**3. Na View (Interface)**
```html
<!-- src/Orama.Web/Views/Produtos/Criar.cshtml -->
<div class="form-group">
    <label asp-for="NovoField">Novo Campo</label>
    <input asp-for="NovoField" class="form-control" />
</div>
```

### **Criar um Novo Relatório**

**1. No Service**
```csharp
public async Task<List<RelatorioDto>> ObterRelatorioAsync()
{
    return await _context.Vendas
        .Where(v => v.Status == StatusVenda.Faturada)
        .Select(v => new RelatorioDto { ... })
        .ToListAsync();
}
```

**2. No Controller**
```csharp
public async Task<IActionResult> NovoRelatorio()
{
    var dados = await _service.ObterRelatorioAsync();
    return View(dados);
}
```

**3. Na View**
```html
<table class="table">
    @foreach(var item in Model)
    {
        <tr>
            <td>@item.Nome</td>
            <td>@item.Valor.ToString("C")</td>
        </tr>
    }
</table>
```

---

## 🎯 **REGRAS DE OURO**

### **1. Simplicidade Sempre**
- ✅ Código que um estagiário entende
- ✅ Nomes autoexplicativos
- ✅ Métodos curtos (máximo 20 linhas)
- ❌ Abstrações desnecessárias

### **2. Padrão do Sistema**
- ✅ Controllers simples (receber → chamar → retornar)
- ✅ Services coordenam casos de uso
- ✅ Domain contém regras de negócio
- ✅ Um método = uma responsabilidade

### **3. Onde Colocar Cada Coisa**
- **Validações** → Domain (entidades)
- **Cálculos** → Domain (entidades)
- **Consultas** → Services
- **Interface** → Controllers + Views
- **Regras de negócio** → Domain

---

## 🐛 **COMO DEBUGGAR**

### **1. Erro de Compilação**
```bash
# Ver erros detalhados
dotnet build

# Restaurar pacotes se necessário
dotnet restore
```

### **2. Erro em Runtime**
- **F12** no navegador → Console para ver erros JavaScript
- **Breakpoints** no Visual Studio/VS Code
- **Logs** no console da aplicação

### **3. Problemas Comuns**
- **Erro 404** → Rota não existe, verificar Controller/Action
- **Erro 500** → Erro no servidor, ver console da aplicação
- **Página em branco** → Erro na View, verificar sintaxe Razor

---

## 📖 **RECURSOS PARA APRENDER**

### **Tecnologias Usadas**
- **.NET 8** - Framework principal
- **Entity Framework** - Banco de dados
- **ASP.NET MVC** - Interface web
- **Bootstrap** - CSS framework
- **jQuery** - JavaScript

### **Documentação Oficial**
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Entity Framework](https://docs.microsoft.com/ef/core)
- [Bootstrap](https://getbootstrap.com/docs)

### **Dentro do Projeto**
- **[Arquitetura](arquitetura.md)** - Como o sistema está estruturado
- Materiais históricos de convenções ficam em [archive](archive/), quando disponíveis.
- **[Backend](backend.md)** - Documentação técnica completa

---

## 🎊 **DICAS FINAIS**

### **Para Começar Bem**
1. **Explore o sistema** - Use todas as funcionalidades
2. **Leia o código** - Comece pelos Controllers mais simples
3. **Faça perguntas** - Melhor perguntar que fazer errado
4. **Teste sempre** - Execute o sistema após cada mudança

### **Para Evoluir**
1. **Entenda o negócio** - Por que cada funcionalidade existe
2. **Siga os padrões** - Mantenha consistência
3. **Documente** - Comente código complexo
4. **Seja simples** - Clareza > sofisticação

---

**Bem-vindo ao time! 🚀**

**Qualquer dúvida, consulte a documentação ou pergunte para o time.**
