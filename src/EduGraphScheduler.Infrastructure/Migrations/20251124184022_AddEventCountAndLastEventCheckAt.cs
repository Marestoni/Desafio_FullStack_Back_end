using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduGraphScheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCountAndLastEventCheckAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEventCheckAt",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastEventCheckAt",
                table: "Users");
        }
    }
}
