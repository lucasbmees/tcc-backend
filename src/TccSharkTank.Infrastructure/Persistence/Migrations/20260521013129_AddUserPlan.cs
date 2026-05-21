using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "usu_plano_id",
                table: "usu_usuario",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "usu_plano",
                columns: table => new
                {
                    usu_plano_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    usu_plano_nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usu_plano", x => x.usu_plano_id);
                });

            migrationBuilder.InsertData(
                table: "usu_plano",
                columns: new[] { "usu_plano_id", "usu_plano_nome" },
                values: new object[,]
                {
                    { 1, "basico" },
                    { 2, "pro" },
                    { 3, "elite" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_usu_usuario_usu_plano_id",
                table: "usu_usuario",
                column: "usu_plano_id");

            migrationBuilder.CreateIndex(
                name: "IX_usu_plano_usu_plano_nome",
                table: "usu_plano",
                column: "usu_plano_nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_usu_usuario_usu_plano_usu_plano_id",
                table: "usu_usuario",
                column: "usu_plano_id",
                principalTable: "usu_plano",
                principalColumn: "usu_plano_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usu_usuario_usu_plano_usu_plano_id",
                table: "usu_usuario");

            migrationBuilder.DropTable(
                name: "usu_plano");

            migrationBuilder.DropIndex(
                name: "IX_usu_usuario_usu_plano_id",
                table: "usu_usuario");

            migrationBuilder.DropColumn(
                name: "usu_plano_id",
                table: "usu_usuario");
        }
    }
}
