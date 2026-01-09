using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class challanheads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "sub_description",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "acf_amountHeads",
                table: "admissionChallan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ChallanHeads",
                columns: table => new
                {
                    chh_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    chh_schid = table.Column<int>(type: "int", nullable: false),
                    chh_Challanid = table.Column<int>(type: "int", nullable: false),
                    chh_Appid = table.Column<int>(type: "int", nullable: false),
                    chh_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    chh_amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallanHeads", x => x.chh_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChallanHeads");

            migrationBuilder.DropColumn(
                name: "acf_amountHeads",
                table: "admissionChallan");

            migrationBuilder.AlterColumn<long>(
                name: "sub_description",
                table: "Subjects",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
