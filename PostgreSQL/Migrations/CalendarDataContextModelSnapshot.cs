﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PostgreSQL;

#nullable disable

namespace PostgreSQL.Migrations
{
    [DbContext(typeof(CalendarDataContext))]
    partial class CalendarDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "decision_type", new[] { "none", "default", "apply", "deny" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "event_status", new[] { "none", "not_started", "within_reminder_offset", "live", "finished", "cancelled" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "event_type", new[] { "none", "personal", "one_to_one", "stand_up", "meeting" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "group_type", new[] { "none", "educational", "job" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "issue_status", new[] { "none", "reported", "in_progress", "closed" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "issue_type", new[] { "none", "bag_issue", "violation_issue" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "snapshot_audit_type", new[] { "personal", "group" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "snapshot_type", new[] { "none", "events_snapshot", "tasks_snapshot", "issues_snapshot", "reports_snapshot" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "task_current_status", new[] { "none", "to_do", "in_progress", "review", "done" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "task_type", new[] { "none", "abstract_goal", "meeting_presense", "job_complete" });
            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "user_role", new[] { "none", "user", "admin" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Models.Alert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<bool>("IsAlerted")
                        .HasColumnType("boolean")
                        .HasColumnName("is_alerted");

                    b.Property<DateTimeOffset>("Moment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("moment");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_alerts");

                    b.ToTable("alerts", (string)null);
                });

            modelBuilder.Entity("Models.DirectChat", b =>
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

                    b.Property<DateTimeOffset>("CreateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("create_time");

                    b.Property<int>("User1Id")
                        .HasColumnType("integer")
                        .HasColumnName("user1id");

                    b.Property<int>("User2Id")
                        .HasColumnType("integer")
                        .HasColumnName("user2id");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_direct_chats");

                    b.HasIndex("User1Id")
                        .HasDatabaseName("ix_direct_chats_user1id");

                    b.HasIndex("User2Id")
                        .HasDatabaseName("ix_direct_chats_user2id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_direct_chats_user_id");

                    b.ToTable("direct_chats", (string)null);
                });

            modelBuilder.Entity("Models.DirectMessage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ChatId")
                        .HasColumnType("integer")
                        .HasColumnName("chat_id");

                    b.Property<DateTimeOffset>("SendTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("send_time");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<bool>("isEdited")
                        .HasColumnType("boolean")
                        .HasColumnName("is_edited");

                    b.HasKey("Id")
                        .HasName("pk_messages");

                    b.HasIndex("ChatId")
                        .HasDatabaseName("ix_messages_chat_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_messages_user_id");

                    b.ToTable("messages", (string)null);
                });

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

                    b.Property<int>("ManagerId")
                        .HasColumnType("integer")
                        .HasColumnName("manager_id");

                    b.Property<GroupType>("Type")
                        .HasColumnType("group_type")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_groups");

                    b.HasIndex("ManagerId")
                        .HasDatabaseName("ix_groups_manager_id");

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

            modelBuilder.Entity("Models.Issue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("ImgLink")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("img_link");

                    b.Property<DateTimeOffset>("IssueMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("issue_moment");

                    b.Property<IssueType>("IssueType")
                        .HasColumnType("issue_type")
                        .HasColumnName("issue_type");

                    b.Property<IssueStatus>("Status")
                        .HasColumnType("issue_status")
                        .HasColumnName("status");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_issues");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_issues_user_id");

                    b.ToTable("issues", (string)null);
                });

            modelBuilder.Entity("Models.Snapshot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("BeginMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("begin_moment");

                    b.Property<DateTimeOffset>("CreateMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("create_moment");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<DateTimeOffset>("EndMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_moment");

                    b.Property<SnapshotAuditType>("SnapshotAuditType")
                        .HasColumnType("snapshot_audit_type")
                        .HasColumnName("snapshot_audit_type");

                    b.Property<SnapshotType>("SnapshotType")
                        .HasColumnType("snapshot_type")
                        .HasColumnName("snapshot_type");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_snapshots");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_snapshots_user_id");

                    b.ToTable("snapshots", (string)null);
                });

            modelBuilder.Entity("Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("AccountCreation")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("account_creation");

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

                    b.Property<UserRole>("Role")
                        .HasColumnType("user_role")
                        .HasColumnName("role");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_name");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Models.UserDeviceMap", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<string>("FirebaseToken")
                        .HasColumnType("text")
                        .HasColumnName("firebase_token");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<DateTimeOffset>("TokenSetMoment")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("token_set_moment");

                    b.HasKey("UserId", "FirebaseToken")
                        .HasName("pk_user_devices");

                    b.ToTable("user_devices", (string)null);
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

            modelBuilder.Entity("Models.DirectChat", b =>
                {
                    b.HasOne("Models.User", "User1")
                        .WithMany("DirectChatsForUserHome")
                        .HasForeignKey("User1Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_direct_chats_users_user_id");

                    b.HasOne("Models.User", "User2")
                        .WithMany("DirectChatsForUserAway")
                        .HasForeignKey("User2Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_direct_chats_users_user_id1");

                    b.HasOne("Models.User", null)
                        .WithMany("DirectChats")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_direct_chats_users_user_id11");

                    b.Navigation("User1");

                    b.Navigation("User2");
                });

            modelBuilder.Entity("Models.DirectMessage", b =>
                {
                    b.HasOne("Models.DirectChat", "Chat")
                        .WithMany("DirectMessages")
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_messages_direct_chats_chat_id");

                    b.HasOne("Models.User", "User")
                        .WithMany("Messages")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_messages_users_user_id");

                    b.Navigation("Chat");

                    b.Navigation("User");
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

            modelBuilder.Entity("Models.Group", b =>
                {
                    b.HasOne("Models.User", "Manager")
                        .WithMany("ManagedGroups")
                        .HasForeignKey("ManagerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_groups_users_manager_id");

                    b.Navigation("Manager");
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

            modelBuilder.Entity("Models.Issue", b =>
                {
                    b.HasOne("Models.User", "User")
                        .WithMany("Issues")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_issues_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Models.Snapshot", b =>
                {
                    b.HasOne("Models.User", "User")
                        .WithMany("Snapshots")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_snapshots_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Models.UserDeviceMap", b =>
                {
                    b.HasOne("Models.User", "User")
                        .WithMany("Devices")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_devices_users_user_id");

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

            modelBuilder.Entity("Models.DirectChat", b =>
                {
                    b.Navigation("DirectMessages");
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
                    b.Navigation("Devices");

                    b.Navigation("DirectChats");

                    b.Navigation("DirectChatsForUserAway");

                    b.Navigation("DirectChatsForUserHome");

                    b.Navigation("EventMaps");

                    b.Navigation("GroupingMaps");

                    b.Navigation("Issues");

                    b.Navigation("ManagedEvents");

                    b.Navigation("ManagedGroups");

                    b.Navigation("Messages");

                    b.Navigation("ReportedTasks");

                    b.Navigation("Snapshots");

                    b.Navigation("TasksForImplementation");
                });
#pragma warning restore 612, 618
        }
    }
}
