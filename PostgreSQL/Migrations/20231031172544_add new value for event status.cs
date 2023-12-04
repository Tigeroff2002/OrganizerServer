using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class addnewvalueforeventstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .Annotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "none,educational,job")
                .Annotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .Annotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .OldAnnotation("Npgsql:Enum:event_status", "none,not_started,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "none,educational,job")
                .OldAnnotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .Annotation("Npgsql:Enum:event_status", "none,not_started,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "none,educational,job")
                .Annotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .Annotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .OldAnnotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "none,educational,job")
                .OldAnnotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete");
        }
    }
}
