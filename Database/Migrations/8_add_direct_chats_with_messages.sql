CREATE TABLE direct_chats (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    caption text NOT NULL,
    create_time timestamp with time zone NOT NULL,
    user1id integer NOT NULL,
    user2id integer NOT NULL,
    user_id integer NULL,
    CONSTRAINT pk_direct_chats PRIMARY KEY (id),
    CONSTRAINT fk_direct_chats_users_user_id FOREIGN KEY (user1id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT fk_direct_chats_users_user_id1 FOREIGN KEY (user2id) REFERENCES users (id) ON DELETE CASCADE,
    CONSTRAINT fk_direct_chats_users_user_id11 FOREIGN KEY (user_id) REFERENCES users (id));

CREATE TABLE messages (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    send_time timestamp with time zone NOT NULL,
    text text NOT NULL,
    is_edited boolean NOT NULL,
    user_id integer NOT NULL,
    chat_id integer NOT NULL,
    CONSTRAINT pk_messages PRIMARY KEY (id),
    CONSTRAINT fk_messages_direct_chats_chat_id FOREIGN KEY (chat_id) REFERENCES direct_chats (id) ON DELETE CASCADE,
    CONSTRAINT fk_messages_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE);

CREATE INDEX ix_direct_chats_user_id ON direct_chats (user_id);
CREATE INDEX ix_direct_chats_user1id ON direct_chats (user1id);
CREATE INDEX ix_direct_chats_user2id ON direct_chats (user2id);
CREATE INDEX ix_messages_chat_id ON messages (chat_id);
CREATE INDEX ix_messages_user_id ON messages (user_id);