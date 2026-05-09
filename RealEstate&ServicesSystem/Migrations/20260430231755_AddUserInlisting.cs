using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstate_ServicesSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInlisting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Listings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_ApplicationUserId",
                table: "Listings",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listings_AspNetUsers_ApplicationUserId",
                table: "Listings",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listings_AspNetUsers_ApplicationUserId",
                table: "Listings");

            migrationBuilder.DropIndex(
                name: "IX_Listings_ApplicationUserId",
                table: "Listings");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Listings");
        }
    }
}
