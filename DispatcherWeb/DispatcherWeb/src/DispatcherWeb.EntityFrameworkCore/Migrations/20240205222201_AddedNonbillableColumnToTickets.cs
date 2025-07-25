﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedNonbillableColumnToTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Nonbillable",
                table: "Ticket",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("Update Ticket set Nonbillable = 1 where IsImported = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nonbillable",
                table: "Ticket");
        }
    }
}
