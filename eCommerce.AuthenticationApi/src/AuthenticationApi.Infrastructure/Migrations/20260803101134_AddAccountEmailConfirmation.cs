using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountEmailConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmedAtUtc",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Accounts"
                SET "EmailConfirmedAtUtc" = "CreatedAtUtc"
                WHERE "EmailConfirmedAtUtc" IS NULL
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmedAtUtc",
                table: "Accounts");
        }
    }
}
