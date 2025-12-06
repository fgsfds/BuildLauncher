using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Client.Migrations;

/// <inheritdoc />
public sealed partial class AddOptionsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "addon_options",
            columns: table => new
            {
                addon_id = table.Column<string>(type: "TEXT", nullable: false),
                enabled_options = table.Column<string>(type: "TEXT", nullable: false)
            },
            schema: "main",
            constraints: table =>
            _ = table.PrimaryKey("PK_addon_options", x => x.addon_id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "addon_options",
            schema: "main");
    }
}
