using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddItemStoreQuantityPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "items",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "store_id",
                table: "items",
                type: "varchar",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price",
                table: "items");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "items");

            migrationBuilder.DropColumn(
                name: "store_id",
                table: "items");
        }
    }
}
