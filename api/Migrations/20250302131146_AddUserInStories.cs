using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInStories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "StoryPart",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Story",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryPart_UserId",
                table: "StoryPart",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Story_UserId",
                table: "Story",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Story_AppUser_UserId",
                table: "Story",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryPart_AppUser_UserId",
                table: "StoryPart",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Story_AppUser_UserId",
                table: "Story");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryPart_AppUser_UserId",
                table: "StoryPart");

            migrationBuilder.DropIndex(
                name: "IX_StoryPart_UserId",
                table: "StoryPart");

            migrationBuilder.DropIndex(
                name: "IX_Story_UserId",
                table: "Story");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StoryPart");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Story");
        }
    }
}
