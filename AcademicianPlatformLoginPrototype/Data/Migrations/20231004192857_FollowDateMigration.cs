using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicianPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class FollowDateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FollowDate",
                table: "Follows",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowDate",
                table: "Follows");
        }
    }
}
