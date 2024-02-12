using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Models.Enums;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class groupmanageridandissuestatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .Annotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "none,educational,job")
                .Annotation("Npgsql:Enum:issue_status", "none,reported,in_progress,closed")
                .Annotation("Npgsql:Enum:issue_type", "none,bag_issue,violation_issue")
                .Annotation("Npgsql:Enum:snapshot_type", "none,events_snapshot,tasks_snapshot,issues_snapshot,reports_snapshot")
                .Annotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .Annotation("Npgsql:Enum:user_role", "none,user,admin")
                .OldAnnotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .OldAnnotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "none,educational,job")
                .OldAnnotation("Npgsql:Enum:issue_type", "none,bag_issue,violation_issue")
                .OldAnnotation("Npgsql:Enum:snapshot_type", "none,events_snapshot,tasks_snapshot,issues_snapshot,reports_snapshot")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:user_role", "none,user,admin");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "create_moment",
                table: "snapshots",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<IssueStatus>(
                name: "status",
                table: "issues",
                type: "issue_status",
                nullable: false,
                defaultValue: IssueStatus.None);

            migrationBuilder.AddColumn<int>(
                name: "manager_id",
                table: "groups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_groups_manager_id",
                table: "groups",
                column: "manager_id");

            migrationBuilder.AddForeignKey(
                name: "fk_groups_users_manager_id",
                table: "groups",
                column: "manager_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_groups_users_manager_id",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "ix_groups_manager_id",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "create_moment",
                table: "snapshots");

            migrationBuilder.DropColumn(
                name: "status",
                table: "issues");

            migrationBuilder.DropColumn(
                name: "manager_id",
                table: "groups");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .Annotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "none,educational,job")
                .Annotation("Npgsql:Enum:issue_type", "none,bag_issue,violation_issue")
                .Annotation("Npgsql:Enum:snapshot_type", "none,events_snapshot,tasks_snapshot,issues_snapshot,reports_snapshot")
                .Annotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .Annotation("Npgsql:Enum:user_role", "none,user,admin")
                .OldAnnotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .OldAnnotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "none,educational,job")
                .OldAnnotation("Npgsql:Enum:issue_status", "none,reported,in_progress,closed")
                .OldAnnotation("Npgsql:Enum:issue_type", "none,bag_issue,violation_issue")
                .OldAnnotation("Npgsql:Enum:snapshot_type", "none,events_snapshot,tasks_snapshot,issues_snapshot,reports_snapshot")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:user_role", "none,user,admin");
        }
    }
}
