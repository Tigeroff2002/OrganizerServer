using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Models.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class create_db : Migration
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
                .Annotation("Npgsql:Enum:task_type", "none,abstract_goal,meeting_presense,job_complete");

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<GroupType>(type: "group_type", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    auth_token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    caption = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    scheduled_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    event_type = table.Column<EventType>(type: "event_type", nullable: false),
                    status = table.Column<EventStatus>(type: "event_status", nullable: false),
                    related_group_id = table.Column<int>(type: "integer", nullable: false),
                    manager_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_events_groups_related_group_id",
                        column: x => x.related_group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_events_users_manager_id",
                        column: x => x.manager_id,
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

            migrationBuilder.CreateTable(
                name: "reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    description = table.Column<string>(type: "text", nullable: false),
                    report_type = table.Column<SnapshotType>(type: "report_type", nullable: false),
                    begin_moment = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_moment = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    caption = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    task_type = table.Column<TaskType>(type: "task_type", nullable: false),
                    task_status = table.Column<TaskCurrentStatus>(type: "task_current_status", nullable: false),
                    reporter_id = table.Column<int>(type: "integer", nullable: false),
                    implementer_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tasks", x => x.id);
                    table.ForeignKey(
                        name: "fk_tasks_users_implementer_id",
                        column: x => x.implementer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tasks_users_user_id",
                        column: x => x.reporter_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "ix_events_manager_id",
                table: "events",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_related_group_id",
                table: "events",
                column: "related_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_users_map_event_id",
                table: "events_users_map",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_users_map_group_id",
                table: "groups_users_map",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_reports_user_id",
                table: "reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_implementer_id",
                table: "tasks",
                column: "implementer_id");

            migrationBuilder.CreateIndex(
                name: "ix_tasks_reporter_id",
                table: "tasks",
                column: "reporter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events_users_map");

            migrationBuilder.DropTable(
                name: "groups_users_map");

            migrationBuilder.DropTable(
                name: "reports");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
