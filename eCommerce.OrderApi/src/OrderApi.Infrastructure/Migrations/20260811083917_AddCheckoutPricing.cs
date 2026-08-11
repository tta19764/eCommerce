using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutCurrency",
                table: "Orders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<DateTime>(
                name: "FxEffectiveOnUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FxExpiresOnUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FxQuoteId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FxRateProvider",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GrandTotalMinor",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "OrderItems",
                type: "numeric(28,12)",
                precision: 28,
                scale: 12,
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "OriginalCurrency",
                table: "OrderItems",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalUnitPrice",
                table: "OrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE "OrderItems"
                SET "OriginalCurrency" = "Currency",
                    "OriginalUnitPrice" = "UnitPrice",
                    "ExchangeRate" = 1;

                UPDATE "Orders" AS o
                SET "CheckoutCurrency" = totals."Currency",
                    "GrandTotalMinor" = totals."GrandTotalMinor"
                FROM (
                    SELECT oi."OrderId",
                           MIN(oi."Currency") AS "Currency",
                           CASE WHEN COUNT(DISTINCT oi."Currency") = 1
                                THEN CAST(ROUND(SUM(oi."UnitPrice" * oi."Quantity") * 100) AS bigint)
                                ELSE 0
                           END AS "GrandTotalMinor"
                    FROM "OrderItems" oi
                    GROUP BY oi."OrderId"
                ) totals
                WHERE o."Id" = totals."OrderId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckoutCurrency",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FxEffectiveOnUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FxExpiresOnUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FxQuoteId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FxRateProvider",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GrandTotalMinor",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OriginalCurrency",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "OriginalUnitPrice",
                table: "OrderItems");
        }
    }
}
