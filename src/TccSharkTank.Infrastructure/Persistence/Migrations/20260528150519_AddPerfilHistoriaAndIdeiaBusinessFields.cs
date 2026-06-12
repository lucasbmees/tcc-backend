using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfilHistoriaAndIdeiaBusinessFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "usu_perfil_historia",
                table: "usu_perfil",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ida_info_custos_mensais",
                table: "ida_info",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ida_info_faturamento",
                table: "ida_info",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ida_info_feedback_clientes",
                table: "ida_info",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ida_info_quantidade_clientes",
                table: "ida_info",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ida_info_tempo_mercado_meses",
                table: "ida_info",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "usu_perfil_historia",
                table: "usu_perfil");

            migrationBuilder.DropColumn(
                name: "ida_info_custos_mensais",
                table: "ida_info");

            migrationBuilder.DropColumn(
                name: "ida_info_faturamento",
                table: "ida_info");

            migrationBuilder.DropColumn(
                name: "ida_info_feedback_clientes",
                table: "ida_info");

            migrationBuilder.DropColumn(
                name: "ida_info_quantidade_clientes",
                table: "ida_info");

            migrationBuilder.DropColumn(
                name: "ida_info_tempo_mercado_meses",
                table: "ida_info");
        }
    }
}
