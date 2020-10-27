using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CF.Api.Migrations
{
    public partial class Customer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Customer",
                table => new
                {
                    Id = table.Column<long>("bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Created = table.Column<DateTime>("datetime2", nullable: false),
                    Email = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>("nvarchar(2000)", maxLength: 2000, nullable: false),
                    Surname = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: false),
                    Updated = table.Column<DateTime>("datetime2", nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Customer", x => x.Id); });

            migrationBuilder.CreateIndex(
                "IX_Customer_Email",
                "Customer",
                "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Customer");
        }
    }
}