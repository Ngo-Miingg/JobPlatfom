using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewNotesToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InterviewNotes",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterviewNotes",
                table: "Applications");
        }
    }
}
