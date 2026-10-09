using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UserAccess.Api.Infrastructure.Persistence;

#nullable disable

namespace UserAccess.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(UserDbContext))]
[Migration("20261009120000_StoreUserImagesAsBytes")]
public partial class StoreUserImagesAsBytes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users_new",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false, collation: "NOCASE"),
                PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                ImageData = table.Column<byte[]>(type: "BLOB", nullable: true),
                ImageContentType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.CheckConstraint("CK_Users_Email_Format", "length(\"Email\") BETWEEN 6 AND 254 AND instr(\"Email\", '@') > 1 AND length(\"Email\") - length(replace(\"Email\", '@', '')) = 1 AND instr(substr(\"Email\", instr(\"Email\", '@') + 1), '.') > 1 AND instr(\"Email\", ' ') = 0 AND instr(\"Email\", '..') = 0 AND \"Email\" = trim(\"Email\")");
            });

        migrationBuilder.Sql("INSERT INTO Users_new (Id, FirstName, LastName, Email, PasswordHash, CreatedAtUtc) SELECT Id, FirstName, LastName, Email, PasswordHash, CreatedAtUtc FROM Users;");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.RenameTable(name: "Users_new", newName: "Users");
        migrationBuilder.CreateIndex(name: "UX_Users_Email", table: "Users", column: "Email", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users_old",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false, collation: "NOCASE"),
                PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                ImagePath = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.CheckConstraint("CK_Users_Email_Format", "length(\"Email\") BETWEEN 6 AND 254 AND instr(\"Email\", '@') > 1 AND length(\"Email\") - length(replace(\"Email\", '@', '')) = 1 AND instr(substr(\"Email\", instr(\"Email\", '@') + 1), '.') > 1 AND instr(\"Email\", ' ') = 0 AND instr(\"Email\", '..') = 0 AND \"Email\" = trim(\"Email\")");
            });

        migrationBuilder.Sql("INSERT INTO Users_old (Id, FirstName, LastName, Email, PasswordHash, CreatedAtUtc) SELECT Id, FirstName, LastName, Email, PasswordHash, CreatedAtUtc FROM Users;");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.RenameTable(name: "Users_old", newName: "Users");
        migrationBuilder.CreateIndex(name: "UX_Users_Email", table: "Users", column: "Email", unique: true);
    }
}
