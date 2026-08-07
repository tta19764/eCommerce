using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewsReviewerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewerName",
                table: "ProductReviews",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewerName",
                table: "ProductReviews");
        }
    }
}
