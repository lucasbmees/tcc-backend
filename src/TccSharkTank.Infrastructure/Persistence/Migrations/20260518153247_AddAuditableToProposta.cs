using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccSharkTank.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditableToProposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "prp_create_date",
                table: "prp_proposta",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "prp_update_date",
                table: "prp_proposta",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "prp_create_date",
                table: "prp_proposta");

            migrationBuilder.DropColumn(
                name: "prp_update_date",
                table: "prp_proposta");
        }
    }
}
