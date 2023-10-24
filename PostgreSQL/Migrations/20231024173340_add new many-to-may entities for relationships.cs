using Microsoft.EntityFrameworkCore.Migrations;
using Models.Enums;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class addnewmanytomayentitiesforrelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users_events_calendar");

            migrationBuilder.DropTable(
                name: "users_groups_map");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .Annotation("Npgsql:Enum:event_status", "none,not_started,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "none,educational,job")
                .Annotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .Annotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:event_status", "not_started,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "educational,job")
                .OldAnnotation("Npgsql:Enum:report_type", "events_report,tasks_report")
                .OldAnnotation("Npgsql:Enum:task_current_status", "to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "abstract_goal,meeting_presense,job_complete");

            migrationBuilder.CreateTable(
                name: "events_users_map",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    decision_type = table.Column<DecisionType>(type: "decision_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events_users_map", x => new { x.user_id, x.event_id });
                    table.ForeignKey(
                        name: "fk_events_users_map_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_events_users_map_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups_users_map",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups_users_map", x => new { x.user_id, x.group_id });
                    table.ForeignKey(
                        name: "fk_groups_users_map_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_groups_users_map_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_users_map_event_id",
                table: "events_users_map",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_users_map_group_id",
                table: "groups_users_map",
                column: "group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events_users_map");

            migrationBuilder.DropTable(
                name: "groups_users_map");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:event_status", "not_started,live,finished,cancelled")
                .Annotation("Npgsql:Enum:event_type", "personal,one_to_one,stand_up,meeting")
                .Annotation("Npgsql:Enum:group_type", "educational,job")
                .Annotation("Npgsql:Enum:report_type", "events_report,tasks_report")
                .Annotation("Npgsql:Enum:task_current_status", "to_do,in_progress,review,done")
                .Annotation("Npgsql:Enum:task_type", "abstract_goal,meeting_presense,job_complete")
                .OldAnnotation("Npgsql:Enum:decision_type", "none,default,apply,deny")
                .OldAnnotation("Npgsql:Enum:event_status", "none,not_started,live,finished,cancelled")
                .OldAnnotation("Npgsql:Enum:event_type", "none,personal,one_to_one,stand_up,meeting")
                .OldAnnotation("Npgsql:Enum:group_type", "none,educational,job")
                .OldAnnotation("Npgsql:Enum:report_type", "none,events_report,tasks_report")
                .OldAnnotation("Npgsql:Enum:task_current_status", "none,to_do,in_progress,review,done")
                .OldAnnotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete");

            migrationBuilder.CreateTable(
                name: "users_events_calendar",
                columns: table => new
                {
                    events_id = table.Column<int>(type: "integer", nullable: false),
                    guests_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users_events_calendar", x => new { x.events_id, x.guests_id });
                    table.ForeignKey(
                        name: "fk_users_events_calendar_events_events_id",
                        column: x => x.events_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_users_events_calendar_users_guests_id",
                        column: x => x.guests_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_groups_map",
                columns: table => new
                {
                    groups_id = table.Column<int>(type: "integer", nullable: false),
                    participants_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users_groups_map", x => new { x.groups_id, x.participants_id });
                    table.ForeignKey(
                        name: "fk_users_groups_map_groups_groups_id",
                        column: x => x.groups_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_users_groups_map_users_participants_id",
                        column: x => x.participants_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_users_events_calendar_guests_id",
                table: "users_events_calendar",
                column: "guests_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_groups_map_participants_id",
                table: "users_groups_map",
                column: "participants_id");
        }
    }
}
