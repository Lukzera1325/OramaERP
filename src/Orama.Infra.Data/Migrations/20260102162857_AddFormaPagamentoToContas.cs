using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orama.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFormaPagamentoToContas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FormaPagamento",
                table: "ContasReceber",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FormaPagamento",
                table: "ContasPagar",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4381));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4384));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4386));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4388));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4391));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4393));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4395));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4397));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4399));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4520));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4524));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4526));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4529));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4531));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4538));

            migrationBuilder.UpdateData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 584, DateTimeKind.Utc).AddTicks(9602));

            migrationBuilder.UpdateData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 584, DateTimeKind.Utc).AddTicks(9605));

            migrationBuilder.UpdateData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 584, DateTimeKind.Utc).AddTicks(9607));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4137));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4154));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4156));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4157));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4158));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4160));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4161));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4163));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4164));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 10,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4166));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 11,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4167));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 12,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4168));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 13,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4169));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 14,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4170));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 15,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4171));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 16,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4173));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 17,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4174));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 18,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4185));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 19,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4186));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 20,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4188));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 21,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4189));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 22,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4190));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 23,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4191));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 24,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4192));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 25,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4193));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 26,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4194));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 27,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4195));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 28,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4196));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 29,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4197));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 30,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4204));

            migrationBuilder.UpdateData(
                table: "Perfis",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 584, DateTimeKind.Utc).AddTicks(9740));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3468));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3469));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3470));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3470));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3470));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3471));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3471));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3472));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3472));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 10,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3472));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 11,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3473));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 12,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3473));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 13,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3474));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 14,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3474));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 15,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3474));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 16,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3475));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 17,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3475));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 18,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3476));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 19,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3476));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 20,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3476));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 21,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3477));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 22,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3477));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 23,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3478));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 24,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3478));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 25,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3478));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 26,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3479));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 27,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3479));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 28,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3480));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 29,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3480));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 30,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3481));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4452));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4457));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4461));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4465));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4469));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(4474));

            migrationBuilder.UpdateData(
                table: "UsuarioEmpresas",
                keyColumns: new[] { "EmpresaId", "UsuarioId" },
                keyValues: new object[] { 1, 1 },
                column: "DataVinculo",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3360));

            migrationBuilder.UpdateData(
                table: "UsuarioEmpresas",
                keyColumns: new[] { "EmpresaId", "UsuarioId" },
                keyValues: new object[] { 2, 1 },
                column: "DataVinculo",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3361));

            migrationBuilder.UpdateData(
                table: "UsuarioEmpresas",
                keyColumns: new[] { "EmpresaId", "UsuarioId" },
                keyValues: new object[] { 3, 1 },
                column: "DataVinculo",
                value: new DateTime(2026, 1, 2, 16, 28, 55, 718, DateTimeKind.Utc).AddTicks(3363));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "ContasReceber");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "ContasPagar");

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3557));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3559));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3561));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3563));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3565));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3566));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3568));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3577));

            migrationBuilder.UpdateData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3579));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3706));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3710));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3712));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3715));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3717));

            migrationBuilder.UpdateData(
                table: "ContasBancarias",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3728));

            migrationBuilder.UpdateData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4139));

            migrationBuilder.UpdateData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4142));

            migrationBuilder.UpdateData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4144));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3327));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3339));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3341));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3343));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3344));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3347));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3348));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3349));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3350));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 10,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3353));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 11,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3354));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 12,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3355));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 13,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3356));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 14,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3358));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 15,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3359));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 16,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3360));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 17,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3361));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 18,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3369));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 19,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3370));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 20,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3372));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 21,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3373));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 22,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3374));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 23,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3375));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 24,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3376));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 25,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3377));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 26,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3378));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 27,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3380));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 28,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3381));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 29,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3382));

            migrationBuilder.UpdateData(
                table: "PerfilPermissoes",
                keyColumn: "Id",
                keyValue: 30,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3392));

            migrationBuilder.UpdateData(
                table: "Perfis",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 21, 875, DateTimeKind.Utc).AddTicks(4368));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2483));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2483));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2484));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2484));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2484));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2485));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 7,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2485));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 8,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2486));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 9,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2486));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 10,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2487));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 11,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2487));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 12,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2487));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 13,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2488));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 14,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2488));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 15,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2489));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 16,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2489));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 17,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2489));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 18,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2490));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 19,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2490));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 20,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2491));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 21,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2491));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 22,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2491));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 23,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2492));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 24,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2492));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 25,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2493));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 26,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2493));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 27,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2493));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 28,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2494));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 29,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2494));

            migrationBuilder.UpdateData(
                table: "Permissoes",
                keyColumn: "Id",
                keyValue: 30,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2495));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 1,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3629));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 2,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3633));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 3,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3636));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 4,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3640));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 5,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3643));

            migrationBuilder.UpdateData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 6,
                column: "DataCriacao",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(3647));

            migrationBuilder.UpdateData(
                table: "UsuarioEmpresas",
                keyColumns: new[] { "EmpresaId", "UsuarioId" },
                keyValues: new object[] { 1, 1 },
                column: "DataVinculo",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2393));

            migrationBuilder.UpdateData(
                table: "UsuarioEmpresas",
                keyColumns: new[] { "EmpresaId", "UsuarioId" },
                keyValues: new object[] { 2, 1 },
                column: "DataVinculo",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2394));

            migrationBuilder.UpdateData(
                table: "UsuarioEmpresas",
                keyColumns: new[] { "EmpresaId", "UsuarioId" },
                keyValues: new object[] { 3, 1 },
                column: "DataVinculo",
                value: new DateTime(2025, 12, 30, 0, 51, 22, 8, DateTimeKind.Utc).AddTicks(2396));

        }
    }
}
