using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Orama.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                    InscricaoEstadual = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    InscricaoMunicipal = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Complemento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Uf = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    RegimeTributario = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Acao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TipoPessoa = table.Column<int>(type: "INTEGER", nullable: false),
                    CpfCnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                    RgIe = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Celular = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Endereco = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Complemento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Uf = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    LimiteCredito = table.Column<decimal>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContasBancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Banco = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Agencia = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    NumeroConta = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TipoConta = table.Column<int>(type: "INTEGER", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "TEXT", nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "TEXT", nullable: false),
                    ContaPadrao = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasBancarias_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TipoPessoa = table.Column<int>(type: "INTEGER", nullable: false),
                    CpfCnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                    RgIe = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Celular = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Contato = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Complemento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Uf = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Banco = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Agencia = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    ContaBancaria = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TipoConta = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ChavePix = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PrazoPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fornecedores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Senha = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    UltimoLogin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataUltimoAcesso = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PerfilId = table.Column<int>(type: "INTEGER", nullable: false),
                    Permissoes = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Usuarios_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilPermissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PerfilId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Concedida = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilPermissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilPermissoes_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilPermissoes_Permissoes_PermissaoId",
                        column: x => x.PermissaoId,
                        principalTable: "Permissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DescricaoDetalhada = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Unidade = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Ncm = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Cest = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PrecoCusto = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrecoMinimo = table.Column<decimal>(type: "TEXT", nullable: false),
                    MargemLucro = table.Column<decimal>(type: "TEXT", nullable: false),
                    ControlaEstoque = table.Column<bool>(type: "INTEGER", nullable: false),
                    EstoqueAtual = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstoqueMinimo = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstoqueMaximo = table.Column<decimal>(type: "TEXT", nullable: false),
                    Peso = table.Column<decimal>(type: "TEXT", nullable: true),
                    Altura = table.Column<decimal>(type: "TEXT", nullable: true),
                    Largura = table.Column<decimal>(type: "TEXT", nullable: true),
                    Profundidade = table.Column<decimal>(type: "TEXT", nullable: true),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Categoria = table.Column<string>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Produtos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesFinanceiras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContaBancariaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DataMovimentacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", nullable: false),
                    SaldoAnterior = table.Column<decimal>(type: "TEXT", nullable: false),
                    SaldoPosterior = table.Column<decimal>(type: "TEXT", nullable: false),
                    ContaReceberId = table.Column<int>(type: "INTEGER", nullable: true),
                    ContaPagarId = table.Column<int>(type: "INTEGER", nullable: true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CompraId = table.Column<int>(type: "INTEGER", nullable: true),
                    Conciliada = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataConciliacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesFinanceiras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesFinanceiras_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesFinanceiras_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroCompra = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FornecedorId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroNF = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataCompra = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataEntrega = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataRecebimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorFrete = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    FormaPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    Parcelas = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Compras_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Compras_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioEmpresas",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataVinculo = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioEmpresas", x => new { x.UsuarioId, x.EmpresaId });
                    table.ForeignKey(
                        name: "FK_UsuarioEmpresas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioEmpresas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    VendedorId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataVenda = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataEntrega = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorFrete = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    FormaPagamento = table.Column<int>(type: "INTEGER", nullable: false),
                    Parcelas = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vendas_Usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ListasMateriais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Versao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataVigencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    QuantidadeBase = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CustoTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioCriacaoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListasMateriais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListasMateriais_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListasMateriais_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ListasMateriais_Usuarios_UsuarioCriacaoId",
                        column: x => x.UsuarioCriacaoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataMovimentacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstoqueAnterior = table.Column<decimal>(type: "TEXT", nullable: false),
                    EstoquePosterior = table.Column<decimal>(type: "TEXT", nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "TEXT", nullable: true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CompraId = table.Column<int>(type: "INTEGER", nullable: true),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrdensProducao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadePlanejada = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantidadeProduzida = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataPlanejada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Prioridade = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CustoMaterial = table.Column<decimal>(type: "TEXT", nullable: false),
                    CustoMaoObra = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioCriacaoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensProducao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensProducao_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdensProducao_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdensProducao_Usuarios_UsuarioCriacaoId",
                        column: x => x.UsuarioCriacaoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompraItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompraId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompraItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompraItens_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompraItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContasPagar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FornecedorId = table.Column<int>(type: "INTEGER", nullable: true),
                    CompraId = table.Column<int>(type: "INTEGER", nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NumeroDocumento = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValorOriginal = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorJuros = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorMulta = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorPago = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ContaBancariaId = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusAprovacao = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioAprovadorId = table.Column<int>(type: "INTEGER", nullable: true),
                    DataAprovacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoReprovacao = table.Column<string>(type: "TEXT", nullable: true),
                    DataAgendamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasPagar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasPagar_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContasPagar_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContasPagar_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContasPagar_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ContasReceber",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NumeroDocumento = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValorOriginal = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorJuros = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorMulta = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorRecebido = table.Column<decimal>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ContaBancariaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasReceber", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasReceber_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ContasReceber_ContasBancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "ContasBancarias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContasReceber_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContasReceber_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotasFiscais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Serie = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataSaida = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    FornecedorId = table.Column<int>(type: "INTEGER", nullable: true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CompraId = table.Column<int>(type: "INTEGER", nullable: true),
                    ValorProdutos = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorFrete = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorSeguro = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorOutrasDespesas = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorIPI = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorICMS = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorPIS = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorCOFINS = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    NaturezaOperacao = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CFOP = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    ChaveAcesso = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Protocolo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataAutorizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InformacaoComplementar = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ObservacaoFisco = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioCriacaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioAlteracaoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasFiscais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NotasFiscais_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VendaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VendaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendaItens_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListaMateriaisItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ListaMateriaisId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Critico = table.Column<bool>(type: "INTEGER", nullable: false),
                    PercentualPerda = table.Column<decimal>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaMateriaisItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListaMateriaisItens_ListasMateriais_ListaMateriaisId",
                        column: x => x.ListaMateriaisId,
                        principalTable: "ListasMateriais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ListaMateriaisItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdemProducaoEtapas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemProducaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Sequencia = table.Column<int>(type: "INTEGER", nullable: false),
                    TempoEstimado = table.Column<decimal>(type: "TEXT", nullable: false),
                    TempoRealizado = table.Column<decimal>(type: "TEXT", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponsavelId = table.Column<int>(type: "INTEGER", nullable: true),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemProducaoEtapas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemProducaoEtapas_OrdensProducao_OrdemProducaoId",
                        column: x => x.OrdemProducaoId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdemProducaoEtapas_Usuarios_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrdemProducaoItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemProducaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeNecessaria = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantidadeConsumida = table.Column<decimal>(type: "TEXT", nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemProducaoItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemProducaoItens_OrdensProducao_OrdemProducaoId",
                        column: x => x.OrdemProducaoId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdemProducaoItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasFiscaisItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NotaFiscalId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Unidade = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Quantidade = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "TEXT", nullable: false),
                    CFOP = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    NCM = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CST = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    BaseCalculoICMS = table.Column<decimal>(type: "TEXT", nullable: false),
                    AliquotaICMS = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorICMS = table.Column<decimal>(type: "TEXT", nullable: false),
                    BaseCalculoIPI = table.Column<decimal>(type: "TEXT", nullable: false),
                    AliquotaIPI = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorIPI = table.Column<decimal>(type: "TEXT", nullable: false),
                    BaseCalculoPIS = table.Column<decimal>(type: "TEXT", nullable: false),
                    AliquotaPIS = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorPIS = table.Column<decimal>(type: "TEXT", nullable: false),
                    BaseCalculoCOFINS = table.Column<decimal>(type: "TEXT", nullable: false),
                    AliquotaCOFINS = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorCOFINS = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasFiscaisItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasFiscaisItens_NotasFiscais_NotaFiscalId",
                        column: x => x.NotaFiscalId,
                        principalTable: "NotasFiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasFiscaisItens_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApontamentosHoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemProducaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    EtapaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FuncionarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HorasTrabalhadas = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorHora = table.Column<decimal>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioCriacaoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApontamentosHoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApontamentosHoras_OrdemProducaoEtapas_EtapaId",
                        column: x => x.EtapaId,
                        principalTable: "OrdemProducaoEtapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ApontamentosHoras_OrdensProducao_OrdemProducaoId",
                        column: x => x.OrdemProducaoId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApontamentosHoras_Usuarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApontamentosHoras_Usuarios_UsuarioCriacaoId",
                        column: x => x.UsuarioCriacaoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspecoesQualidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrdemProducaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    EtapaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeInspecionada = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantidadeAprovada = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantidadeRejeitada = table.Column<decimal>(type: "TEXT", nullable: false),
                    Resultado = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInspecao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InspetorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AcaoCorretiva = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspecoesQualidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspecoesQualidade_OrdemProducaoEtapas_EtapaId",
                        column: x => x.EtapaId,
                        principalTable: "OrdemProducaoEtapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InspecoesQualidade_OrdensProducao_OrdemProducaoId",
                        column: x => x.OrdemProducaoId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspecoesQualidade_Usuarios_InspetorId",
                        column: x => x.InspetorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NaoConformidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    InspecaoQualidadeId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrdemProducaoId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProdutoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Severidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataDeteccao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DetectadoPorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponsavelId = table.Column<int>(type: "INTEGER", nullable: true),
                    CausaRaiz = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AcaoCorretiva = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AcaoPreventiva = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataPrazo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataResolucao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ObservacoesResolucao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NaoConformidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NaoConformidades_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NaoConformidades_InspecoesQualidade_InspecaoQualidadeId",
                        column: x => x.InspecaoQualidadeId,
                        principalTable: "InspecoesQualidade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NaoConformidades_OrdensProducao_OrdemProducaoId",
                        column: x => x.OrdemProducaoId,
                        principalTable: "OrdensProducao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NaoConformidades_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NaoConformidades_Usuarios_DetectadoPorId",
                        column: x => x.DetectadoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NaoConformidades_Usuarios_ResponsavelId",
                        column: x => x.ResponsavelId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Ativo", "Bairro", "Cep", "Cidade", "Cnpj", "Complemento", "DataAtualizacao", "DataCriacao", "Email", "InscricaoEstadual", "InscricaoMunicipal", "Logradouro", "NomeFantasia", "Numero", "RazaoSocial", "RegimeTributario", "Telefone", "Uf" },
                values: new object[,]
                {
                    { 1, true, null, null, null, "00.000.000/0001-00", null, null, new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4139), "contato@oramademo.com.br", null, null, null, "Orama Demo", null, "Empresa Demonstração Ltda", 1, null, null },
                    { 2, true, null, null, null, "11.111.111/0001-11", null, null, new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4142), "contato@techsolutions.com.br", null, null, null, "Tech Solutions", null, "Tech Solutions Brasil Ltda", 1, null, null },
                    { 3, true, null, null, null, "22.222.222/0001-22", null, null, new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4144), "contato@abccomercio.com.br", null, null, null, "ABC Comércio", null, "Comércio Geral ABC Ltda", 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "Perfis",
                columns: new[] { "Id", "Ativo", "DataAtualizacao", "DataCriacao", "Descricao", "Nome" },
                values: new object[] { 1, true, null, new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4368), "Acesso total ao sistema", "Administrador" });

            migrationBuilder.InsertData(
                table: "Permissoes",
                columns: new[] { "Id", "Acao", "Ativo", "DataAtualizacao", "DataCriacao", "Descricao", "Modulo", "Nome" },
                values: new object[,]
                {
                    { 1, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2483), "Visualizar usuários", "Segurança", "Usuarios.Visualizar" },
                    { 2, "Incluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2483), "Incluir usuários", "Segurança", "Usuarios.Incluir" },
                    { 3, "Alterar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2484), "Alterar usuários", "Segurança", "Usuarios.Alterar" },
                    { 4, "Excluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2484), "Excluir usuários", "Segurança", "Usuarios.Excluir" },
                    { 5, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2484), "Visualizar clientes", "Cadastros", "Clientes.Visualizar" },
                    { 6, "Incluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2485), "Incluir clientes", "Cadastros", "Clientes.Incluir" },
                    { 7, "Alterar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2485), "Alterar clientes", "Cadastros", "Clientes.Alterar" },
                    { 8, "Excluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2486), "Excluir clientes", "Cadastros", "Clientes.Excluir" },
                    { 9, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2486), "Visualizar produtos", "Cadastros", "Produtos.Visualizar" },
                    { 10, "Incluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2487), "Incluir produtos", "Cadastros", "Produtos.Incluir" },
                    { 11, "Alterar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2487), "Alterar produtos", "Cadastros", "Produtos.Alterar" },
                    { 12, "Excluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2487), "Excluir produtos", "Cadastros", "Produtos.Excluir" },
                    { 13, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2488), "Visualizar fornecedores", "Cadastros", "Fornecedores.Visualizar" },
                    { 14, "Incluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2488), "Incluir fornecedores", "Cadastros", "Fornecedores.Incluir" },
                    { 15, "Alterar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2489), "Alterar fornecedores", "Cadastros", "Fornecedores.Alterar" },
                    { 16, "Excluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2489), "Excluir fornecedores", "Cadastros", "Fornecedores.Excluir" },
                    { 17, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2489), "Visualizar financeiro", "Financeiro", "Financeiro.Visualizar" },
                    { 18, "Incluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2490), "Incluir lançamentos", "Financeiro", "Financeiro.Incluir" },
                    { 19, "Alterar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2490), "Alterar lançamentos", "Financeiro", "Financeiro.Alterar" },
                    { 20, "Excluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2491), "Excluir lançamentos", "Financeiro", "Financeiro.Excluir" },
                    { 21, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2491), "Visualizar vendas", "Vendas", "Vendas.Visualizar" },
                    { 22, "Incluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2491), "Incluir vendas", "Vendas", "Vendas.Incluir" },
                    { 23, "Alterar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2492), "Alterar vendas", "Vendas", "Vendas.Alterar" },
                    { 24, "Excluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2492), "Excluir vendas", "Vendas", "Vendas.Excluir" },
                    { 25, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2493), "Visualizar compras", "Compras", "Compras.Visualizar" },
                    { 26, "Incluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2493), "Incluir compras", "Compras", "Compras.Incluir" },
                    { 27, "Alterar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2493), "Alterar compras", "Compras", "Compras.Alterar" },
                    { 28, "Excluir", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2494), "Excluir compras", "Compras", "Compras.Excluir" },
                    { 29, "Visualizar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2494), "Visualizar estoque", "Estoque", "Estoque.Visualizar" },
                    { 30, "Movimentar", true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2495), "Movimentar estoque", "Estoque", "Estoque.Movimentar" }
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Ativo", "DataAtualizacao", "DataCriacao", "Descricao", "EmpresaId", "Nome" },
                values: new object[,]
                {
                    { 1, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3557), "Produtos eletrônicos", 1, "Eletrônicos" },
                    { 2, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3559), "Produtos de informática", 1, "Informática" },
                    { 3, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3561), "Móveis e decoração", 1, "Móveis" },
                    { 4, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3563), "Licenças de software", 2, "Software" },
                    { 5, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3565), "Equipamentos de TI", 2, "Hardware" },
                    { 6, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3566), "Serviços de consultoria", 2, "Serviços" },
                    { 7, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3568), "Produtos alimentícios", 3, "Alimentação" },
                    { 8, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3577), "Produtos de limpeza", 3, "Limpeza" },
                    { 9, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3579), "Produtos de higiene pessoal", 3, "Higiene" }
                });

            migrationBuilder.InsertData(
                table: "ContasBancarias",
                columns: new[] { "Id", "Agencia", "Ativo", "Banco", "ContaPadrao", "DataAtualizacao", "DataCriacao", "Descricao", "EmpresaId", "NumeroConta", "SaldoAtual", "SaldoInicial", "TipoConta" },
                values: new object[,]
                {
                    { 1, null, true, null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3706), "Caixa Geral", 1, null, 1000.00m, 1000.00m, 3 },
                    { 2, "1234", true, "001", false, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3710), "Banco do Brasil - CC 12345-6", 1, "12345-6", 50000.00m, 50000.00m, 1 },
                    { 3, null, true, null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3712), "Caixa Geral", 2, null, 2000.00m, 2000.00m, 3 },
                    { 4, "9876", true, "341", false, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3715), "Itaú - CC 98765-4", 2, "98765-4", 120000.00m, 120000.00m, 1 },
                    { 5, null, true, null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3717), "Caixa Geral", 3, null, 500.00m, 500.00m, 3 },
                    { 6, "5555", true, "104", false, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3728), "Caixa Econômica - CC 55555-5", 3, "55555-5", 25000.00m, 25000.00m, 1 }
                });

            migrationBuilder.InsertData(
                table: "PerfilPermissoes",
                columns: new[] { "Id", "Ativo", "Concedida", "DataAtualizacao", "DataCriacao", "PerfilId", "PermissaoId" },
                values: new object[,]
                {
                    { 1, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3327), 1, 1 },
                    { 2, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3339), 1, 2 },
                    { 3, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3341), 1, 3 },
                    { 4, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3343), 1, 4 },
                    { 5, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3344), 1, 5 },
                    { 6, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3347), 1, 6 },
                    { 7, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3348), 1, 7 },
                    { 8, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3349), 1, 8 },
                    { 9, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3350), 1, 9 },
                    { 10, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3353), 1, 10 },
                    { 11, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3354), 1, 11 },
                    { 12, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3355), 1, 12 },
                    { 13, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3356), 1, 13 },
                    { 14, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3358), 1, 14 },
                    { 15, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3359), 1, 15 },
                    { 16, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3360), 1, 16 },
                    { 17, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3361), 1, 17 },
                    { 18, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3369), 1, 18 },
                    { 19, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3370), 1, 19 },
                    { 20, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3372), 1, 20 },
                    { 21, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3373), 1, 21 },
                    { 22, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3374), 1, 22 },
                    { 23, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3375), 1, 23 },
                    { 24, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3376), 1, 24 },
                    { 25, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3377), 1, 25 },
                    { 26, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3378), 1, 26 },
                    { 27, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3380), 1, 27 },
                    { 28, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3381), 1, 28 },
                    { 29, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3382), 1, 29 },
                    { 30, true, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3392), 1, 30 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Ativo", "DataAtualizacao", "DataCriacao", "DataUltimoAcesso", "Email", "EmpresaId", "IsSuperAdmin", "Nome", "PerfilId", "Permissoes", "Senha", "Telefone", "UltimoLogin" },
                values: new object[] { 1, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2118), null, "admin@orama.com.br", 1, true, "Administrador", 1, "[]", "$2a$11$nZ/FWIV4VvxrxD3O6BKsaujiei3TsiStDF60i7kkirwUcAm6JO1eC", null, null });

            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "Altura", "Ativo", "Categoria", "CategoriaId", "Cest", "Codigo", "CodigoBarras", "ControlaEstoque", "DataAtualizacao", "DataCriacao", "Descricao", "DescricaoDetalhada", "EmpresaId", "EstoqueAtual", "EstoqueMaximo", "EstoqueMinimo", "Largura", "MargemLucro", "Ncm", "Observacoes", "Peso", "PrecoCusto", "PrecoMinimo", "PrecoVenda", "Profundidade", "Unidade" },
                values: new object[,]
                {
                    { 1, null, true, null, 2, null, "NOTEBOOK001", null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3629), "Notebook Dell Inspiron 15", null, 1, 10m, 50m, 2m, null, 28.00m, "84713012", null, null, 2500.00m, 0m, 3200.00m, null, "UN" },
                    { 2, null, true, null, 2, null, "MOUSE001", null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3633), "Mouse Óptico USB", null, 1, 50m, 200m, 10m, null, 66.67m, "84716070", null, null, 15.00m, 0m, 25.00m, null, "UN" },
                    { 3, null, true, null, 4, null, "OFFICE365", null, false, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3636), "Microsoft Office 365 Business", null, 2, 0m, 0m, 0m, null, 38.89m, "85234910", null, null, 180.00m, 0m, 250.00m, null, "LIC" },
                    { 4, null, true, null, 5, null, "SERVIDOR001", null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3640), "Servidor Dell PowerEdge T340", null, 2, 3m, 10m, 1m, null, 41.18m, "84713012", null, null, 8500.00m, 0m, 12000.00m, null, "UN" },
                    { 5, null, true, null, 7, null, "ARROZ001", null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3643), "Arroz Branco Tipo 1 - 5kg", null, 3, 200m, 500m, 50m, null, 51.20m, "10063021", null, null, 12.50m, 0m, 18.90m, null, "PCT" },
                    { 6, null, true, null, 8, null, "DETERGENTE001", null, true, null, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3647), "Detergente Líquido 500ml", null, 3, 150m, 300m, 30m, null, 94.44m, "34022000", null, null, 1.80m, 0m, 3.50m, null, "UN" }
                });

            migrationBuilder.InsertData(
                table: "UsuarioEmpresas",
                columns: new[] { "EmpresaId", "UsuarioId", "DataVinculo", "IsAdmin" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2393), true },
                    { 2, 1, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2394), true },
                    { 3, 1, new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2396), true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApontamentosHoras_EtapaId",
                table: "ApontamentosHoras",
                column: "EtapaId");

            migrationBuilder.CreateIndex(
                name: "IX_ApontamentosHoras_FuncionarioId",
                table: "ApontamentosHoras",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ApontamentosHoras_OrdemProducaoId",
                table: "ApontamentosHoras",
                column: "OrdemProducaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ApontamentosHoras_UsuarioCriacaoId",
                table: "ApontamentosHoras",
                column: "UsuarioCriacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_EmpresaId",
                table: "Categorias",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EmpresaId",
                table: "Clientes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CompraItens_CompraId",
                table: "CompraItens",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_CompraItens_ProdutoId",
                table: "CompraItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_EmpresaId",
                table: "Compras",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_FornecedorId",
                table: "Compras",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasBancarias_EmpresaId",
                table: "ContasBancarias",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_CompraId",
                table: "ContasPagar",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_ContaBancariaId",
                table: "ContasPagar",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_EmpresaId",
                table: "ContasPagar",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasPagar_FornecedorId",
                table: "ContasPagar",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_ClienteId",
                table: "ContasReceber",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_ContaBancariaId",
                table: "ContasReceber",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_EmpresaId",
                table: "ContasReceber",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContasReceber_VendaId",
                table: "ContasReceber",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_EmpresaId",
                table: "Fornecedores",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_InspecoesQualidade_EtapaId",
                table: "InspecoesQualidade",
                column: "EtapaId");

            migrationBuilder.CreateIndex(
                name: "IX_InspecoesQualidade_InspetorId",
                table: "InspecoesQualidade",
                column: "InspetorId");

            migrationBuilder.CreateIndex(
                name: "IX_InspecoesQualidade_OrdemProducaoId",
                table: "InspecoesQualidade",
                column: "OrdemProducaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaMateriaisItens_ListaMateriaisId",
                table: "ListaMateriaisItens",
                column: "ListaMateriaisId");

            migrationBuilder.CreateIndex(
                name: "IX_ListaMateriaisItens_ProdutoId",
                table: "ListaMateriaisItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ListasMateriais_EmpresaId",
                table: "ListasMateriais",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ListasMateriais_ProdutoId",
                table: "ListasMateriais",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ListasMateriais_UsuarioCriacaoId",
                table: "ListasMateriais",
                column: "UsuarioCriacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_EmpresaId",
                table: "MovimentacoesEstoque",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId",
                table: "MovimentacoesEstoque",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_UsuarioId",
                table: "MovimentacoesEstoque",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesFinanceiras_ContaBancariaId",
                table: "MovimentacoesFinanceiras",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesFinanceiras_EmpresaId",
                table: "MovimentacoesFinanceiras",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_NaoConformidades_DetectadoPorId",
                table: "NaoConformidades",
                column: "DetectadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_NaoConformidades_EmpresaId_Numero",
                table: "NaoConformidades",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NaoConformidades_InspecaoQualidadeId",
                table: "NaoConformidades",
                column: "InspecaoQualidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_NaoConformidades_OrdemProducaoId",
                table: "NaoConformidades",
                column: "OrdemProducaoId");

            migrationBuilder.CreateIndex(
                name: "IX_NaoConformidades_ProdutoId",
                table: "NaoConformidades",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_NaoConformidades_ResponsavelId",
                table: "NaoConformidades",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_ChaveAcesso",
                table: "NotasFiscais",
                column: "ChaveAcesso",
                unique: true,
                filter: "[ChaveAcesso] IS NOT NULL AND [ChaveAcesso] != ''");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_ClienteId",
                table: "NotasFiscais",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_CompraId",
                table: "NotasFiscais",
                column: "CompraId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_EmpresaId_Numero_Serie_Tipo",
                table: "NotasFiscais",
                columns: new[] { "EmpresaId", "Numero", "Serie", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_FornecedorId",
                table: "NotasFiscais",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscais_VendaId",
                table: "NotasFiscais",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscaisItens_NotaFiscalId",
                table: "NotasFiscaisItens",
                column: "NotaFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasFiscaisItens_ProdutoId",
                table: "NotasFiscaisItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemProducaoEtapas_OrdemProducaoId",
                table: "OrdemProducaoEtapas",
                column: "OrdemProducaoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemProducaoEtapas_ResponsavelId",
                table: "OrdemProducaoEtapas",
                column: "ResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemProducaoItens_OrdemProducaoId",
                table: "OrdemProducaoItens",
                column: "OrdemProducaoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemProducaoItens_ProdutoId",
                table: "OrdemProducaoItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensProducao_EmpresaId_Numero",
                table: "OrdensProducao",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensProducao_ProdutoId",
                table: "OrdensProducao",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensProducao_UsuarioCriacaoId",
                table: "OrdensProducao",
                column: "UsuarioCriacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilPermissoes_PerfilId_PermissaoId",
                table: "PerfilPermissoes",
                columns: new[] { "PerfilId", "PermissaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilPermissoes_PermissaoId",
                table: "PerfilPermissoes",
                column: "PermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CategoriaId",
                table: "Produtos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_EmpresaId_Codigo",
                table: "Produtos",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEmpresas_EmpresaId",
                table: "UsuarioEmpresas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId",
                table: "Usuarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PerfilId",
                table: "Usuarios",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaItens_ProdutoId",
                table: "VendaItens",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaItens_VendaId",
                table: "VendaItens",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ClienteId",
                table: "Vendas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_EmpresaId",
                table: "Vendas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_VendedorId",
                table: "Vendas",
                column: "VendedorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApontamentosHoras");

            migrationBuilder.DropTable(
                name: "CompraItens");

            migrationBuilder.DropTable(
                name: "ContasPagar");

            migrationBuilder.DropTable(
                name: "ContasReceber");

            migrationBuilder.DropTable(
                name: "ListaMateriaisItens");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "MovimentacoesFinanceiras");

            migrationBuilder.DropTable(
                name: "NaoConformidades");

            migrationBuilder.DropTable(
                name: "NotasFiscaisItens");

            migrationBuilder.DropTable(
                name: "OrdemProducaoItens");

            migrationBuilder.DropTable(
                name: "PerfilPermissoes");

            migrationBuilder.DropTable(
                name: "UsuarioEmpresas");

            migrationBuilder.DropTable(
                name: "VendaItens");

            migrationBuilder.DropTable(
                name: "ListasMateriais");

            migrationBuilder.DropTable(
                name: "ContasBancarias");

            migrationBuilder.DropTable(
                name: "InspecoesQualidade");

            migrationBuilder.DropTable(
                name: "NotasFiscais");

            migrationBuilder.DropTable(
                name: "Permissoes");

            migrationBuilder.DropTable(
                name: "OrdemProducaoEtapas");

            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "OrdensProducao");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Perfis");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
