using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientCommon.Migrations
{
    /// <inheritdoc />
    public partial class CreateRatingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scores",
                schema: "main");

            migrationBuilder.CreateTable(
                name: "rating",
                schema: "main",
                columns: table => new
                {
                    addon_id = table.Column<string>(type: "TEXT", nullable: false),
                    rating = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rating", x => x.addon_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rating",
                schema: "main");

            migrationBuilder.CreateTable(
                name: "scores",
                schema: "main",
                columns: table => new
                {
                    addon_id = table.Column<string>(type: "TEXT", nullable: false),
                    is_upvoted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scores", x => x.addon_id);
                });
        }
    }
}
