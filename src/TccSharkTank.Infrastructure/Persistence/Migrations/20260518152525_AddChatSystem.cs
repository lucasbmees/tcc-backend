using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_ida_documento_pdf",
                table: "ida_documento");

            migrationBuilder.CreateTable(
                name: "cht_conversa",
                columns: table => new
                {
                    cht_conversa_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cht_conversa_usuario1_id = table.Column<long>(type: "INTEGER", nullable: false),
                    cht_conversa_usuario2_id = table.Column<long>(type: "INTEGER", nullable: false),
                    cht_conversa_ideia_id = table.Column<long>(type: "INTEGER", nullable: true),
                    cht_conversa_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    cht_conversa_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cht_conversa", x => x.cht_conversa_id);
                    table.ForeignKey(
                        name: "FK_cht_conversa_ida_ideia_cht_conversa_ideia_id",
                        column: x => x.cht_conversa_ideia_id,
                        principalTable: "ida_ideia",
                        principalColumn: "ida_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cht_conversa_usu_usuario_cht_conversa_usuario1_id",
                        column: x => x.cht_conversa_usuario1_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cht_conversa_usu_usuario_cht_conversa_usuario2_id",
                        column: x => x.cht_conversa_usuario2_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cht_mensagem",
                columns: table => new
                {
                    cht_mensagem_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cht_mensagem_conversa_id = table.Column<long>(type: "INTEGER", nullable: false),
                    cht_mensagem_remetente_id = table.Column<long>(type: "INTEGER", nullable: false),
                    cht_mensagem_texto = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    cht_mensagem_lida = table.Column<bool>(type: "INTEGER", nullable: false),
                    cht_mensagem_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    cht_mensagem_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cht_mensagem", x => x.cht_mensagem_id);
                    table.ForeignKey(
                        name: "FK_cht_mensagem_cht_conversa_cht_mensagem_conversa_id",
                        column: x => x.cht_mensagem_conversa_id,
                        principalTable: "cht_conversa",
                        principalColumn: "cht_conversa_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cht_mensagem_usu_usuario_cht_mensagem_remetente_id",
                        column: x => x.cht_mensagem_remetente_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cht_conversa_cht_conversa_ideia_id",
                table: "cht_conversa",
                column: "cht_conversa_ideia_id");

            migrationBuilder.CreateIndex(
                name: "IX_cht_conversa_cht_conversa_usuario1_id",
                table: "cht_conversa",
                column: "cht_conversa_usuario1_id");

            migrationBuilder.CreateIndex(
                name: "IX_cht_conversa_cht_conversa_usuario2_id",
                table: "cht_conversa",
                column: "cht_conversa_usuario2_id");

            migrationBuilder.CreateIndex(
                name: "IX_cht_mensagem_cht_mensagem_conversa_id",
                table: "cht_mensagem",
                column: "cht_mensagem_conversa_id");

            migrationBuilder.CreateIndex(
                name: "IX_cht_mensagem_cht_mensagem_remetente_id",
                table: "cht_mensagem",
                column: "cht_mensagem_remetente_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cht_mensagem");

            migrationBuilder.DropTable(
                name: "cht_conversa");

            migrationBuilder.AddCheckConstraint(
                name: "ck_ida_documento_pdf",
                table: "ida_documento",
                sql: "ida_documento_arquivo LIKE '%.pdf'");
        }
    }
}
