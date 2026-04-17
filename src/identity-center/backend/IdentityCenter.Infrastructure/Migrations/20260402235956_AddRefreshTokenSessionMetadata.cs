using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenSessionMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UsuarioId",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                schema: "identity",
                table: "refresh_tokens",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SessionFingerprint",
                schema: "identity",
                table: "refresh_tokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoUsoEn",
                schema: "identity",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                schema: "identity",
                table: "refresh_tokens",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UsuarioId_SessionFingerprint",
                schema: "identity",
                table: "refresh_tokens",
                columns: new[] { "UsuarioId", "SessionFingerprint" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_UsuarioId_SessionFingerprint",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "SessionFingerprint",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "UltimoUsoEn",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                schema: "identity",
                table: "refresh_tokens");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UsuarioId",
                schema: "identity",
                table: "refresh_tokens",
                column: "UsuarioId");
        }
    }
}
