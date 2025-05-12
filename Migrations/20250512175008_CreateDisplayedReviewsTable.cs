using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poplensFeedApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateDisplayedReviewsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "DisplayedReviews",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisplayedReviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DisplayedReviews_ProfileId_ReviewId",
                schema: "public",
                table: "DisplayedReviews",
                columns: new[] { "ProfileId", "ReviewId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisplayedReviews",
                schema: "public");
        }
    }
}
