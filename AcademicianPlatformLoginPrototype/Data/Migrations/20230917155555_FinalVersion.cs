using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicianPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstNameId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastNameId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoPathId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleId",
                table: "Comments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_DepartmentId",
                table: "Comments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_FirstNameId",
                table: "Comments",
                column: "FirstNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_LastNameId",
                table: "Comments",
                column: "LastNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ProfilePhotoPathId",
                table: "Comments",
                column: "ProfilePhotoPathId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TitleId",
                table: "Comments",
                column: "TitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_DepartmentId",
                table: "Comments",
                column: "DepartmentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_FirstNameId",
                table: "Comments",
                column: "FirstNameId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_LastNameId",
                table: "Comments",
                column: "LastNameId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_ProfilePhotoPathId",
                table: "Comments",
                column: "ProfilePhotoPathId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_TitleId",
                table: "Comments",
                column: "TitleId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_DepartmentId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_FirstNameId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_LastNameId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_ProfilePhotoPathId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_TitleId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_DepartmentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_FirstNameId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_LastNameId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ProfilePhotoPathId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_TitleId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FirstNameId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "LastNameId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoPathId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "TitleId",
                table: "Comments");
        }
    }
}
