using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoryAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CurrentAuthorId",
                table: "Story",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Story_CurrentAuthorId",
                table: "Story",
                column: "CurrentAuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Story_AppUser_CurrentAuthorId",
                table: "Story",
                column: "CurrentAuthorId",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Story_AppUser_CurrentAuthorId",
                table: "Story");

            migrationBuilder.DropIndex(
                name: "IX_Story_CurrentAuthorId",
                table: "Story");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentAuthorId",
                table: "Story",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
