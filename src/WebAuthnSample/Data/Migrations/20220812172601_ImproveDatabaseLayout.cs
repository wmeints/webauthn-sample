using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAuthnSample.Data.Migrations
{
    public partial class ImproveDatabaseLayout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicKeyCredentials_AspNetUsers_ApplicationUserId",
                table: "PublicKeyCredentials");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "PublicKeyCredentials",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicKeyCredentials_AspNetUsers_ApplicationUserId",
                table: "PublicKeyCredentials",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicKeyCredentials_AspNetUsers_ApplicationUserId",
                table: "PublicKeyCredentials");

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "PublicKeyCredentials",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicKeyCredentials_AspNetUsers_ApplicationUserId",
                table: "PublicKeyCredentials",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
