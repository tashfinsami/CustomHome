using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomHome.Migrations
{
    /// <inheritdoc />
    public partial class SeedQueueSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "QueueSettings",
                columns: new[] { "Id", "MaxServing", "MaxWaiting" },
                values: new object[] { 1, 2, 5 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "QueueSettings",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
