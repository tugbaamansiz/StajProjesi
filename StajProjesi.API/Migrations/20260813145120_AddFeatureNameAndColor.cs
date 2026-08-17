using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajProjesi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureNameAndColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "tbl_polygon",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "tbl_polygon",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "tbl_point",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "tbl_point",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "tbl_line",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "tbl_line",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "tbl_polygon");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "tbl_polygon");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "tbl_point");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "tbl_point");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "tbl_line");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "tbl_line");
        }
    }
}
