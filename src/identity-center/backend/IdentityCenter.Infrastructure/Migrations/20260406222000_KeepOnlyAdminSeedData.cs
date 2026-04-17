using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityCenter.Infrastructure.Migrations;

[DbContext(typeof(IdentityDbContext))]
[Migration("20260406222000_KeepOnlyAdminSeedData")]
public sealed class KeepOnlyAdminSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}