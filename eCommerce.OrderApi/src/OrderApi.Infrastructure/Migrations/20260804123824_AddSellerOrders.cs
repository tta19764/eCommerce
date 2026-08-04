using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SellerOrderId",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SellerOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConfirmedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShippedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SellerOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SellerId",
                table: "OrderItems",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SellerOrderId",
                table: "OrderItems",
                column: "SellerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerOrders_OrderId",
                table: "SellerOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerOrders_SellerId",
                table: "SellerOrders",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_SellerOrders_Status",
                table: "SellerOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellerOrders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SellerId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_SellerOrderId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SellerOrderId",
                table: "OrderItems");
        }
    }
}
