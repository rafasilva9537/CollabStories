using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class NewChangesInStoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CurrentAuthorId",
                table: "Story",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AuthorsMembershipChangeDate",
                table: "Story",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "GetUtcDate()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorsMembershipChangeDate",
                table: "Story");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentAuthorId",
                table: "Story",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
