ALTER TABLE users ADD account_creation timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '0001-01-01 00:00:00+00:00';

CREATE TABLE user_devices (
    user_id integer NOT NULL,
    firebase_token text NOT NULL,
    token_set_moment timestamp with time zone NOT NULL,
    is_active boolean NOT NULL,
    CONSTRAINT pk_user_devices PRIMARY KEY (user_id, firebase_token),
    CONSTRAINT fk_user_devices_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE);