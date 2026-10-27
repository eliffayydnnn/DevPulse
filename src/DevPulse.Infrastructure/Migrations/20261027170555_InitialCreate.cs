using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepositorySnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AnalyzedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Forks = table.Column<int>(type: "int", nullable: false),
                    OpenIssues = table.Column<int>(type: "int", nullable: false),
                    PrimaryLanguage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SummaryMetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositorySnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedRepositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PrimaryLanguage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StarCountAtSave = table.Column<int>(type: "int", nullable: false),
                    ForkCountAtSave = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SavedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedRepositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedRepositories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepositorySnapshots_AnalyzedAtUtc",
                table: "RepositorySnapshots",
                column: "AnalyzedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RepositorySnapshots_Owner_Name",
                table: "RepositorySnapshots",
                columns: new[] { "Owner", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedRepositories_Owner_Name",
                table: "SavedRepositories",
                columns: new[] { "Owner", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedRepositories_UserId_Owner_Name",
                table: "SavedRepositories",
                columns: new[] { "UserId", "Owner", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepositorySnapshots");

            migrationBuilder.DropTable(
                name: "SavedRepositories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
