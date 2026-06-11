using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListVersionConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "version",
                table: "lists",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                table: "lists");
        }
    }
}
