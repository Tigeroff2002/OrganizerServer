CREATE TYPE issue_status AS ENUM ('none', 'reported', 'in_progress', 'closed');

ALTER TABLE issues ADD status issue_status NOT NULL DEFAULT 'none'::issue_status;

ALTER TABLE snapshots ADD create_moment timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '0001-01-01 00:00:00+00:00';

ALTER TABLE groups ADD manager_id integer NOT NULL DEFAULT 0;

CREATE INDEX ix_groups_manager_id ON groups (manager_id);

ALTER TABLE groups ADD CONSTRAINT fk_groups_users_manager_id FOREIGN KEY (manager_id) REFERENCES users (id) ON DELETE CASCADE;
