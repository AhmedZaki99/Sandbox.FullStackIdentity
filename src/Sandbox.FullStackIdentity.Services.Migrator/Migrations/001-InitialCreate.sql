
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


CREATE SCHEMA identity;

CREATE TABLE identity.users (
    id uuid PRIMARY KEY,
    user_name varchar(128),
    normalized_user_name varchar(128),
    email varchar(128),
    normalized_email varchar(128),
    email_confirmed boolean NOT NULL DEFAULT FALSE,
    password_hash varchar(256),
    security_stamp varchar(256),
    concurrency_stamp varchar(256),
    phone_number varchar(128),
    phone_number_confirmed boolean NOT NULL DEFAULT FALSE,
    two_factor_enabled boolean NOT NULL DEFAULT FALSE,
    lockout_end timestamptz,
    lockout_enabled boolean NOT NULL DEFAULT FALSE,
    access_failed_count int NOT NULL DEFAULT 0,
    tenant_id uuid REFERENCES tenants (id),
    is_invited boolean NOT NULL DEFAULT FALSE,
    invitation_accepted boolean NOT NULL DEFAULT FALSE,
    granted_permission int NOT NULL DEFAULT 0,
    first_name varchar(128),
    last_name varchar(128),
    is_deleted boolean NOT NULL DEFAULT FALSE
);

CREATE TABLE identity.roles (
    id uuid PRIMARY KEY,
    name varchar(128),
    normalized_name varchar(128),
    concurrency_stamp varchar(256)
);

CREATE TABLE identity.user_roles (
    user_id uuid NOT NULL REFERENCES identity.users (id),
    role_id uuid NOT NULL REFERENCES identity.roles (id),
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE identity.user_claims (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES identity.users (id),
    claim_type varchar(128),
    claim_value varchar(512)
);

CREATE TABLE identity.role_claims (
    id uuid PRIMARY KEY,
    role_id uuid NOT NULL REFERENCES identity.roles (id),
    claim_type varchar(128),
    claim_value varchar(512)
);

CREATE TABLE identity.user_logins (
    login_provider varchar(128) NOT NULL,
    provider_key varchar(128) NOT NULL,
    provider_display_name varchar(128),
    user_id uuid NOT NULL REFERENCES identity.users (id),
    PRIMARY KEY (login_provider, provider_key)
);

CREATE TABLE identity.user_tokens (
    login_provider varchar(128) NOT NULL,
    name varchar(128) NOT NULL,
    value varchar(512),
    user_id uuid NOT NULL REFERENCES identity.users (id),
    PRIMARY KEY (login_provider, name)
);

CREATE UNIQUE INDEX idx_users_normalized_user_name ON identity.users (normalized_user_name) WHERE normalized_user_name IS NOT NULL;
CREATE UNIQUE INDEX idx_users_normalized_email ON identity.users (normalized_email) WHERE normalized_email IS NOT NULL;
CREATE INDEX idx_users_tenant_id ON identity.users (tenant_id);
CREATE INDEX idx_user_claims_user_id ON identity.user_claims (user_id);
CREATE INDEX idx_role_claims_role_id ON identity.role_claims (role_id);


CREATE TABLE refresh_tokens (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES identity.users (id),
    token varchar(256) NOT NULL,
    expires_on_utc timestamp NOT NULL
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens (user_id);
CREATE UNIQUE INDEX idx_refresh_tokens_token ON refresh_tokens (token);


CREATE TABLE books (
    id uuid PRIMARY KEY,
    tenant_id uuid REFERENCES tenants (id),
    owner_id uuid NOT NULL REFERENCES identity.users (id),
    title varchar(128) NOT NULL,
    created_on_utc timestamp NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    details book_details,
    is_deleted boolean NOT NULL DEFAULT FALSE,
    CHECK (details IS NULL OR (details).author IS NOT NULL)
);

CREATE INDEX idx_books_tenant_id ON books (tenant_id);
CREATE INDEX idx_books_owner_id ON books (owner_id);
CREATE INDEX idx_books_created_on_utc ON books (created_on_utc);
