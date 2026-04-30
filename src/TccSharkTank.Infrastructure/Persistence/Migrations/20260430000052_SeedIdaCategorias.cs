using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdaCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ida_categoria",
                columns: new[] { "ida_categoria_id", "ida_categoria_nome" },
                values: new object[,]
                {
                    { 1, "tecnologia" },
                    { 2, "Agro" },
                    { 3, "inovacao" },
                    { 4, "infraestrutura" },
                    { 5, "moda" },
                    { 6, "automobilismo" },
                    { 7, "sustentabilidade" },
                    { 8, "Comodidade" },
                    { 9, "lazer" },
                    { 10, "uso diario" },
                    { 11, "Moradia" },
                    { 12, "Energia" },
                    { 13, "maritimo" },
                    { 14, "aeronáutico" },
                    { 15, "outros" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ida_categoria",
                keyColumn: "ida_categoria_id",
                keyValue: 15);
        }
    }
}
