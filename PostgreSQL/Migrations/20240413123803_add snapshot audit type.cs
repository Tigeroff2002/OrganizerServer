using Microsoft.EntityFrameworkCore.Migrations;
using Models.Enums;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class addsnapshotaudittype : Migration
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
                .Annotation("Npgsql:Enum:snapshot_audit_type", "personal,group")
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

            migrationBuilder.AddColumn<SnapshotAuditType>(
                name: "snapshot_audit_type",
                table: "snapshots",
                type: "snapshot_audit_type",
                nullable: false,
                defaultValue: SnapshotAuditType.Personal);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "snapshot_audit_type",
                table: "snapshots");

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
                .OldAnnotation("Npgsql:Enum:issue_status", "none,reported,in_progress,closed")
                .OldAnnotation("Npgsql:Enum:issue_type", "none,bag_issue,violation_issue")
                .OldAnnotation("Npgsql:Enum:snapshot_audit_type", "personal,group")
                .OldAnnotation("Npgsql:Enum:snapshot_type", "none,events_snapshot,tasks_snapshot,issues_snapshot,reports_snapshot")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:user_role", "none,user,admin");
        }
    }
}
