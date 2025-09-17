using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNameIndexAndMinorChangesOnStory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "AppUser",
                newName: "NickName");

            migrationBuilder.AlterColumn<int>(
                name: "MaximumAuthors",
                table: "Story",
                type: "int",
                nullable: false,
                defaultValue: 8,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 6);

            migrationBuilder.CreateIndex(
                name: "IX_AppUser_UserName",
                table: "AppUser",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppUser_UserName",
                table: "AppUser");

            migrationBuilder.RenameColumn(
                name: "NickName",
                table: "AppUser",
                newName: "Nickname");

            migrationBuilder.AlterColumn<int>(
                name: "MaximumAuthors",
                table: "Story",
                type: "int",
                nullable: false,
                defaultValue: 6,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 8);
        }
    }
}
