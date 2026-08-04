using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategoriesAndSearchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "Products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Physical");

            migrationBuilder.AddColumn<Guid>(
                name: "SellerId",
                table: "Products",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "IsActive", "Name", "ParentCategoryId", "Slug" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), true, "Electronics", null, "electronics" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), true, "Computers", new Guid("10000000-0000-0000-0000-000000000001"), "computers" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), true, "Phones", new Guid("10000000-0000-0000-0000-000000000001"), "phones" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), true, "Home & Living", null, "home-living" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), true, "Fashion", null, "fashion" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), true, "Digital Products", null, "digital-products" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), true, "E-books", new Guid("10000000-0000-0000-0000-000000000006"), "ebooks" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), true, "Templates", new Guid("10000000-0000-0000-0000-000000000006"), "templates" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), true, "Software", new Guid("10000000-0000-0000-0000-000000000006"), "software" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), true, "Courses", new Guid("10000000-0000-0000-0000-000000000006"), "courses" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductType",
                table: "Products",
                column: "ProductType");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Rating",
                table: "Products",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerId",
                table: "Products",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryId",
                table: "ProductCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Slug",
                table: "ProductCategories",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductType",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Rating",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Products");
        }
    }
}
