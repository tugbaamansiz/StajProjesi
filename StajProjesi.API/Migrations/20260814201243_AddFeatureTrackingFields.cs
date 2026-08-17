using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajProjesi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================
            // POLYGON
            // =====================================================

            migrationBuilder.AddColumn<DateTime>(
                name: "inserted_date",
                table: "tbl_polygon",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "inserted_user_id",
                table: "tbl_polygon",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "tbl_polygon",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tbl_polygon",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_date",
                table: "tbl_polygon",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");


            // =====================================================
            // POINT
            // =====================================================

            migrationBuilder.AddColumn<DateTime>(
                name: "inserted_date",
                table: "tbl_point",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "inserted_user_id",
                table: "tbl_point",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "tbl_point",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tbl_point",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_date",
                table: "tbl_point",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");


            // =====================================================
            // LINE
            // =====================================================

            migrationBuilder.AddColumn<DateTime>(
                name: "inserted_date",
                table: "tbl_line",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "inserted_user_id",
                table: "tbl_line",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "tbl_line",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tbl_line",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_date",
                table: "tbl_line",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =====================================================
            // POLYGON
            // =====================================================

            migrationBuilder.DropColumn(
                name: "inserted_date",
                table: "tbl_polygon");

            migrationBuilder.DropColumn(
                name: "inserted_user_id",
                table: "tbl_polygon");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "tbl_polygon");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tbl_polygon");

            migrationBuilder.DropColumn(
                name: "modified_date",
                table: "tbl_polygon");


            // =====================================================
            // POINT
            // =====================================================

            migrationBuilder.DropColumn(
                name: "inserted_date",
                table: "tbl_point");

            migrationBuilder.DropColumn(
                name: "inserted_user_id",
                table: "tbl_point");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "tbl_point");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tbl_point");

            migrationBuilder.DropColumn(
                name: "modified_date",
                table: "tbl_point");


            // =====================================================
            // LINE
            // =====================================================

            migrationBuilder.DropColumn(
                name: "inserted_date",
                table: "tbl_line");

            migrationBuilder.DropColumn(
                name: "inserted_user_id",
                table: "tbl_line");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "tbl_line");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tbl_line");

            migrationBuilder.DropColumn(
                name: "modified_date",
                table: "tbl_line");
        }
    }
}