using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedNtfTiposPropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ntf_tipo",
                columns: new[] { "ntf_tipo_id", "ntf_tipo_nome" },
                values: new object[,]
                {
                    { 5, "prp recebida" },
                    { 6, "prp contraproposta" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ntf_tipo",
                keyColumn: "ntf_tipo_id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ntf_tipo",
                keyColumn: "ntf_tipo_id",
                keyValue: 6);
        }
    }
}
