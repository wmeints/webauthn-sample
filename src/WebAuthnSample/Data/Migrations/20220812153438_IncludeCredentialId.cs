using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAuthnSample.Data.Migrations
{
    public partial class IncludeCredentialId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CredentialId",
                table: "PublicKeyCredentials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredentialId",
                table: "PublicKeyCredentials");
        }
    }
}
