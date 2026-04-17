using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesCatalog : Migration
    {
        private static readonly Guid SuperAdminRoleId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid AdminRoleId = new("22222222-2222-2222-2222-222222222222");
        private static readonly Guid TecnicoRoleId = new("33333333-3333-3333-3333-333333333333");
        private static readonly Guid RecepcionistaRoleId = new("44444444-4444-4444-4444-444444444444");
        private static readonly Guid AlmacenistaRoleId = new("55555555-5555-5555-5555-555555555555");
        private static readonly Guid SoloLecturaRoleId = new("66666666-6666-6666-6666-666666666666");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "roles",
                columns: new[] { "Id", "Nombre", "Descripcion", "Activo", "CreadoEn", "ModificadoEn" },
                values: new object[,]
                {
                    { SuperAdminRoleId, "SuperAdmin", "Acceso total al modulo de identidad.", true, new DateTime(2026, 4, 6, 16, 27, 21, DateTimeKind.Utc), null },
                    { AdminRoleId, "Admin", "Administracion operativa del modulo de identidad.", true, new DateTime(2026, 4, 6, 16, 27, 21, DateTimeKind.Utc), null },
                    { TecnicoRoleId, "Tecnico", "Usuario tecnico que interactua con repair-center.", true, new DateTime(2026, 4, 6, 16, 27, 21, DateTimeKind.Utc), null },
                    { RecepcionistaRoleId, "Recepcionista", "Recepcion y seguimiento inicial de equipos.", true, new DateTime(2026, 4, 6, 16, 27, 21, DateTimeKind.Utc), null },
                    { AlmacenistaRoleId, "Almacenista", "Control de inventario y resguardo.", true, new DateTime(2026, 4, 6, 16, 27, 21, DateTimeKind.Utc), null },
                    { SoloLecturaRoleId, "SoloLectura", "Consulta sin capacidad de modificacion.", true, new DateTime(2026, 4, 6, 16, 27, 21, DateTimeKind.Utc), null }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "RolId",
                schema: "identity",
                table: "usuarios",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql($"""
                UPDATE identity.usuarios
                SET "RolId" = CASE "Rol"
                    WHEN 'SuperAdmin' THEN '{SuperAdminRoleId}'::uuid
                    WHEN 'Admin' THEN '{AdminRoleId}'::uuid
                    WHEN 'Tecnico' THEN '{TecnicoRoleId}'::uuid
                    WHEN 'Recepcionista' THEN '{RecepcionistaRoleId}'::uuid
                    WHEN 'Almacenista' THEN '{AlmacenistaRoleId}'::uuid
                    WHEN 'SoloLectura' THEN '{SoloLecturaRoleId}'::uuid
                    ELSE '{SoloLecturaRoleId}'::uuid
                END;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "RolId",
                schema: "identity",
                table: "usuarios",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Rol",
                schema: "identity",
                table: "usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_RolId",
                schema: "identity",
                table: "usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Nombre",
                schema: "identity",
                table: "roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_RolId",
                schema: "identity",
                table: "usuarios",
                column: "RolId",
                principalSchema: "identity",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rol",
                schema: "identity",
                table: "usuarios",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "SoloLectura");

            migrationBuilder.Sql("""
                UPDATE identity.usuarios u
                SET "Rol" = COALESCE(r."Nombre", 'SoloLectura')
                FROM identity.roles r
                WHERE u."RolId" = r."Id";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_RolId",
                schema: "identity",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_RolId",
                schema: "identity",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "RolId",
                schema: "identity",
                table: "usuarios");
        }
    }
}
