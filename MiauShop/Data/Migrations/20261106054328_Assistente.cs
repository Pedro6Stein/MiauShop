using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiauShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class Assistente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerguntasFrequentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Pergunta = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Resposta = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PalavrasChave = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerguntasFrequentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerguntasSemResposta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TextoNormalizado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Vezes = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerguntasSemResposta", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerguntasSemResposta_TextoNormalizado",
                table: "PerguntasSemResposta",
                column: "TextoNormalizado",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerguntasFrequentes");

            migrationBuilder.DropTable(
                name: "PerguntasSemResposta");
        }
    }
}
