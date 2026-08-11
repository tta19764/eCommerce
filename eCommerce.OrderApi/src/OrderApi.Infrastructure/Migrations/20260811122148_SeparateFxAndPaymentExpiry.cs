using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeparateFxAndPaymentExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FxExpiresOnUtc",
                table: "Orders",
                newName: "FxQuoteExpiresOnUtc");

            migrationBuilder.RenameColumn(
                name: "FxEffectiveOnUtc",
                table: "Orders",
                newName: "FxRateEffectiveOnUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "FxQuotedOnUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentExpiresOnUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            // Preserve the previous FX expiry as quote provenance. Existing payable orders receive
            // an independent 24-hour payment window based on their original creation time.
            migrationBuilder.Sql(
                """
                UPDATE "Orders"
                SET "FxQuotedOnUtc" = COALESCE("FxQuoteExpiresOnUtc" - INTERVAL '15 minutes', "CreatedAtUtc"),
                    "PaymentExpiresOnUtc" = "CreatedAtUtc" + INTERVAL '24 hours'
                WHERE "GrandTotalMinor" > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FxQuotedOnUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentExpiresOnUtc",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "FxQuoteExpiresOnUtc",
                table: "Orders",
                newName: "FxExpiresOnUtc");

            migrationBuilder.RenameColumn(
                name: "FxRateEffectiveOnUtc",
                table: "Orders",
                newName: "FxEffectiveOnUtc");
        }
    }
}
