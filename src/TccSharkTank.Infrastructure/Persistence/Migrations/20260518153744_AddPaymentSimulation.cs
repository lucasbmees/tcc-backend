using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSimulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pgt_pagamento",
                columns: table => new
                {
                    pgt_pagamento_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    usu_id = table.Column<long>(type: "INTEGER", nullable: false),
                    pgt_pagamento_valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    pgt_pagamento_descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    pgt_pagamento_metodo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    pgt_pagamento_status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    pgt_pagamento_create_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    pgt_pagamento_update_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pgt_pagamento", x => x.pgt_pagamento_id);
                    table.ForeignKey(
                        name: "FK_pgt_pagamento_usu_usuario_usu_id",
                        column: x => x.usu_id,
                        principalTable: "usu_usuario",
                        principalColumn: "usu_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pgt_pagamento_usu_id",
                table: "pgt_pagamento",
                column: "usu_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pgt_pagamento");
        }
    }
}
