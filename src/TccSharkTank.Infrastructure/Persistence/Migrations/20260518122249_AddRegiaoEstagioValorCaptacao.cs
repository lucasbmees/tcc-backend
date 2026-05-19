using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegiaoEstagioValorCaptacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ida_info_valor_captacao",
                table: "ida_info",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ida_estagio_id",
                table: "ida_ideia",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ida_regiao",
                table: "ida_ideia",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ida_estagio",
                columns: table => new
                {
                    ida_estagio_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ida_estagio_nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ida_estagio", x => x.ida_estagio_id);
                });

            migrationBuilder.InsertData(
                table: "ida_estagio",
                columns: new[] { "ida_estagio_id", "ida_estagio_nome" },
                values: new object[,]
                {
                    { 1, "Ideação" },
                    { 2, "MVP" },
                    { 3, "Tração" },
                    { 4, "Scale-up" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_ida_ideia_ida_estagio_id",
                table: "ida_ideia",
                column: "ida_estagio_id");

            migrationBuilder.CreateIndex(
                name: "IX_ida_estagio_ida_estagio_nome",
                table: "ida_estagio",
                column: "ida_estagio_nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ida_ideia_ida_estagio_ida_estagio_id",
                table: "ida_ideia",
                column: "ida_estagio_id",
                principalTable: "ida_estagio",
                principalColumn: "ida_estagio_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ida_ideia_ida_estagio_ida_estagio_id",
                table: "ida_ideia");

            migrationBuilder.DropTable(
                name: "ida_estagio");

            migrationBuilder.DropIndex(
                name: "ix_ida_ideia_ida_estagio_id",
                table: "ida_ideia");

            migrationBuilder.DropColumn(
                name: "ida_info_valor_captacao",
                table: "ida_info");

            migrationBuilder.DropColumn(
                name: "ida_estagio_id",
                table: "ida_ideia");

            migrationBuilder.DropColumn(
                name: "ida_regiao",
                table: "ida_ideia");
        }
    }
}
