using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class reportedtasksforusersandtaskstatusfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tasks_users_user_id",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "fk_users_groups_map_groups_groups_temp_id",
                table: "users_groups_map");

            migrationBuilder.AlterColumn<int>(
                name: "implementer_id",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reporter_id",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "task_status",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_tasks_reporter_id",
                table: "tasks",
                column: "reporter_id");

            migrationBuilder.AddForeignKey(
                name: "fk_tasks_users_implementer_id",
                table: "tasks",
                column: "implementer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tasks_users_user_id",
                table: "tasks",
                column: "reporter_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_users_groups_map_groups_groups_id",
                table: "users_groups_map",
                column: "groups_id",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tasks_users_implementer_id",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "fk_tasks_users_user_id",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "fk_users_groups_map_groups_groups_id",
                table: "users_groups_map");

            migrationBuilder.DropIndex(
                name: "ix_tasks_reporter_id",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "reporter_id",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "task_status",
                table: "tasks");

            migrationBuilder.AlterColumn<int>(
                name: "implementer_id",
                table: "tasks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "fk_tasks_users_user_id",
                table: "tasks",
                column: "implementer_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_users_groups_map_groups_groups_temp_id",
                table: "users_groups_map",
                column: "groups_id",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
