INSERT INTO users (user_name, role, email, password, phone_number, auth_token, account_creation) 
VALUES ('Kirill', 'admin', 'kirill.parakhin@altenar.com', 'kirill2002', '+79042555027', '0347837483', '2023-01-20 12:00:00.000 +0300');
INSERT INTO users (user_name, role, email, password, phone_number, auth_token, account_creation) 
VALUES ('Tigeroff', 'user', 'parahinkirill2002@yandex.ru', 'tigeroff2002', '+79032565027', '0745637489', '2023-01-12 12:00:00.000 +0300');
INSERT INTO users (user_name, role, email, password, phone_number, auth_token, account_creation) 
VALUES ('Nikita', 'user', 'portnovnikitos2002@yandex.ru', 'nikita2002', '+79302555027', '0890837483', '2024-01-01 12:00:00.000 +0300');

INSERT INTO groups (group_name, type, manager_id) VALUES ('Add dot net pls', 'educational', 1);
INSERT INTO groups(group_name, type, manager_id) VALUES ('ADF', 'job', 1);
INSERT INTO groups(group_name, type, manager_id) VALUES ('pri120', 'educational', 1);

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

INSERT INTO snapshots (description, snapshot_type, begin_moment, end_moment, user_id, create_moment)
VALUES ('Empty snapshot about Kirill`s events in january', 'events_snapshot',
        '2024-01-01 12:00:00.000 +0300', '2024-01-30 00:00:00.000 +0300', 1, '2024-02-02 12:00:00.000 +0300');
INSERT INTO snapshots (description, snapshot_type, begin_moment, end_moment, user_id, create_moment)
VALUES ('Empty snapshot about Nikits`s tasks in february beginning', 'tasks_snapshot',
        '2024-02-01 14:00:00.000 +0300', '2024-02-10 10:00:00.000 +0300', 3, '2024-02-23 12:00:00.000 +0300');

INSERT INTO issues (issue_type, status, title, description, img_link, issue_moment, user_id)
VALUES ('violation_issue', 'reported', 'Reporting user portnovnikitos2002@yandex.ru', 'Violation of project rules by user Kirill',
        'https://ir.ozone.ru/s3/multimedia-6/wc1000/6768115818.jpg', '2024-02-28 12:00:00.000 +0300', 1);
INSERT INTO issues (issue_type, status, title, description, img_link, issue_moment, user_id)
VALUES ('violation_issue', 'closed', 'Reporting user kirill.parakhin@altenar.com', 'Violation of project rules by user Nikita',
        'https://ir.ozone.ru/s3/multimedia-1/wc1000/6266202541.jpg', '2024-02-29 12:00:00.000 +0300', 1);

INSERT INTO direct_chats (caption, create_time, user1id, user2id)
VALUES ('Kirill-Nikita', '2024-03-01 12:00:00.000 +0300', 1, 3);
INSERT INTO direct_chats (caption, create_time, user1id, user2id)
VALUES ('Tigeroff-Kirill', '2024-03-03 17:00:00.000 +0300', 2, 1);

INSERT INTO messages (send_time, text, is_edited, user_id, chat_id)
VALUES ('2024-03-01 12:00:00.000 +0300', 'Hello, Nikita!', false, 1, 1);
INSERT INTO messages (send_time, text, is_edited, user_id, chat_id)
VALUES ('2024-03-01 12:30:00.000 +0300', 'Hi you to, Kirill!', false, 2, 1);
INSERT INTO messages (send_time, text, is_edited, user_id, chat_id)
VALUES ('2024-03-01 12:40:00.000 +0300', 'How are you today?', false, 1, 1);

INSERT INTO messages (send_time, text, is_edited, user_id, chat_id)
VALUES ('2024-03-03 17:00:00.000 +0300', 'Hello, team leader Kirill! I have done my work for today', false, 2, 2);
INSERT INTO messages (send_time, text, is_edited, user_id, chat_id)
VALUES ('2024-03-03 17:10:00.000 +0300', 'You may be free, Tigeroff', false, 1, 2);