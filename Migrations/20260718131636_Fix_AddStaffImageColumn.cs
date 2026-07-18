using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AHUWeb.Migrations
{
    /// <inheritdoc />
    public partial class Fix_AddStaffImageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cot Image da co trong Models/Staff.cs va trong model snapshot tu truoc (baseline),
            // nhung migration goc tao ra no (AddStaffImage) da bi mat file - EF khong tu phat
            // hien duoc drift nay vi snapshot da "biet" cot nay tu dau. Them tay bang AddColumn.
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Staffs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Staffs");
        }
    }
}
