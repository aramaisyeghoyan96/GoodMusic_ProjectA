using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbContext.Migrations.SqlServerDbContext
{
    /// <inheritdoc />
    public partial class miInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "supusr");

            migrationBuilder.CreateTable(
                name: "Artists",
                schema: "supusr",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    BirthDay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Seeded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.ArtistId);
                });

            migrationBuilder.CreateTable(
                name: "MusicGroups",
                schema: "supusr",
                columns: table => new
                {
                    MusicGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    EstablishedYear = table.Column<int>(type: "int", nullable: false),
                    strGenre = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Genre = table.Column<int>(type: "int", nullable: false),
                    Seeded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicGroups", x => x.MusicGroupId);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                schema: "supusr",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    MusicGroupDbMMusicGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseYear = table.Column<int>(type: "int", nullable: false),
                    CopiesSold = table.Column<long>(type: "bigint", nullable: false),
                    Seeded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.AlbumId);
                    table.ForeignKey(
                        name: "FK_Albums_MusicGroups_MusicGroupDbMMusicGroupId",
                        column: x => x.MusicGroupDbMMusicGroupId,
                        principalSchema: "supusr",
                        principalTable: "MusicGroups",
                        principalColumn: "MusicGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistDbMMusicGroupDbM",
                schema: "supusr",
                columns: table => new
                {
                    ArtistsDbMArtistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MusicGroupsDbMMusicGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistDbMMusicGroupDbM", x => new { x.ArtistsDbMArtistId, x.MusicGroupsDbMMusicGroupId });
                    table.ForeignKey(
                        name: "FK_ArtistDbMMusicGroupDbM_Artists_ArtistsDbMArtistId",
                        column: x => x.ArtistsDbMArtistId,
                        principalSchema: "supusr",
                        principalTable: "Artists",
                        principalColumn: "ArtistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistDbMMusicGroupDbM_MusicGroups_MusicGroupsDbMMusicGroupId",
                        column: x => x.MusicGroupsDbMMusicGroupId,
                        principalSchema: "supusr",
                        principalTable: "MusicGroups",
                        principalColumn: "MusicGroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_MusicGroupDbMMusicGroupId",
                schema: "supusr",
                table: "Albums",
                column: "MusicGroupDbMMusicGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistDbMMusicGroupDbM_MusicGroupsDbMMusicGroupId",
                schema: "supusr",
                table: "ArtistDbMMusicGroupDbM",
                column: "MusicGroupsDbMMusicGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Albums",
                schema: "supusr");

            migrationBuilder.DropTable(
                name: "ArtistDbMMusicGroupDbM",
                schema: "supusr");

            migrationBuilder.DropTable(
                name: "Artists",
                schema: "supusr");

            migrationBuilder.DropTable(
                name: "MusicGroups",
                schema: "supusr");
        }
    }
}
