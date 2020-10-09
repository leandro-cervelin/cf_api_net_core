using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CF.Api.Migrations
{
    public partial class customer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Customer",
                table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy",
                            SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    Email = table.Column<string>(maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(maxLength: 100, nullable: false),
                    Password = table.Column<string>(maxLength: 2000, nullable: false),
                    Surname = table.Column<string>(maxLength: 100, nullable: false),
                    Updated = table.Column<DateTime>(nullable: true)
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