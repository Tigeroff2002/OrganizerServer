CREATE TYPE issue_type AS ENUM ('none', 'bag_issue', 'violation_issue');

CREATE TABLE issues (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    issue_type issue_type NOT NULL,
    title text NOT NULL,
    description text NOT NULL,
    img_link text NOT NULL,
    issue_moment timestamp with time zone NOT NULL,
    user_id integer NOT NULL,
    CONSTRAINT pk_issues PRIMARY KEY (id),
    CONSTRAINT fk_issues_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE);

CREATE INDEX ix_issues_user_id ON issues (user_id);