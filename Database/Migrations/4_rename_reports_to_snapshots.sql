DROP TABLE reports;

CREATE TYPE snapshot_type AS ENUM ('none', 'events_snapshot', 'tasks_snapshot', 'issues_snapshot', 'reports_snapshot');
DROP TYPE report_type;

CREATE TABLE snapshots (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    description text NOT NULL,
    snapshot_type snapshot_type NOT NULL,
    begin_moment timestamp with time zone NOT NULL,
    end_moment timestamp with time zone NOT NULL,
    user_id integer NOT NULL,
    CONSTRAINT pk_snapshots PRIMARY KEY (id),
    CONSTRAINT fk_snapshots_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE);

CREATE INDEX ix_snapshots_user_id ON snapshots (user_id);