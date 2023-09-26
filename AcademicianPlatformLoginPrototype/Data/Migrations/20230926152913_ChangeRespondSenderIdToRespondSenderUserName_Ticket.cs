using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicianPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRespondSenderIdToRespondSenderUserName_Ticket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TicketRespondSenderId",
                table: "Tickets",
                newName: "TicketRespondSenderUserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TicketRespondSenderUserName",
                table: "Tickets",
                newName: "TicketRespondSenderId");
        }
    }
}
