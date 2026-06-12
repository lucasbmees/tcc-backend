using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestorProfileAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "usu_perfil_invest_interesses",
                table: "usu_perfil",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "usu_perfil_invest_ticket_max",
                table: "usu_perfil",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "usu_perfil_invest_ticket_min",
                table: "usu_perfil",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ida_comentario",
                columns: table => new
                {
                    ida_comentario_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ida_comentario_ideia_id = table.Column<long>(type: "INTEGER", nullable: false),
                    ida_comentario_usuario_id = table.Column<long>(type: "INTEGER", nullable: false),
                    ida_comentario_parent_id = table.Column<long>(type: "INTEGER", nullable: true),
                    ida_comentario_texto = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ida_comentario_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ida_comentario_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ida_comentario", x => x.ida_comentario_id);
                    table.ForeignKey(
                        name: "FK_ida_comentario_ida_comentario_ida_comentario_parent_id",
                        column: x => x.ida_comentario_parent_id,
                        principalTable: "ida_comentario",
                        principalColumn: "ida_comentario_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ida_comentario_ida_ideia_ida_comentario_ideia_id",
                        column: x => x.ida_comentario_ideia_id,
                        principalTable: "ida_ideia",
                        principalColumn: "ida_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ida_comentario_usu_usuario_ida_comentario_usuario_id",
                        column: x => x.ida_comentario_usuario_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ida_comentario_ida_comentario_ideia_id",
                table: "ida_comentario",
                column: "ida_comentario_ideia_id");

            migrationBuilder.CreateIndex(
                name: "IX_ida_comentario_ida_comentario_parent_id",
                table: "ida_comentario",
                column: "ida_comentario_parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_ida_comentario_ida_comentario_usuario_id",
                table: "ida_comentario",
                column: "ida_comentario_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ida_comentario");

            migrationBuilder.DropColumn(
                name: "usu_perfil_invest_interesses",
                table: "usu_perfil");

            migrationBuilder.DropColumn(
                name: "usu_perfil_invest_ticket_max",
                table: "usu_perfil");

            migrationBuilder.DropColumn(
                name: "usu_perfil_invest_ticket_min",
                table: "usu_perfil");
        }
    }
}
