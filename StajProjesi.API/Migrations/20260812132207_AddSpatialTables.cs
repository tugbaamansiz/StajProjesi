using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StajProjesi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSpatialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "tbl_line",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Geometry = table.Column<Geometry>(type: "geometry(LineString,4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_line", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_point",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Geometry = table.Column<Geometry>(type: "geometry(Point,4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_point", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_polygon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Geometry = table.Column<Geometry>(type: "geometry(Polygon,4326)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_polygon", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_line");

            migrationBuilder.DropTable(
                name: "tbl_point");

            migrationBuilder.DropTable(
                name: "tbl_polygon");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
