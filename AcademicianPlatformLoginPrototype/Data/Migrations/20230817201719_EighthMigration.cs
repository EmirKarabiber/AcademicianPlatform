using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicianPlatform.Data.Migrations
{
    public partial class EighthMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_AspNetUsers_AnnouncementSenderId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_AnnouncementSenderId",
                table: "Announcements");

            migrationBuilder.RenameColumn(
                name: "AnnouncementSenderId",
                table: "Announcements",
                newName: "AnnouncementSenderID");

            migrationBuilder.AlterColumn<int>(
                name: "AnnouncementSenderID",
                table: "Announcements",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AnnouncementSenderID",
                table: "Announcements",
                newName: "AnnouncementSenderId");

            migrationBuilder.AlterColumn<string>(
                name: "AnnouncementSenderId",
                table: "Announcements",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_AnnouncementSenderId",
                table: "Announcements",
                column: "AnnouncementSenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_AspNetUsers_AnnouncementSenderId",
                table: "Announcements",
                column: "AnnouncementSenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
