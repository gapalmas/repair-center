using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityCenter.Infrastructure.Migrations
{
    public partial class EnsureDebeCambiarPasswordColumnExists : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE identity.usuarios
                ADD COLUMN IF NOT EXISTS \"DebeCambiarPassword\" boolean NOT NULL DEFAULT FALSE;

                UPDATE identity.usuarios
                SET \"DebeCambiarPassword\" = true,
                    \"ModificadoEn\" = NOW()
                WHERE \"Email\" = 'admin@repairmodel.local';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE identity.usuarios
                DROP COLUMN IF EXISTS \"DebeCambiarPassword\";
                """);
        }
    }
}