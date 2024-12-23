
-- This enum is required since Npgsql doesn't have support for int to enum conversion within composite types.
CREATE TYPE publication_status AS ENUM ('draft', 'published');

CREATE TYPE book_details AS (
    author varchar(128),
    publisher varchar(128),
    publish_date timestamp,
    publication_status publication_status,
    pages_count int
);
