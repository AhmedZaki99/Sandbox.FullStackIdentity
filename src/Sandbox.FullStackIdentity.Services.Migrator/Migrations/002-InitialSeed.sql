
INSERT INTO global_configs (key, value) 
VALUES ('TokenCleanupJob.Cron', '0 * * * *');


INSERT INTO identity.roles (id, name, normalized_name) 
VALUES ('305b4973-8dfc-48b9-910f-a6f1b843d4ed', 'Administrator', 'ADMINISTRATOR'),
	   ('020ed778-f0dd-4d4e-8b50-164ab898bd12', 'Organization', 'ORGANIZATION');

INSERT INTO identity.users (id, user_name, normalized_user_name, email, normalized_email, email_confirmed) 
VALUES ('a2ce9fa9-1666-4915-b291-c6c825956a19', 'admin', 'ADMIN', 'ahmed.zaki.dev@gmail.com', 'AHMED.ZAKI.DEV@GMAIL.COM', false);

INSERT INTO identity.user_roles (user_id, role_id)
VALUES ('a2ce9fa9-1666-4915-b291-c6c825956a19', '305b4973-8dfc-48b9-910f-a6f1b843d4ed');
