using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListService.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveItemPriceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price",
                table: "items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "items",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
