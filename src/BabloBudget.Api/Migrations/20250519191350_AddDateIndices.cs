using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabloBudget.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDateIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MoneyFlows_LastCheckedUtc_StartingDateUtc",
                table: "MoneyFlows",
                columns: new[] { "LastCheckedUtc", "StartingDateUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MoneyFlows_LastCheckedUtc_StartingDateUtc",
                table: "MoneyFlows");
        }
    }
}
