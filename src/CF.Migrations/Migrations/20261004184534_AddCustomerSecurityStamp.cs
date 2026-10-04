using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CF.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerSecurityStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Customer",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Give existing customers a real stamp; an empty one would make every token they get be rejected.
            migrationBuilder.Sql(
                "UPDATE [Customer] SET [SecurityStamp] = LOWER(REPLACE(CONVERT(nvarchar(36), NEWID()), '-', ''))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Customer");
        }
    }
}
