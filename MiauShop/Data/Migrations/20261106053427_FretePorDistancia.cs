using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiauShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class FretePorDistancia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesFrete",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CepOrigem = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    CidadeOrigem = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    UfOrigem = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    ValorBase = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorPorKm = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    ValorMaximo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesFrete", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesFrete");
        }
    }
}
