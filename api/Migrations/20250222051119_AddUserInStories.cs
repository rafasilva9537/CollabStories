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
                name: "OwnerUserId",
                table: "StoryPart",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Story",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryPart_OwnerUserId",
                table: "StoryPart",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Story_OwnerUserId",
                table: "Story",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Story_AppUser_OwnerUserId",
                table: "Story",
                column: "OwnerUserId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryPart_AppUser_OwnerUserId",
                table: "StoryPart",
                column: "OwnerUserId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Story_AppUser_OwnerUserId",
                table: "Story");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryPart_AppUser_OwnerUserId",
                table: "StoryPart");

            migrationBuilder.DropIndex(
                name: "IX_StoryPart_OwnerUserId",
                table: "StoryPart");

            migrationBuilder.DropIndex(
                name: "IX_Story_OwnerUserId",
                table: "Story");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "StoryPart");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Story");
        }
    }
}
