using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ida_categoria",
                columns: table => new
                {
                    ida_categoria_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ida_categoria_nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ida_categoria", x => x.ida_categoria_id);
                });

            migrationBuilder.CreateTable(
                name: "ida_status",
                columns: table => new
                {
                    ida_status_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ida_status_nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ida_status", x => x.ida_status_id);
                });

            migrationBuilder.CreateTable(
                name: "ntf_tipo",
                columns: table => new
                {
                    ntf_tipo_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ntf_tipo_nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ntf_tipo", x => x.ntf_tipo_id);
                });

            migrationBuilder.CreateTable(
                name: "prp_aceite",
                columns: table => new
                {
                    prp_aceite_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    prp_aceite_nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prp_aceite", x => x.prp_aceite_id);
                });

            migrationBuilder.CreateTable(
                name: "trn_tipo",
                columns: table => new
                {
                    trn_tipo_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    trn_tipo_nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trn_tipo", x => x.trn_tipo_id);
                });

            migrationBuilder.CreateTable(
                name: "usu_cargo",
                columns: table => new
                {
                    usu_cargo_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    usu_cargo_nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usu_cargo", x => x.usu_cargo_id);
                });

            migrationBuilder.CreateTable(
                name: "usu_usuario",
                columns: table => new
                {
                    usu_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    usu_cpf = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    usu_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    usu_telefone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    usu_senha = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    usu_cargo_id = table.Column<int>(type: "INTEGER", nullable: false),
                    usu_status = table.Column<bool>(type: "INTEGER", nullable: false),
                    usu_ultimo_login = table.Column<DateTime>(type: "TEXT", nullable: true),
                    usu_nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    usu_sobrenome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usu_usuario", x => x.usu_id);
                    table.ForeignKey(
                        name: "FK_usu_usuario_usu_cargo_usu_cargo_id",
                        column: x => x.usu_cargo_id,
                        principalTable: "usu_cargo",
                        principalColumn: "usu_cargo_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ida_ideia",
                columns: table => new
                {
                    ida_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ida_usuario_id = table.Column<long>(type: "INTEGER", nullable: false),
                    ida_status_id = table.Column<int>(type: "INTEGER", nullable: false),
                    ida_motivo_status = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ida_categoria_id = table.Column<int>(type: "INTEGER", nullable: false),
                    ida_nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ida_ideia", x => x.ida_id);
                    table.ForeignKey(
                        name: "FK_ida_ideia_ida_categoria_ida_categoria_id",
                        column: x => x.ida_categoria_id,
                        principalTable: "ida_categoria",
                        principalColumn: "ida_categoria_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ida_ideia_ida_status_ida_status_id",
                        column: x => x.ida_status_id,
                        principalTable: "ida_status",
                        principalColumn: "ida_status_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ida_ideia_usu_usuario_ida_usuario_id",
                        column: x => x.ida_usuario_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ntf_notificacao",
                columns: table => new
                {
                    ntf_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ntf_usuario_id = table.Column<long>(type: "INTEGER", nullable: false),
                    ntf_tipo_id = table.Column<int>(type: "INTEGER", nullable: false),
                    ntf_mensagem = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    ntf_lida = table.Column<bool>(type: "INTEGER", nullable: false),
                    ntf_create_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ntf_notificacao", x => x.ntf_id);
                    table.ForeignKey(
                        name: "FK_ntf_notificacao_ntf_tipo_ntf_tipo_id",
                        column: x => x.ntf_tipo_id,
                        principalTable: "ntf_tipo",
                        principalColumn: "ntf_tipo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ntf_notificacao_usu_usuario_ntf_usuario_id",
                        column: x => x.ntf_usuario_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usu_perfil",
                columns: table => new
                {
                    usu_perfil_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    usu_usuario_id = table.Column<long>(type: "INTEGER", nullable: false),
                    usu_perfil_descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    usu_perfil_cep = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    usu_perfil_data_nasc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    usu_perfil_link_redes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    usu_perfil_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    usu_perfil_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usu_perfil", x => x.usu_perfil_id);
                    table.ForeignKey(
                        name: "FK_usu_perfil_usu_usuario_usu_usuario_id",
                        column: x => x.usu_usuario_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ida_documento",
                columns: table => new
                {
                    ida_documento_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ida_ideia_id = table.Column<long>(type: "INTEGER", nullable: false),
                    ida_documento_arquivo = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ida_documento", x => x.ida_documento_id);
                    table.CheckConstraint("ck_ida_documento_pdf", "ida_documento_arquivo LIKE '%.pdf'");
                    table.ForeignKey(
                        name: "FK_ida_documento_ida_ideia_ida_ideia_id",
                        column: x => x.ida_ideia_id,
                        principalTable: "ida_ideia",
                        principalColumn: "ida_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ida_info",
                columns: table => new
                {
                    ida_info_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ida_ideia_id = table.Column<long>(type: "INTEGER", nullable: false),
                    ida_info_cnpj = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ida_info_descricao = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ida_info_link_video = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ida_info_imagem = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ida_info_fatia = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    ida_info_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ida_info_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ida_info", x => x.ida_info_id);
                    table.ForeignKey(
                        name: "FK_ida_info_ida_ideia_ida_ideia_id",
                        column: x => x.ida_ideia_id,
                        principalTable: "ida_ideia",
                        principalColumn: "ida_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prp_proposta",
                columns: table => new
                {
                    prp_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    prp_ideia_id = table.Column<long>(type: "INTEGER", nullable: false),
                    prp_usuario_id = table.Column<long>(type: "INTEGER", nullable: false),
                    prp_status = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prp_proposta", x => x.prp_id);
                    table.ForeignKey(
                        name: "FK_prp_proposta_ida_ideia_prp_ideia_id",
                        column: x => x.prp_ideia_id,
                        principalTable: "ida_ideia",
                        principalColumn: "ida_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prp_proposta_usu_usuario_prp_usuario_id",
                        column: x => x.prp_usuario_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prp_info",
                columns: table => new
                {
                    prp_info_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    prp_proposta_id = table.Column<long>(type: "INTEGER", nullable: false),
                    prp_info_mensagem = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    prp_info_valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    prp_info_fatia_pret = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    prp_aceite_id = table.Column<int>(type: "INTEGER", nullable: false),
                    prp_info_retorno = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    prp_info_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    prp_info_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prp_info", x => x.prp_info_id);
                    table.ForeignKey(
                        name: "FK_prp_info_prp_aceite_prp_aceite_id",
                        column: x => x.prp_aceite_id,
                        principalTable: "prp_aceite",
                        principalColumn: "prp_aceite_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prp_info_prp_proposta_prp_proposta_id",
                        column: x => x.prp_proposta_id,
                        principalTable: "prp_proposta",
                        principalColumn: "prp_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trn_log",
                columns: table => new
                {
                    trn_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    trn_tipo_id = table.Column<int>(type: "INTEGER", nullable: false),
                    trn_usuario_id = table.Column<long>(type: "INTEGER", nullable: true),
                    trn_ideia_id = table.Column<long>(type: "INTEGER", nullable: true),
                    trn_proposta_id = table.Column<long>(type: "INTEGER", nullable: true),
                    trn_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    trn_descricao = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trn_log", x => x.trn_id);
                    table.ForeignKey(
                        name: "FK_trn_log_ida_ideia_trn_ideia_id",
                        column: x => x.trn_ideia_id,
                        principalTable: "ida_ideia",
                        principalColumn: "ida_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_trn_log_prp_proposta_trn_proposta_id",
                        column: x => x.trn_proposta_id,
                        principalTable: "prp_proposta",
                        principalColumn: "prp_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_trn_log_trn_tipo_trn_tipo_id",
                        column: x => x.trn_tipo_id,
                        principalTable: "trn_tipo",
                        principalColumn: "trn_tipo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trn_log_usu_usuario_trn_usuario_id",
                        column: x => x.trn_usuario_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "ida_status",
                columns: new[] { "ida_status_id", "ida_status_nome" },
                values: new object[,]
                {
                    { 1, "pendente" },
                    { 2, "aprovada" },
                    { 3, "reprovada" }
                });

            migrationBuilder.InsertData(
                table: "ntf_tipo",
                columns: new[] { "ntf_tipo_id", "ntf_tipo_nome" },
                values: new object[,]
                {
                    { 1, "prp aceita" },
                    { 2, "prp recusada" },
                    { 3, "alerta" },
                    { 4, "n" }
                });

            migrationBuilder.InsertData(
                table: "prp_aceite",
                columns: new[] { "prp_aceite_id", "prp_aceite_nome" },
                values: new object[,]
                {
                    { 1, "aceita" },
                    { 2, "recusada" },
                    { 3, "pendente" }
                });

            migrationBuilder.InsertData(
                table: "trn_tipo",
                columns: new[] { "trn_tipo_id", "trn_tipo_nome" },
                values: new object[,]
                {
                    { 1, "cadastro" },
                    { 2, "edição" },
                    { 3, "proposta" },
                    { 4, "login" }
                });

            migrationBuilder.InsertData(
                table: "usu_cargo",
                columns: new[] { "usu_cargo_id", "usu_cargo_nome" },
                values: new object[,]
                {
                    { 1, "adm" },
                    { 2, "empreendedor" },
                    { 3, "investidor" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ida_categoria_ida_categoria_nome",
                table: "ida_categoria",
                column: "ida_categoria_nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ida_documento_ida_ideia_id",
                table: "ida_documento",
                column: "ida_ideia_id");

            migrationBuilder.CreateIndex(
                name: "ix_ida_ideia_ida_categoria_id",
                table: "ida_ideia",
                column: "ida_categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_ida_ideia_ida_nome",
                table: "ida_ideia",
                column: "ida_nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ida_ideia_ida_status_id",
                table: "ida_ideia",
                column: "ida_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_ida_ideia_ida_usuario_id",
                table: "ida_ideia",
                column: "ida_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_ida_info_ida_ideia_id",
                table: "ida_info",
                column: "ida_ideia_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ida_info_ida_info_cnpj",
                table: "ida_info",
                column: "ida_info_cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ida_status_ida_status_nome",
                table: "ida_status",
                column: "ida_status_nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ntf_notificacao_ntf_tipo_id",
                table: "ntf_notificacao",
                column: "ntf_tipo_id");

            migrationBuilder.CreateIndex(
                name: "IX_ntf_notificacao_ntf_usuario_id",
                table: "ntf_notificacao",
                column: "ntf_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_ntf_tipo_ntf_tipo_nome",
                table: "ntf_tipo",
                column: "ntf_tipo_nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prp_aceite_prp_aceite_nome",
                table: "prp_aceite",
                column: "prp_aceite_nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prp_info_prp_aceite_id",
                table: "prp_info",
                column: "prp_aceite_id");

            migrationBuilder.CreateIndex(
                name: "IX_prp_info_prp_proposta_id",
                table: "prp_info",
                column: "prp_proposta_id");

            migrationBuilder.CreateIndex(
                name: "IX_prp_proposta_prp_ideia_id",
                table: "prp_proposta",
                column: "prp_ideia_id");

            migrationBuilder.CreateIndex(
                name: "IX_prp_proposta_prp_usuario_id",
                table: "prp_proposta",
                column: "prp_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_log_trn_ideia_id",
                table: "trn_log",
                column: "trn_ideia_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_log_trn_proposta_id",
                table: "trn_log",
                column: "trn_proposta_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_log_trn_tipo_id",
                table: "trn_log",
                column: "trn_tipo_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_log_trn_usuario_id",
                table: "trn_log",
                column: "trn_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_trn_tipo_trn_tipo_nome",
                table: "trn_tipo",
                column: "trn_tipo_nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usu_cargo_usu_cargo_nome",
                table: "usu_cargo",
                column: "usu_cargo_nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usu_perfil_usu_usuario_id",
                table: "usu_perfil",
                column: "usu_usuario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usu_usuario_usu_cargo_id",
                table: "usu_usuario",
                column: "usu_cargo_id");

            migrationBuilder.CreateIndex(
                name: "ix_usu_usuario_usu_cpf",
                table: "usu_usuario",
                column: "usu_cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usu_usuario_usu_email",
                table: "usu_usuario",
                column: "usu_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usu_usuario_usu_telefone",
                table: "usu_usuario",
                column: "usu_telefone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ida_documento");

            migrationBuilder.DropTable(
                name: "ida_info");

            migrationBuilder.DropTable(
                name: "ntf_notificacao");

            migrationBuilder.DropTable(
                name: "prp_info");

            migrationBuilder.DropTable(
                name: "trn_log");

            migrationBuilder.DropTable(
                name: "usu_perfil");

            migrationBuilder.DropTable(
                name: "ntf_tipo");

            migrationBuilder.DropTable(
                name: "prp_aceite");

            migrationBuilder.DropTable(
                name: "prp_proposta");

            migrationBuilder.DropTable(
                name: "trn_tipo");

            migrationBuilder.DropTable(
                name: "ida_ideia");

            migrationBuilder.DropTable(
                name: "ida_categoria");

            migrationBuilder.DropTable(
                name: "ida_status");

            migrationBuilder.DropTable(
                name: "usu_usuario");

            migrationBuilder.DropTable(
                name: "usu_cargo");
        }
    }
}
