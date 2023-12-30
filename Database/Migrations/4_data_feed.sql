INSERT INTO groups (group_name, type) VALUES ('Add dot net pls', 'educational');
INSERT INTO groups(group_name, type) VALUES ('ADF', 'job');
INSERT INTO groups(group_name, type) VALUES ('pri120', 'educational');

INSERT INTO users (user_name, email, password, phone_number, auth_token) 
VALUES ('Kirill', 'kirill.parakhin@altenar.com', 'kirill2002', '+79042555027', '0347837483');
INSERT INTO users (user_name, email, password, phone_number, auth_token) 
VALUES ('Tigeroff', 'parahinkirill2002@yandex.ru', 'tigeroff2002', '+79032565027', '0745637489');
INSERT INTO users (user_name, email, password, phone_number, auth_token) 
VALUES ('Nikita', 'portnovnikitos2002@yandex.ru', 'nikita2002', '+79302555027', '0890837483');

INSERT INTO events 
(caption, description, scheduled_start, duration, event_type, status, related_group_id, manager_id)
VALUES ('StandUp', 'Every day morning standup meeting', '2024-01-20 12:00:00.000 +0300',
        '00:15:00', 'stand_up', 'not_started', 2, 1);
INSERT INTO events 
(caption, description, scheduled_start, duration, event_type, status, related_group_id, manager_id)
VALUES ('Lection trmp', 'Every week trmp lection', '2024-01-15 10:20:00.000 +0300',
        '01:30:00', 'meeting', 'not_started', 3, 2);
INSERT INTO events 
(caption, description, scheduled_start, duration, event_type, status, related_group_id, manager_id)
VALUES ('Olimpiad discussion', 'Discussion about december tour of ICPC olimpiad', '2024-01-27 14:00:00.000 +0300',
        '03:00:00', 'meeting', 'not_started', 1, 1);
INSERT INTO events 
(caption, description, scheduled_start, duration, event_type, status, related_group_id, manager_id)
VALUES ('Retro', 'Project retro discussion', '2024-02-22 14:30:00.000 +0300',
        '00:30:00', 'stand_up', 'not_started', 2, 1);

INSERT INTO groups_users_map (user_id, group_id)
VALUES (1, 1);
INSERT INTO groups_users_map (user_id, group_id)
VALUES (1, 2);
INSERT INTO groups_users_map (user_id, group_id)
VALUES (1, 3);
INSERT INTO groups_users_map (user_id, group_id)
VALUES (2, 1);
INSERT INTO groups_users_map (user_id, group_id)
VALUES (2, 3);
INSERT INTO groups_users_map (user_id, group_id)
VALUES (3, 2);
INSERT INTO groups_users_map (user_id, group_id)
VALUES (3, 3);

INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (1, 1, 'apply');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (3, 1, 'default');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (1, 2, 'apply');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (2, 2, 'apply');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (3, 2, 'apply');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (1, 3, 'apply');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (2, 3, 'apply');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (1, 4, 'apply');
INSERT INTO events_users_map (user_id, event_id, decision_type)
VALUES (3, 4, 'deny');

INSERT INTO tasks (caption, description, task_type, task_status, reporter_id, implementer_id)
VALUES ('Create new backend .net server', 'Create asp.net server for trmp course work', 
       'job_complete', 'in_progress', 1, 3);
INSERT INTO tasks (caption, description, task_type, task_status, reporter_id, implementer_id)
VALUES ('Create new olimpiad plan', 'Create plan for preparation to ICPC olimpiad', 
       'abstract_goal', 'to_do', 1, 2);
INSERT INTO tasks (caption, description, task_type, task_status, reporter_id, implementer_id)
VALUES ('December meeting presense', 'Be presense on december ADF common meetings', 
       'meeting_presense', 'in_progress', 1, 1);

INSERT INTO reports (description, report_type, begin_moment, end_moment, user_id)
VALUES ('Empty report about Kirill`s events in december', 'events_report',
        '2024-01-01 12:00:00.000 +0300', '2024-01-30 00:00:00.000 +0300', 1);
INSERT INTO reports (description, report_type, begin_moment, end_moment, user_id)
VALUES ('Empty report about Nikits`s tasks in december beginning', 'tasks_report',
        '2024-01-01 14:00:00.000 +0300', '2024-01-10 10:00:00.000 +0300', 3);
