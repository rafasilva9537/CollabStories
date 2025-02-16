using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStoryPartUpdateDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "StoryPart");

            migrationBuilder.AlterColumn<int>(
                name: "MaximumAuthors",
                table: "Story",
                type: "int",
                nullable: false,
                defaultValue: 6,
                oldClrType: typeof(int),
                oldType: "INT ",
                oldDefaultValue: 6);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Story",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "StoryPart",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<int>(
                name: "MaximumAuthors",
                table: "Story",
                type: "INT ",
                nullable: false,
                defaultValue: 6,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 6);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Story",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
