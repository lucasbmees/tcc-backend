using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifPrefsDashAndReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "usu_perfil_email_alertas",
                table: "usu_perfil",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "usu_perfil_email_mensagens",
                table: "usu_perfil",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "usu_perfil_email_propostas",
                table: "usu_perfil",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ida_create_date",
                table: "ida_ideia",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ida_update_date",
                table: "ida_ideia",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "gov_denuncia",
                columns: table => new
                {
                    gov_denuncia_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    denunciante_id = table.Column<long>(type: "INTEGER", nullable: false),
                    gov_denuncia_tipo_alvo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    gov_denuncia_alvo_id = table.Column<long>(type: "INTEGER", nullable: false),
                    gov_denuncia_motivo = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    gov_denuncia_status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    gov_denuncia_obs_adm = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    gov_denuncia_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    gov_denuncia_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gov_denuncia", x => x.gov_denuncia_id);
                    table.ForeignKey(
                        name: "FK_gov_denuncia_usu_usuario_denunciante_id",
                        column: x => x.denunciante_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gov_denuncia_denunciante_id",
                table: "gov_denuncia",
                column: "denunciante_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gov_denuncia");

            migrationBuilder.DropColumn(
                name: "usu_perfil_email_alertas",
                table: "usu_perfil");

            migrationBuilder.DropColumn(
                name: "usu_perfil_email_mensagens",
                table: "usu_perfil");

            migrationBuilder.DropColumn(
                name: "usu_perfil_email_propostas",
                table: "usu_perfil");

            migrationBuilder.DropColumn(
                name: "ida_create_date",
                table: "ida_ideia");

            migrationBuilder.DropColumn(
                name: "ida_update_date",
                table: "ida_ideia");
        }
    }
}
