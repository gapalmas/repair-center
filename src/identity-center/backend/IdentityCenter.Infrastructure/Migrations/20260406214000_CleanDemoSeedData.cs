using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityCenter.Infrastructure.Migrations;

[DbContext(typeof(IdentityDbContext))]
[Migration("20260406214000_CleanDemoSeedData")]
public sealed class CleanDemoSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}