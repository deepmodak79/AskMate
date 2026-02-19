# Deep Overflow – Project Detail Report

**Document Version:** 1.0  
**Report Date:** February 2026  
**Project:** Enterprise Stack Overflow for RMES (Resource Management & Engineering Solutions)

---

## Executive Summary

**Deep Overflow** is an enterprise-grade Q&A platform modeled after Stack Overflow, built for RMES. It enables internal knowledge sharing for technical topics including rugged monitoring systems, substation automation, SCADA, networking, hardware, firmware, and DevOps.

The application is **fully deployed** and operational:
- **Frontend:** GitHub Pages at `https://deepmodak79.github.io/AskMate/`
- **Backend API:** Render at `https://askmate-xsv8.onrender.com`

---

## 1. Project Overview

| Attribute | Details |
|----------|---------|
| **Project Name** | Deep Overflow |
| **Repository** | `deepmodak79/AskMate` (GitHub) |
| **Purpose** | Internal Q&A knowledge platform for RMES engineering team |
| **Type** | Full-stack web application (SPA + REST API) |
| **Status** | Production (deployed) |

### Core Capabilities

- Ask and answer technical questions
- Vote on questions and answers (upvote/downvote)
- Search and filter questions (newest, active, unanswered, most votes)
- User registration and JWT authentication
- Post questions with optional solutions (Q&A in one step)
- Tag-based organization
- User profiles with name, department, and reputation
- Dark/light theme support

---

## 2. Technology Stack

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| Angular | 17.x | SPA framework |
| TypeScript | 5.2 | Language |
| Angular Material | 17.x | UI components |
| RxJS | 7.8 | Reactive programming |
| ngx-markdown | 17.x | Markdown rendering |
| SignalR client | 8.x | Real-time notifications |

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Runtime |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM |
| MediatR | - | CQRS pattern |
| Serilog | 8.0 | Logging |
| Swagger/OpenAPI | 6.5 | API documentation |
| AspNetCoreRateLimit | 5.0 | Rate limiting |
| SignalR | 1.1 | Real-time hub |

### Database

| Environment | Database | Notes |
|-------------|----------|-------|
| Development | SQLite | Auto-created `deepoverflow.dev.db` |
| Production (Render free) | SQLite | Ephemeral – resets on redeploy |
| Production (optional) | PostgreSQL | Persistent – add via Render |

### Infrastructure & DevOps

| Tool | Purpose |
|------|---------|
| GitHub Actions | Frontend CI/CD (deploy to GitHub Pages) |
| Render | Backend hosting (Docker) |
| GitHub Pages | Frontend hosting |
| Docker | Backend containerization |

---

## 3. Architecture

### Backend – Clean Architecture

```
┌─────────────────────────────────────────────────────────┐
│  DeepOverflow.API          Controllers, Program.cs      │
├─────────────────────────────────────────────────────────┤
│  DeepOverflow.Application  Commands, Queries, MediatR   │
├─────────────────────────────────────────────────────────┤
│  DeepOverflow.Infrastructure  EF Core, Repositories     │
├─────────────────────────────────────────────────────────┤
│  DeepOverflow.Domain        Entities, Enums, Interfaces │
└─────────────────────────────────────────────────────────┘
```

- **Domain:** Entities (User, Question, Answer, Tag, Vote, etc.), enums, repository interfaces
- **Application:** CQRS with MediatR (CreateQuestion, VoteQuestion, VoteAnswer, GetQuestion)
- **Infrastructure:** EF Core, repositories, JWT, password hashing, NoOp services (AI, Search, Cache)
- **API:** REST controllers, SignalR hub, middleware

### Frontend – Feature-Based Structure

```
frontend/src/app/
├── core/           Auth, interceptors, guards, services
├── features/       auth (login, register), questions (list, detail, form)
├── layout/         main-layout (header, footer, nav)
└── shared/         Reusable components
```

### Key Patterns

- **CQRS:** Commands and queries via MediatR
- **Repository + Unit of Work:** Data access abstraction
- **JWT Authentication:** Stateless auth with access + refresh tokens
- **Result&lt;T&gt;:** Success/failure pattern for command handlers

---

## 4. Main Features

### Implemented

| Feature | Description |
|---------|-------------|
| **Questions** | List, create, view by slug, filter (newest/active/unanswered/votes), search |
| **Answers** | Create, vote, accept (question owner) |
| **Voting** | One vote per user per question/answer; real-time UI update |
| **Auth** | Register, login, logout; JWT stored in localStorage |
| **User Profile** | Display name, department/team, reputation, avatar in header |
| **Tags** | Comma-separated; at least one required per question |
| **Question + Solution** | Optional “I have a solution” – posts question and accepted answer together |
| **404 Fix** | SPA routing on GitHub Pages via 404.html copy |
| **Theme** | Dark/light mode toggle |

### API Endpoints Summary

| Controller | Key Endpoints |
|------------|---------------|
| **Auth** | POST /login, /register, /logout, /refresh |
| **Questions** | GET /, GET /{idOrSlug}, POST /, POST /{id}/vote |
| **Answers** | POST /, GET /{id}, POST /{id}/vote, POST /{id}/accept |

---

## 5. Deployment

### Frontend (GitHub Pages)

| Item | Value |
|------|-------|
| **URL** | https://deepmodak79.github.io/AskMate/ |
| **Base path** | `/AskMate/` |
| **Trigger** | Push to `main` branch |
| **Workflow** | `.github/workflows/deploy-pages.yml` |
| **Build** | `ng build --configuration production --base-href /AskMate/` |
| **SPA routing** | `index.html` copied to `404.html` for hard refresh support |

### Backend (Render)

| Item | Value |
|------|-------|
| **URL** | https://askmate-xsv8.onrender.com |
| **Health** | https://askmate-xsv8.onrender.com/health |
| **Swagger** | https://askmate-xsv8.onrender.com/swagger (dev only) |
| **Runtime** | Docker (backend/Dockerfile) |
| **Port** | 10000 |
| **Instance** | Free tier (spins down after inactivity) |

### Configuration

- **Frontend `environment.prod.ts`:** `apiUrl: 'https://askmate-xsv8.onrender.com/api'`
- **Backend CORS:** Includes `https://deepmodak79.github.io`
- **Database:** SQLite on Render free tier (ephemeral)

---

## 6. Known Limitations & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| **404 on hard refresh** | GitHub Pages serves static files; `/questions` has no file | Fixed: Copy `index.html` to `404.html` in deploy workflow |
| **Posted questions disappear** | Ephemeral SQLite on Render free tier | Add Render PostgreSQL; set `ConnectionStrings__DefaultConnection` |
| **User not found on create question** | JWT has userId from old DB; DB was reset | User must log out and log in again (or re-register) |
| **Slow first request** | Render free instance spins down after ~15 min | Normal; first request can take 30–60 seconds |

---

## 7. Project Structure (Key Paths)

```
Deep Overflow/
├── backend/
│   ├── Dockerfile
│   └── src/
│       ├── DeepOverflow.API/           # Controllers, Program.cs
│       ├── DeepOverflow.Application/   # Commands, Queries
│       ├── DeepOverflow.Domain/        # Entities, Enums
│       └── DeepOverflow.Infrastructure/
├── frontend/
│   ├── package.json
│   ├── angular.json
│   └── src/
│       ├── app/
│       │   ├── core/                   # Auth, interceptors, guards
│       │   ├── features/               # auth, questions
│       │   └── layout/
│       └── environments/
├── .github/workflows/
│   └── deploy-pages.yml
├── render.yaml
├── DEPLOY_BACKEND_STEP_BY_STEP.md
├── DEPLOY_FRONTEND_STEP_BY_STEP.md
├── DEEP_OVERFLOW_DOCUMENTATION.md
└── PROJECT_STRUCTURE.md
```

---

## 8. Quick Start (Local Development)

### Backend

```bash
cd backend/src/DeepOverflow.API
dotnet run
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend

```bash
cd frontend
npm install
npx ng serve
# App: http://localhost:3000
```

### Prerequisites

- Node.js 18+
- .NET 8 SDK

---

## 9. Documentation References

| Document | Purpose |
|----------|---------|
| `README.md` | High-level overview, architecture |
| `DEEP_OVERFLOW_DOCUMENTATION.md` | Detailed technical documentation |
| `PROJECT_STRUCTURE.md` | File tree, features, tech stack |
| `DEPLOY_FRONTEND_STEP_BY_STEP.md` | GitHub Pages deployment guide |
| `DEPLOY_BACKEND_STEP_BY_STEP.md` | Render deployment guide |
| `HOSTING_GUIDE.md` | Hosting options |

---

## 10. Summary

Deep Overflow is a production-deployed Q&A platform for RMES, built with Angular 17 and .NET 8. It uses Clean Architecture, CQRS, and JWT authentication. The frontend is hosted on GitHub Pages and the backend on Render. For persistent data across restarts, a PostgreSQL database should be added on Render.

---

*Report generated for Deep Overflow – Enterprise Stack Overflow for RMES*
