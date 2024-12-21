
BEGIN;


CREATE TABLE global_configs (
    key varchar(64) PRIMARY KEY,
    value varchar(256) NOT NULL
);

CREATE TABLE tenants (
    id uuid PRIMARY KEY,
    name varchar(128),
    handle varchar(128) NOT NULL,
    is_deleted boolean NOT NULL DEFAULT FALSE
);

CREATE UNIQUE INDEX idx_tenants_handle ON tenants (handle);


CREATE TABLE users (
    id uuid PRIMARY KEY,
    tenant_id uuid REFERENCES tenants (id),
    is_invited boolean NOT NULL,
    invitation_accepted boolean NOT NULL,
    granted_permission int NOT NULL,
    user_name varchar(128),
    normalized_user_name varchar(128),
    email varchar(128),
    normalized_email varchar(128),
    email_confirmed boolean NOT NULL,
    password_hash varchar(256),
    security_stamp varchar(256),
    lockout_end timestamptz,
    lockout_enabled boolean NOT NULL,
    access_failed_count int NOT NULL,
    first_name varchar(128),
    last_name varchar(128),
    is_deleted boolean NOT NULL DEFAULT FALSE
);

CREATE INDEX idx_users_tenant_id ON users (tenant_id);
CREATE UNIQUE INDEX idx_users_normalized_user_name ON users (normalized_user_name) WHERE normalized_user_name IS NOT NULL;
CREATE UNIQUE INDEX idx_users_normalized_email ON users (normalized_email) WHERE normalized_email IS NOT NULL;


CREATE TABLE roles (
    id uuid PRIMARY KEY,
    name varchar(128),
    normalized_name varchar(128)
);

CREATE TABLE user_roles (
    user_id uuid NOT NULL REFERENCES users (id),
    role_id uuid NOT NULL REFERENCES roles (id),
    PRIMARY KEY (user_id, role_id)
);


CREATE TABLE refresh_tokens (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users (id),
    token varchar(256) NOT NULL,
    expires_on_utc timestamp NOT NULL
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens (user_id);
CREATE UNIQUE INDEX idx_refresh_tokens_token ON refresh_tokens (token);


CREATE TYPE book_details AS (
    author varchar(128),
    publisher varchar(128),
    publish_date timestamp,
    publication_status int,
    pages_count int,
);

CREATE TABLE books (
    id uuid PRIMARY KEY,
    tenant_id uuid REFERENCES tenants (id),
    owner_id uuid NOT NULL REFERENCES users (id),
    title varchar(128) NOT NULL,
    created_on_utc timestamp NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    details book_details,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    CHECK (details IS NULL OR (details).author IS NOT NULL)
);

CREATE INDEX idx_books_tenant_id ON books (tenant_id);
CREATE INDEX idx_books_owner_id ON books (owner_id);
CREATE INDEX idx_books_created_on_utc ON books (created_on_utc);


COMMIT;
