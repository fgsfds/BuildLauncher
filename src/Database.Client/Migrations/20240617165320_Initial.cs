using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Client.Migrations;

/// <inheritdoc />
public sealed partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.EnsureSchema(
            name: "main");

        _ = migrationBuilder.CreateTable(
            name: "disabled_addons",
            schema: "main",
            columns: table => new
            {
                addon_id = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_disabled_addons", x => x.addon_id);
            });

        _ = migrationBuilder.CreateTable(
            name: "game_paths",
            schema: "main",
            columns: table => new
            {
                game = table.Column<string>(type: "TEXT", nullable: false),
                path = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_game_paths", x => x.game);
            });

        _ = migrationBuilder.CreateTable(
            name: "playtimes",
            schema: "main",
            columns: table => new
            {
                addon_id = table.Column<string>(type: "TEXT", nullable: false),
                playtime = table.Column<TimeSpan>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_playtimes", x => x.addon_id);
            });

        _ = migrationBuilder.CreateTable(
            name: "scores",
            schema: "main",
            columns: table => new
            {
                addon_id = table.Column<string>(type: "TEXT", nullable: false),
                is_upvoted = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_scores", x => x.addon_id);
            });

        _ = migrationBuilder.CreateTable(
            name: "settings",
            schema: "main",
            columns: table => new
            {
                name = table.Column<string>(type: "TEXT", nullable: false),
                value = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_settings", x => x.name);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "disabled_addons",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "game_paths",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "playtimes",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "scores",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "settings",
            schema: "main");
    }
}
