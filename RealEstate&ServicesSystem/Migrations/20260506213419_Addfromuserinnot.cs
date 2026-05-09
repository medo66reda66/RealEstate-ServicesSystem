using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstate_ServicesSystem.Migrations
{
    /// <inheritdoc />
    public partial class Addfromuserinnot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromUserId",
                table: "notifications",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_FromUserId",
                table: "notifications",
                column: "FromUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_AspNetUsers_FromUserId",
                table: "notifications",
                column: "FromUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_AspNetUsers_FromUserId",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_FromUserId",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "FromUserId",
                table: "notifications");
        }
    }
}
