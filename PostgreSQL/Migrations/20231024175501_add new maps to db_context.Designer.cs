﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PostgreSQL;

#nullable disable

namespace PostgreSQL.Migrations
{
    [DbContext(typeof(CalendarDataContext))]
    [Migration("20231024175501_add new maps to db_context")]
    partial class addnewmapstodb_context
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "decision_type", new[] { "none", "default", "apply", "deny" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "event_status", new[] { "none", "not_started", "live", "finished", "cancelled" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "event_type", new[] { "none", "personal", "one_to_one", "stand_up", "meeting" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "group_type", new[] { "none", "educational", "job" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "report_type", new[] { "none", "events_report", "tasks_report" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "task_current_status", new[] { "none", "to_do", "in_progress", "review", "done" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "task_type", new[] { "none", "abstract_goal", "meeting_presense", "job_complete" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Caption")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("caption");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("interval")
                        .HasColumnName("duration");

                    b.Property<EventType>("EventType")
                        .HasColumnType("event_type")
                        .HasColumnName("event_type");

                    b.Property<int>("ManagerId")
                        .HasColumnType("integer")
                        .HasColumnName("manager_id");

                    b.Property<int>("RelatedGroupId")
                        .HasColumnType("integer")
                        .HasColumnName("related_group_id");

                    b.Property<DateTimeOffset>("ScheduledStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scheduled_start");

                    b.Property<EventStatus>("Status")
                        .HasColumnType("event_status")
                        .HasColumnName("status");

                    b.HasKey("Id")
                        .HasName("pk_events");

                    b.HasIndex("ManagerId")
                        .HasDatabaseName("ix_events_manager_id");

                    b.HasIndex("RelatedGroupId")
                        .HasDatabaseName("ix_events_related_group_id");

                    b.ToTable("events", (string)null);
                });

            modelBuilder.Entity("Models.EventsUsersMap", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<int>("EventId")
                        .HasColumnType("integer")
                        .HasColumnName("event_id");

                    b.Property<DecisionType>("DecisionType")
                        .HasColumnType("decision_type")
                        .HasColumnName("decision_type");

                    b.HasKey("UserId", "EventId")
                        .HasName("pk_events_users_map");

                    b.HasIndex("EventId")
                        .HasDatabaseName("ix_events_users_map_event_id");

                    b.ToTable("events_users_map", (string)null);
                });

            modelBuilder.Entity("Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("group_name");

                    b.Property<GroupType>("Type")
                        .HasColumnType("group_type")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_groups");

                    b.ToTable("groups", (string)null);
                });

            modelBuilder.Entity("Models.GroupingUsersMap", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer")
                        .HasColumnName("group_id");

                    b.HasKey("UserId", "GroupId")
                        .HasName("pk_groups_users_map");

                    b.HasIndex("GroupId")
                        .HasDatabaseName("ix_groups_users_map_group_id");

                    b.ToTable("groups_users_map", (string)null);
                });

            modelBuilder.Entity("Models.Report", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("BeginMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("begin_moment");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<DateTimeOffset>("EndMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_moment");

                    b.Property<ReportType>("ReportType")
                        .HasColumnType("report_type")
                        .HasColumnName("report_type");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_reports");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_reports_user_id");

                    b.ToTable("reports", (string)null);
                });

            modelBuilder.Entity("Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AuthToken")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("auth_token");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("phone_number");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_name");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Models.UserTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Caption")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("caption");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<int>("ImplementerId")
                        .HasColumnType("integer")
                        .HasColumnName("implementer_id");

                    b.Property<int>("ReporterId")
                        .HasColumnType("integer")
                        .HasColumnName("reporter_id");

                    b.Property<TaskCurrentStatus>("TaskStatus")
                        .HasColumnType("task_current_status")
                        .HasColumnName("task_status");

                    b.Property<TaskType>("TaskType")
                        .HasColumnType("task_type")
                        .HasColumnName("task_type");

                    b.HasKey("Id")
                        .HasName("pk_tasks");

                    b.HasIndex("ImplementerId")
                        .HasDatabaseName("ix_tasks_implementer_id");

                    b.HasIndex("ReporterId")
                        .HasDatabaseName("ix_tasks_reporter_id");

                    b.ToTable("tasks", (string)null);
                });

            modelBuilder.Entity("Models.Event", b =>
                {
                    b.HasOne("Models.User", "Manager")
                        .WithMany("ManagedEvents")
                        .HasForeignKey("ManagerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_events_users_manager_id");

                    b.HasOne("Models.Group", "RelatedGroup")
                        .WithMany("RelatedEvents")
                        .HasForeignKey("RelatedGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_events_groups_related_group_id");

                    b.Navigation("Manager");

                    b.Navigation("RelatedGroup");
                });

            modelBuilder.Entity("Models.EventsUsersMap", b =>
                {
                    b.HasOne("Models.Event", "Event")
                        .WithMany("GuestsMap")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_events_users_map_events_event_id");

                    b.HasOne("Models.User", "User")
                        .WithMany("EventMaps")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_events_users_map_users_user_id");

                    b.Navigation("Event");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Models.GroupingUsersMap", b =>
                {
                    b.HasOne("Models.Group", "Group")
                        .WithMany("ParticipantsMap")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_groups_users_map_groups_group_id");

                    b.HasOne("Models.User", "User")
                        .WithMany("GroupingMaps")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_groups_users_map_users_user_id");

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Models.Report", b =>
                {
                    b.HasOne("Models.User", "User")
                        .WithMany("Reports")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_reports_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Models.UserTask", b =>
                {
                    b.HasOne("Models.User", "Implementer")
                        .WithMany("TasksForImplementation")
                        .HasForeignKey("ImplementerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_tasks_users_implementer_id");

                    b.HasOne("Models.User", "Reporter")
                        .WithMany("ReportedTasks")
                        .HasForeignKey("ReporterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_tasks_users_user_id");

                    b.Navigation("Implementer");

                    b.Navigation("Reporter");
                });

            modelBuilder.Entity("Models.Event", b =>
                {
                    b.Navigation("GuestsMap");
                });

            modelBuilder.Entity("Models.Group", b =>
                {
                    b.Navigation("ParticipantsMap");

                    b.Navigation("RelatedEvents");
                });

            modelBuilder.Entity("Models.User", b =>
                {
                    b.Navigation("EventMaps");

                    b.Navigation("GroupingMaps");

                    b.Navigation("ManagedEvents");

                    b.Navigation("ReportedTasks");

                    b.Navigation("Reports");

                    b.Navigation("TasksForImplementation");
                });
#pragma warning restore 612, 618
        }
    }
}
