using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePrpAceiteStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "prp_aceite",
                columns: new[] { "prp_aceite_id", "prp_aceite_nome" },
                values: new object[] { 4, "contraproposta" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "prp_aceite",
                keyColumn: "prp_aceite_id",
                keyValue: 4);
        }
    }
}
