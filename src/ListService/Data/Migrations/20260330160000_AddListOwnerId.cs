using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddListOwnerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "owner_id",
                table: "lists",
                type: "varchar",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_lists_owner_id",
                table: "lists",
                column: "owner_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lists_owner_id",
                table: "lists");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "lists");
        }
    }
}
