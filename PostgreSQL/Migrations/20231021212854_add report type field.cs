using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class addreporttypefield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_events_reports_report_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_events_report_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "caption",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "report_id",
                table: "events");

            migrationBuilder.AddColumn<int>(
                name: "report_type",
                table: "reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "report_type",
                table: "reports");

            migrationBuilder.AddColumn<string>(
                name: "caption",
                table: "reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "report_id",
                table: "events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_report_id",
                table: "events",
                column: "report_id");

            migrationBuilder.AddForeignKey(
                name: "fk_events_reports_report_id",
                table: "events",
                column: "report_id",
                principalTable: "reports",
                principalColumn: "id");
        }
    }
}
