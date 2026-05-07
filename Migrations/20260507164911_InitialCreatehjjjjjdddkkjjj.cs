using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarcodeApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatehjjjjjdddkkjjj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PolicyNumber",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PolicyNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");
        }
    }
}
