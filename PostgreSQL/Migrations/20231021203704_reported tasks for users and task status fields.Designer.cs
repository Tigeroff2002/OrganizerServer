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
    [Migration("20231021203704_reported tasks for users and task status fields")]
    partial class reportedtasksforusersandtaskstatusfields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "activity_kind", new[] { "personal", "educational", "working" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "event_type", new[] { "personal", "one_to_one", "stand_up", "meeting" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "group_type", new[] { "educational", "job" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "task_type", new[] { "abstract_goal", "meeting_presense", "job_complete" });
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

                    b.Property<int?>("ReportId")
                        .HasColumnType("integer")
                        .HasColumnName("report_id");

                    b.Property<DateTimeOffset>("ScheduledStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("scheduled_start");

                    b.HasKey("Id")
                        .HasName("pk_events");

                    b.HasIndex("ManagerId")
                        .HasDatabaseName("ix_events_manager_id");

                    b.HasIndex("ReportId")
                        .HasDatabaseName("ix_events_report_id");

                    b.ToTable("events", (string)null);
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

                    b.Property<string>("Caption")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("caption");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<DateTimeOffset>("EndMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_moment");

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

                    b.Property<int>("TaskStatus")
                        .HasColumnType("integer")
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

            modelBuilder.Entity("users_events_calendar", b =>
                {
                    b.Property<int>("EventsId")
                        .HasColumnType("integer")
                        .HasColumnName("events_id");

                    b.Property<int>("GuestsId")
                        .HasColumnType("integer")
                        .HasColumnName("guests_id");

                    b.HasKey("EventsId", "GuestsId")
                        .HasName("pk_users_events_calendar");

                    b.HasIndex("GuestsId")
                        .HasDatabaseName("ix_users_events_calendar_guests_id");

                    b.ToTable("users_events_calendar", (string)null);
                });

            modelBuilder.Entity("users_groups_map", b =>
                {
                    b.Property<int>("GroupsId")
                        .HasColumnType("integer")
                        .HasColumnName("groups_id");

                    b.Property<int>("ParticipantsId")
                        .HasColumnType("integer")
                        .HasColumnName("participants_id");

                    b.HasKey("GroupsId", "ParticipantsId")
                        .HasName("pk_users_groups_map");

                    b.HasIndex("ParticipantsId")
                        .HasDatabaseName("ix_users_groups_map_participants_id");

                    b.ToTable("users_groups_map", (string)null);
                });

            modelBuilder.Entity("Models.Event", b =>
                {
                    b.HasOne("Models.User", "Manager")
                        .WithMany("ManagedEvents")
                        .HasForeignKey("ManagerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_events_users_manager_id");

                    b.HasOne("Models.Report", null)
                        .WithMany("Events")
                        .HasForeignKey("ReportId")
                        .HasConstraintName("fk_events_reports_report_id");

                    b.Navigation("Manager");
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

            modelBuilder.Entity("users_events_calendar", b =>
                {
                    b.HasOne("Models.Event", null)
                        .WithMany()
                        .HasForeignKey("EventsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_users_events_calendar_events_events_id");

                    b.HasOne("Models.User", null)
                        .WithMany()
                        .HasForeignKey("GuestsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_users_events_calendar_users_guests_id");
                });

            modelBuilder.Entity("users_groups_map", b =>
                {
                    b.HasOne("Models.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_users_groups_map_groups_groups_id");

                    b.HasOne("Models.User", null)
                        .WithMany()
                        .HasForeignKey("ParticipantsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_users_groups_map_users_participants_id");
                });

            modelBuilder.Entity("Models.Report", b =>
                {
                    b.Navigation("Events");
                });

            modelBuilder.Entity("Models.User", b =>
                {
                    b.Navigation("ManagedEvents");

                    b.Navigation("ReportedTasks");

                    b.Navigation("Reports");

                    b.Navigation("TasksForImplementation");
                });
#pragma warning restore 612, 618
        }
    }
}