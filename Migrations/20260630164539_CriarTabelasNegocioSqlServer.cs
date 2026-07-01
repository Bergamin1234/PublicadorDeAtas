using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublicadorDeAtas.Migrations
{
    public partial class CriarTabelasNegocioSqlServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Garante o isolamento das tabelas dentro do Schema da SUPEL
            migrationBuilder.EnsureSchema(
                name: "PublicadorARP");

            // 1. CRIAÇÃO DA TABELA DE ATAS DE REGISTRO DE PREÇO
            migrationBuilder.CreateTable(
                name: "AtaRegistroPreco",
                schema: "PublicadorARP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdPncp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CnpjOrgao = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    AnoCompra = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    SequencialCompra = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NumeroAta = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AnoAta = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    DataAssinatura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataInicioVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFimVigencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PossibilidadeAdesao = table.Column<bool>(type: "bit", nullable: false),
                    CodigoUnidade = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioNome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DataCadastroLocal = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtaRegistroPreco", x => x.Id);
                });

            // 2. CRIAÇÃO DA TABELA DE ÓRGÃOS (PARTES ENVOLVIDAS)
            migrationBuilder.CreateTable(
                name: "ParteEnvolvida",
                schema: "PublicadorARP",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoParteEnvolvidaId = table.Column<int>(type: "int", nullable: false),
                    Cnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    CodigoUnidadeCompradora = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AtaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParteEnvolvida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParteEnvolvida_AtaRegistroPreco_AtaId",
                        column: x => x.AtaId,
                        principalSchema: "PublicadorARP",
                        principalTable: "AtaRegistroPreco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Index de busca composta para otimização de consultas ao PNCP
            migrationBuilder.CreateIndex(
                name: "IX_AtaRegistroPreco_FiltroPNCP",
                schema: "PublicadorARP",
                table: "AtaRegistroPreco",
                columns: new[] { "CnpjOrgao", "AnoCompra", "SequencialCompra" });

            // Index do relacionamento estrangeiro
            migrationBuilder.CreateIndex(
                name: "IX_ParteEnvolvida_AtaId",
                schema: "PublicadorARP",
                table: "ParteEnvolvida",
                column: "AtaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParteEnvolvida",
                schema: "PublicadorARP");

            migrationBuilder.DropTable(
                name: "AtaRegistroPreco",
                schema: "PublicadorARP");
        }
    }
}