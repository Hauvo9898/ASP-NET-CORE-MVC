using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AHUWeb.Migrations
{
    /// <inheritdoc />
    public partial class Lab15_AddProductIsComingSoon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComingSoon",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsComingSoon",
                table: "Products");
        }
    }
}
