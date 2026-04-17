using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDebeCambiarPasswordToUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                schema: "identity",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE identity.usuarios
                SET "DebeCambiarPassword" = true,
                    "ModificadoEn" = NOW()
                WHERE "Email" = 'admin@repairmodel.local';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                schema: "identity",
                table: "usuarios");
        }
    }
}