using Microsoft.EntityFrameworkCore.Migrations;

namespace Database.Client.Migrations;

/// <inheritdoc />
public partial class AddPortsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "custom_ports",
            schema: "main",
            columns: table => new
            {
                name = table.Column<string>(type: "TEXT", nullable: false),
                path = table.Column<string>(type: "TEXT", nullable: false),
                port = table.Column<byte>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_custom_ports", x => x.name);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "custom_ports",
            schema: "main");
    }
}
