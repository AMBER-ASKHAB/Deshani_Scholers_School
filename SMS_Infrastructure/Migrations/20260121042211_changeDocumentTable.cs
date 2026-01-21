using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changeDocumentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "app_BFORM_file",
                table: "applicants");

            migrationBuilder.DropColumn(
                name: "app_picturefilename",
                table: "applicants");

            migrationBuilder.DropColumn(
                name: "app_prevschool_certfile",
                table: "applicants");

            migrationBuilder.DropColumn(
                name: "app_prevschoolleaving_certfile",
                table: "applicants");

            migrationBuilder.AlterColumn<string>(
                name: "sub_description",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "sub_description",
                table: "Subjects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "app_BFORM_file",
                table: "applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "app_picturefilename",
                table: "applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "app_prevschool_certfile",
                table: "applicants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "app_prevschoolleaving_certfile",
                table: "applicants",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
