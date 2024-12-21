# 🔐 Sandbox.FullStackIdentity

**FullStackIdentity** is one of the sandbox projects that is made to test `Asp.NetCore.Identity` integrations within a containerized full-stack web application. This project contains back-end services built with **ASP.NET Core**.

# 🎯 Test Targets

The project is built to test the following workflows:
- JWT authentication (Generation, Expiration, Refresh, Revoke, etc..)
- Email confirmation and password reset.
- Multitenancy and invitation-based registration.
- Role-based authorization.
- Dapper implementation of **Identity Stores** with `PostgreSQL`.
- Client-based reverse proxy system.

# 🧰 Used Technologies

- PostgreSQL
- Dapper
- Hangfire
- Redis
- RedLock
- DbUp
- Serilog
- SendGrid
- Docker