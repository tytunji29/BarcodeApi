using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarcodeApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatehjjjjj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    CompanyRCNumber = table.Column<string>(type: "text", nullable: false),
                    CompanyRegisteredAddress = table.Column<string>(type: "text", nullable: false),
                    CompanyTelephone = table.Column<string>(type: "text", nullable: false),
                    CompanyEmail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ECerpacNumber = table.Column<string>(type: "text", nullable: false),
                    DateOfExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PassportNumber = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    MiddleName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Sex = table.Column<string>(type: "text", nullable: false),
                    Nationality = table.Column<string>(type: "text", nullable: false),
                    ValidityStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidityEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResidentClass = table.Column<string>(type: "text", nullable: false),
                    Designation = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
