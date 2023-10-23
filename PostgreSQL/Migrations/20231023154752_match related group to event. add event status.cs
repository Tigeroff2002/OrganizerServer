using Microsoft.EntityFrameworkCore.Migrations;
using Models.Enums;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class matchrelatedgrouptoeventaddeventstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:event_status", "not_started,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "educational,job")
                .Annotation("Npgsql:Enum:report_type", "events_report,tasks_report")
                .Annotation("Npgsql:Enum:task_current_status", "to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:activity_kind", "personal,educational,working")
                .OldAnnotation("Npgsql:Enum:event_type", "personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "educational,job")
                .OldAnnotation("Npgsql:Enum:task_type", "abstract_goal,meeting_presense,job_complete");

            migrationBuilder.AlterColumn<TaskCurrentStatus>(
                name: "task_status",
                table: "tasks",
                type: "task_current_status",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<ReportType>(
                name: "report_type",
                table: "reports",
                type: "report_type",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "related_group_id",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<EventStatus>(
                name: "status",
                table: "events",
                type: "event_status",
                nullable: false,
                defaultValue: EventStatus.NotStarted);

            migrationBuilder.CreateIndex(
                name: "ix_events_related_group_id",
                table: "events",
                column: "related_group_id");

            migrationBuilder.AddForeignKey(
                name: "fk_events_groups_related_group_id",
                table: "events",
                column: "related_group_id",
                principalTable: "groups",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_events_groups_related_group_id",
                table: "events");

            migrationBuilder.DropIndex(
                name: "ix_events_related_group_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "related_group_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "status",
                table: "events");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:activity_kind", "personal,educational,working")
                .Annotation("Npgsql:Enum:event_type", "personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "educational,job")
                .Annotation("Npgsql:Enum:task_type", "abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:event_status", "not_started,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "educational,job")
                .OldAnnotation("Npgsql:Enum:report_type", "events_report,tasks_report")
                .OldAnnotation("Npgsql:Enum:task_current_status", "to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "abstract_goal,meeting_presense,job_complete");

            migrationBuilder.AlterColumn<int>(
                name: "task_status",
                table: "tasks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(TaskCurrentStatus),
                oldType: "task_current_status");

            migrationBuilder.AlterColumn<int>(
                name: "report_type",
                table: "reports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(ReportType),
                oldType: "report_type");
        }
    }
}
