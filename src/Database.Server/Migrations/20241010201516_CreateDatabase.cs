using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Server.Migrations;

/// <inheritdoc />
public sealed partial class CreateDatabase : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.EnsureSchema(
            name: "main");

        _ = migrationBuilder.CreateTable(
            name: "addon_type",
            schema: "main",
            columns: table => new
            {
                id = table.Column<byte>(type: "smallint", nullable: false),
                type = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_addon_type", x => x.id);
            });

        _ = migrationBuilder.CreateTable(
            name: "games",
            schema: "main",
            columns: table => new
            {
                id = table.Column<byte>(type: "smallint", nullable: false),
                name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_games", x => x.id);
            });

        _ = migrationBuilder.CreateTable(
            name: "addons",
            schema: "main",
            columns: table => new
            {
                id = table.Column<string>(type: "text", nullable: false),
                game_id = table.Column<byte>(type: "smallint", nullable: false),
                addon_type_id = table.Column<byte>(type: "smallint", nullable: false),
                title = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_addons", x => x.id);
                _ = table.ForeignKey(
                    name: "FK_addons_addon_type_addon_type_id",
                    column: x => x.addon_type_id,
                    principalSchema: "main",
                    principalTable: "addon_type",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "FK_addons_games_game_id",
                    column: x => x.game_id,
                    principalSchema: "main",
                    principalTable: "games",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "installs",
            schema: "main",
            columns: table => new
            {
                addon_id = table.Column<string>(type: "text", nullable: false),
                value = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_installs", x => x.addon_id);
                _ = table.ForeignKey(
                    name: "FK_installs_addons_addon_id",
                    column: x => x.addon_id,
                    principalSchema: "main",
                    principalTable: "addons",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "ratings",
            schema: "main",
            columns: table => new
            {
                addon_id = table.Column<string>(type: "text", nullable: false),
                rating_sum = table.Column<decimal>(type: "numeric", nullable: false),
                rating_total = table.Column<decimal>(type: "numeric", nullable: false),
                rating = table.Column<decimal>(type: "numeric", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_ratings", x => x.addon_id);
                _ = table.ForeignKey(
                    name: "FK_ratings_addons_addon_id",
                    column: x => x.addon_id,
                    principalSchema: "main",
                    principalTable: "addons",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "reports",
            schema: "main",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                addon_id = table.Column<string>(type: "text", nullable: false),
                text = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_reports", x => x.id);
                _ = table.ForeignKey(
                    name: "FK_reports_addons_addon_id",
                    column: x => x.addon_id,
                    principalSchema: "main",
                    principalTable: "addons",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "versions",
            schema: "main",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                addon_id = table.Column<string>(type: "text", nullable: false),
                version = table.Column<string>(type: "varchar(10)", nullable: false),
                download_url = table.Column<string>(type: "text", nullable: false),
                file_size = table.Column<long>(type: "bigint", nullable: false),
                author = table.Column<string>(type: "text", nullable: true),
                description = table.Column<string>(type: "text", nullable: true),
                is_disabled = table.Column<bool>(type: "boolean", nullable: false),
                updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_versions", x => x.id);
                _ = table.ForeignKey(
                    name: "FK_versions_addons_addon_id",
                    column: x => x.addon_id,
                    principalSchema: "main",
                    principalTable: "addons",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "dependencies",
            schema: "main",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                version_id = table.Column<int>(type: "integer", nullable: false),
                dependency_id = table.Column<string>(type: "text", nullable: false),
                dependency_version = table.Column<string>(type: "varchar(10)", nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_dependencies", x => x.id);
                _ = table.ForeignKey(
                    name: "FK_dependencies_versions_version_id",
                    column: x => x.version_id,
                    principalSchema: "main",
                    principalTable: "versions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_addons_addon_type_id",
            schema: "main",
            table: "addons",
            column: "addon_type_id");

        _ = migrationBuilder.CreateIndex(
            name: "IX_addons_game_id",
            schema: "main",
            table: "addons",
            column: "game_id");

        _ = migrationBuilder.CreateIndex(
            name: "IX_dependencies_version_id",
            schema: "main",
            table: "dependencies",
            column: "version_id");

        _ = migrationBuilder.CreateIndex(
            name: "IX_games_name",
            schema: "main",
            table: "games",
            column: "name");

        _ = migrationBuilder.CreateIndex(
            name: "IX_reports_addon_id",
            schema: "main",
            table: "reports",
            column: "addon_id");

        _ = migrationBuilder.CreateIndex(
            name: "IX_versions_addon_id",
            schema: "main",
            table: "versions",
            column: "addon_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "dependencies",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "installs",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "ratings",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "reports",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "versions",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "addons",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "addon_type",
            schema: "main");

        _ = migrationBuilder.DropTable(
            name: "games",
            schema: "main");
    }
}
