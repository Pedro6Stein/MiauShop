using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiauShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConcorrenciaDoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Pedidos",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Pedidos");
        }
    }
}
