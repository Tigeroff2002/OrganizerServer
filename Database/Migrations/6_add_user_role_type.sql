CREATE TYPE user_role AS ENUM ('none', 'user', 'admin');
ALTER TABLE users ADD role user_role NOT NULL DEFAULT 'none'::user_role;