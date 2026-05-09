using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstate_ServicesSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdINProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationuserId",
                table: "Properties",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ApplicationuserId",
                table: "Properties",
                column: "ApplicationuserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AspNetUsers_ApplicationuserId",
                table: "Properties",
                column: "ApplicationuserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AspNetUsers_ApplicationuserId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_ApplicationuserId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ApplicationuserId",
                table: "Properties");
        }
    }
}
