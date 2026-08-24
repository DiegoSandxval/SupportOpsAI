---
title: "SupportOps AI"
description: "A role-based support ticket API built with ASP.NET Core and .NET 10, featuring ticket lifecycle management, comments, audit history, JWT authentication, and an architecture prepared for AI-assisted ticket triage."

category: "backend"

technologies:
  - ".NET 10"
  - "ASP.NET Core"
  - "Entity Framework Core"
  - "SQL Server"
  - "JWT Authentication"
  - "Swagger / OpenAPI"

status: "in-progress"

featured: true
draft: false
---

## About the project

SupportOps AI is a backend support ticket management system designed around clear role-based workflows for Users, Agents, and Administrators.

The API provides secure authentication, ticket assignment and lifecycle management, internal and public comments, ticket history, and role-specific authorization rules.

The project also includes an application-layer abstraction for AI-assisted ticket analysis so support staff can receive suggested categories, priorities, summaries, and reasoning without allowing AI to modify tickets automatically.

## Main features

- JWT authentication and role-based authorization.
- User, Agent, and Admin roles.
- Admin-managed user creation.
- Ticket creation, listing, retrieval, and updates.
- Ticket assignment to support agents.
- Ticket lifecycle: Open, Assigned, InProgress, Resolved, and Closed.
- Automatic resolution and closure timestamps.
- Ticket history and audit tracking.
- Public and internal ticket comments.
- Internal comments hidden from regular users.
- Swagger / OpenAPI documentation and testing.
- AI ticket triage architecture prepared for future model integration.

## Architecture

The solution separates Domain, Application, Infrastructure, and API responsibilities. Repository abstractions, handlers, domain entities, and authorization policies keep the application modular and make it easier to extend with additional support workflows or AI providers later.

## Current status

The core ticketing API and authorization flows are working and tested. Live AI analysis is currently paused until an external AI provider is configured.
