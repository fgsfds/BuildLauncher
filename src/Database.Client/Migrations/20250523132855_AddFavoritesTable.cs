using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Client.Migrations;

/// <inheritdoc />
public sealed partial class AddFavoritesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "favorite_addons",
            schema: "main",
            columns: table => new
            {
                addon_id = table.Column<string>(type: "TEXT", nullable: false),
                addon_version = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_favorite_addons", x => new { x.addon_id, x.addon_version });
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "favorite_addons",
            schema: "main");
    }
}
