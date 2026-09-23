using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomHome.Migrations
{
    /// <inheritdoc />
    public partial class MakeTokenNumberUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ServiceTokens_TokenNumber",
                table: "ServiceTokens",
                column: "TokenNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceTokens_TokenNumber",
                table: "ServiceTokens");
        }
    }
}
