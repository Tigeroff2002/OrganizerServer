using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Models.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class renamereportstosnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .Annotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "none,educational,job")
                .Annotation("Npgsql:Enum:snapshot_type", "none,events_snapshot,tasks_snapshot,issues_snapshot,reports_snapshot")
                .Annotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .OldAnnotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "none,educational,job")
                .OldAnnotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete");

            migrationBuilder.CreateTable(
                name: "snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "text", nullable: false),
                    snapshot_type = table.Column<SnapshotType>(type: "snapshot_type", nullable: false),
                    begin_moment = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_moment = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_snapshots_user_id",
                table: "snapshots",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "snapshots");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .Annotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "none,educational,job")
                .Annotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .Annotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .OldAnnotation("Npgsql:Enum:event_status", "none,not_started,within_reminder_offset,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "none,educational,job")
                .OldAnnotation("Npgsql:Enum:snapshot_type", "none,events_snapshot,tasks_snapshot,issues_snapshot,reports_snapshot")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete");

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    begin_moment = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    end_moment = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    report_type = table.Column<SnapshotType>(type: "report_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_reports_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reports_user_id",
                table: "reports",
                column: "user_id");
        }
    }
}
