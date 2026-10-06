using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Operative.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiceRolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Die1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Die2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Sum = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiceRolls", x => x.Id);
                    table.CheckConstraint("CK_DiceRolls_Die1_Range", "\"Die1\" BETWEEN 1 AND 6");
                    table.CheckConstraint("CK_DiceRolls_Die2_Range", "\"Die2\" BETWEEN 1 AND 6");
                    table.CheckConstraint("CK_DiceRolls_Sum", "\"Sum\" = \"Die1\" + \"Die2\"");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiceRolls_UserId_CreatedAtUtc",
                table: "DiceRolls",
                columns: new[] { "UserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DiceRolls_UserId_Sum",
                table: "DiceRolls",
                columns: new[] { "UserId", "Sum" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiceRolls");
        }
    }
}
