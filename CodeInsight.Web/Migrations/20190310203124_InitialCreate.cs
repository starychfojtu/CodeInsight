using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CodeInsight.Web.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Progress = table.Column<int>(nullable: false),
                    Result = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Owner = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequests",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    RepositoryId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true),
                    Deletions = table.Column<int>(nullable: false),
                    Additions = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    MergedAt = table.Column<DateTimeOffset>(nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(nullable: true),
                    CommentCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PullRequests_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_RepositoryId",
                table: "PullRequests",
                column: "RepositoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobExecutions");

            migrationBuilder.DropTable(
                name: "PullRequests");

            migrationBuilder.DropTable(
                name: "Repositories");
        }
    }
}
