using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstate_ServicesSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdinRquwest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationuserId",
                table: "Userrequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Userrequests_ApplicationuserId",
                table: "Userrequests",
                column: "ApplicationuserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Userrequests_AspNetUsers_ApplicationuserId",
                table: "Userrequests",
                column: "ApplicationuserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Userrequests_AspNetUsers_ApplicationuserId",
                table: "Userrequests");

            migrationBuilder.DropIndex(
                name: "IX_Userrequests_ApplicationuserId",
                table: "Userrequests");

            migrationBuilder.DropColumn(
                name: "ApplicationuserId",
                table: "Userrequests");
        }
    }
}
