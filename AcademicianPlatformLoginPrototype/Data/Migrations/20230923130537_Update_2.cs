﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AcademicianPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    TicketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TicketContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TicketRespondContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketRespondSenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketSenderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.TicketId);
                });
        }
    }
}
