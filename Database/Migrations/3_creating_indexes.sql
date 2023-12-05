      CREATE INDEX ix_events_manager_id ON events (manager_id);
      CREATE INDEX ix_events_related_group_id ON events (related_group_id);
      CREATE INDEX ix_events_users_map_event_id ON events_users_map (event_id);
      CREATE INDEX ix_groups_users_map_group_id ON groups_users_map (group_id);
      CREATE INDEX ix_reports_user_id ON reports (user_id);
      CREATE INDEX ix_tasks_implementer_id ON tasks (implementer_id);
      CREATE INDEX ix_tasks_reporter_id ON tasks (reporter_id);