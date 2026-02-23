using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabloBudget.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMoneyFlows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MoneyFlows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sum = table.Column<decimal>(type: "numeric", nullable: false),
                    StartingDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    LastCheckedUtc = table.Column<DateOnly>(type: "date", nullable: true),
                    PeriodDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyFlows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyFlows_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoneyFlows_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoneyFlows_AccountId",
                table: "MoneyFlows",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyFlows_CategoryId",
                table: "MoneyFlows",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoneyFlows");
        }
    }
}
