# Deep Overflow – Complete Project Documentation

**Enterprise Stack Overflow for RMES (Resource Management & Engineering Solutions)**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Tech Stack & Languages](#2-tech-stack--languages)
3. [Architecture](#3-architecture)
4. [Database](#4-database)
5. [Backend Structure](#5-backend-structure)
6. [Frontend Structure](#6-frontend-structure)
7. [API Endpoints](#7-api-endpoints)
8. [Authentication & Security](#8-authentication--security)
9. [Features & User Flows](#9-features--user-flows)
10. [How to Run](#10-how-to-run)
11. [Configuration](#11-configuration)
12. [FAQ – Questions You Might Be Asked](#12-faq--questions-you-might-be-asked)

---

## 1. Overview

**Deep Overflow** is a Q&A platform similar to Stack Overflow, built for RMES. Users can:

- Ask questions
- Answer questions
- Vote on questions and answers
- Search and filter questions
- Register and log in
- View question details with answers

---

## 2. Tech Stack & Languages

| Layer | Technology | Language |
|-------|------------|----------|
| **Frontend** | Angular 17 | TypeScript |
| **Backend** | ASP.NET Core 8 Web API | C# |
| **Database** | SQLite (dev) / PostgreSQL (prod) | SQL |
| **ORM** | Entity Framework Core 8 | C# |
| **Auth** | JWT (JSON Web Tokens) | - |
| **UI** | Angular Material, SCSS | TypeScript/CSS |

### Key Dependencies

**Frontend:**
- Angular 17 (core, router, forms)
- Angular Material (UI components, SnackBar)
- RxJS, SignalR client
- ngx-markdown, Prism.js (for markdown/code)

**Backend:**
- .NET 8
- Entity Framework Core 8
- MediatR (CQRS)
- Serilog (logging)
- Swagger/OpenAPI
- AspNetCoreRateLimit
- SignalR (real-time)

---

## 3. Architecture

### Clean Architecture (Backend)

The backend follows **Clean Architecture** with 4 layers:

```
┌─────────────────────────────────────────────────────────┐
│                    DeepOverflow.API                      │  ← Controllers, Program.cs
├─────────────────────────────────────────────────────────┤
│              DeepOverflow.Application                    │  ← Commands, Queries, Interfaces
├─────────────────────────────────────────────────────────┤
│              DeepOverflow.Infrastructure                │  ← EF, Repositories, Services
├─────────────────────────────────────────────────────────┤
│                DeepOverflow.Domain                       │  ← Entities, Enums, Interfaces
└─────────────────────────────────────────────────────────┘
```

- **Domain**: Entities, enums, repository interfaces. No external dependencies.
- **Application**: Business logic (MediatR commands/queries), interfaces.
- **Infrastructure**: EF Core, repositories, JWT, password hashing, etc.
- **API**: Controllers, middleware, configuration.

### CQRS Pattern

- **Commands**: CreateQuestion, VoteQuestion, VoteAnswer, etc.
- **Queries**: GetQuestion (by slug/ID)
- Uses **MediatR** for request/response handling.

### Frontend Architecture

- **Standalone components** (Angular 17)
- **Lazy-loaded routes** for questions and auth
- **Services**: AuthService, QuestionService
- **Interceptors**: Auth (JWT), Error handling
- **Guards**: AuthGuard for protected routes

---

## 4. Database

### Database Options

| Environment | Database | File/Connection |
|-------------|----------|----------------|
| **Development** | SQLite | `deepoverflow.dev.db` (auto-created) |
| **Production** | PostgreSQL | Connection string in appsettings |

SQLite is used when the PostgreSQL connection string is missing or contains placeholder values.

### Schema – Main Tables

| Table | Purpose |
|-------|---------|
| **Users** | User accounts (email, username, password hash, role, reputation) |
| **Questions** | Questions (title, body, slug, author, status, vote score, view count) |
| **Answers** | Answers linked to questions |
| **Tags** | Tags (name, description) |
| **QuestionTags** | Many-to-many: Question ↔ Tag |
| **Votes** | Polymorphic votes (Question, Answer, Comment) |
| **Comments** | Comments on questions/answers |
| **Bookmarks** | User bookmarks |
| **EditHistory** | Edit history for questions/answers |
| **Flags** | Moderation flags |
| **Notifications** | User notifications |
| **ReputationHistory** | Reputation changes |
| **BadgeDefinitions** / **UserBadges** | Badges |
| **AuditLog** | Audit trail |

### Key Entity Relationships

```
User 1───* Question (Author)
User 1───* Answer (Author)
Question 1───* Answer
Question *───* Tag (via QuestionTag)
Vote → (Question | Answer | Comment) [polymorphic via TargetType + TargetId]
```

### Vote Table

- **TargetType**: Question (0), Answer (1), Comment (2)
- **TargetId**: ID of the target
- **UserId**: Who voted
- **VoteType**: Upvote (1) or Downvote (-1)
- **Unique constraint**: (UserId, TargetType, TargetId) – one vote per user per item

---

## 5. Backend Structure

### Solution Projects

```
DeepOverflow.sln
├── DeepOverflow.Domain          # Entities, Enums, Interfaces
├── DeepOverflow.Application     # Commands, Queries, DTOs
├── DeepOverflow.Infrastructure  # EF, Repositories, JWT, etc.
└── DeepOverflow.API             # Controllers, Program.cs
```

### Domain Layer

- **Entities**: User, Question, Answer, Tag, Vote, Comment, etc.
- **Enums**: QuestionStatus, VoteType, VoteTargetType, UserRole, AuthProvider
- **BaseEntity**: Id, CreatedAt, UpdatedAt, DeletedAt

### Application Layer

- **Commands**: CreateQuestion, VoteQuestion, VoteAnswer
- **Queries**: GetQuestion
- **Result<T>**: Success/failure pattern
- **Interfaces**: ICurrentUserService, ITokenService, IPasswordHasher, etc.

### Infrastructure Layer

- **Repositories**: QuestionRepository, AnswerRepository, VoteRepository, UserRepository, etc.
- **UnitOfWork**: Transaction management
- **Services**: JwtTokenService, Pbkdf2PasswordHasher, NoOpAIService, NoOpCacheService
- **ApplicationDbContext**: EF Core DbContext

---

## 6. Frontend Structure

```
frontend/src/
├── app/
│   ├── app.component.ts
│   ├── app.config.ts
│   ├── app.routes.ts
│   ├── core/
│   │   ├── guards/          # AuthGuard
│   │   ├── interceptors/   # Auth, Error
│   │   └── services/        # AuthService, QuestionService
│   ├── features/
│   │   ├── auth/
│   │   │   ├── login/
│   │   │   ├── register/
│   │   │   └── auth.routes.ts
│   │   └── questions/
│   │       ├── question-list/
│   │       ├── question-detail/
│   │       ├── question-form/
│   │       └── questions.routes.ts
│   └── layout/
│       └── main-layout/     # Header, footer, nav
├── assets/
│   └── default-avatar.svg
├── environments/
└── styles.scss
```

### Routes

| Path | Component | Description |
|------|-----------|-------------|
| `/` | Redirect to `/questions` | - |
| `/questions` | QuestionListComponent | List of questions |
| `/questions/ask` | QuestionFormComponent | Ask a question |
| `/questions/:slug` | QuestionDetailComponent | Question + answers |
| `/auth/login` | LoginComponent | Login |
| `/auth/register` | RegisterComponent | Register |

---

## 7. API Endpoints

### Auth (`/api/auth`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/login` | Login with email/password |
| POST | `/register` | Register new user |
| POST | `/logout` | Logout (requires auth) |
| POST | `/refresh` | Refresh access token |
| POST | `/sso` | SSO login (stub) |
| GET | `/verify-email` | Email verification |

### Questions (`/api/questions`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | List questions (paginated, filterable) |
| GET | `/{idOrSlug}` | Get question by ID or slug |
| POST | `/` | Create question (auth) |
| PUT | `/{id}` | Update question (auth) |
| DELETE | `/{id}` | Delete question (auth) |
| POST | `/{id}/vote` | Vote on question (auth) |
| POST | `/{id}/bookmark` | Bookmark question (auth) |
| GET | `/{id}/similar` | Similar questions (stub) |

### Answers (`/api/answers`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/` | Create answer (auth) |
| GET | `/{id}` | Get answer |
| PUT | `/{id}` | Update answer (auth) |
| DELETE | `/{id}` | Delete answer (auth) |
| POST | `/{id}/accept` | Accept answer (auth) |
| POST | `/{id}/vote` | Vote on answer (auth) |

### Query Parameters (GET /api/questions)

- `page`, `pageSize`: Pagination
- `sortBy`: `newest`, `active`, `unanswered`, `votes`
- `q`: Search (title/body)
- `tags`: Filter by tags
- `status`: Filter by status

---

## 8. Authentication & Security

### JWT Authentication

- **Access token**: Short-lived (60 min default)
- **Refresh token**: Longer-lived (7 days)
- **Header**: `Authorization: Bearer <token>`
- **Validation**: Issuer, audience, signing key, expiry

### Password Hashing

- **Algorithm**: PBKDF2
- Passwords are never stored in plain text.

### Security Features

- **CORS**: Allowed origins (e.g. `http://localhost:3000`)
- **Rate limiting**: 100 req/min general, 30 req/min for auth
- **Account lockout**: After 5 failed logins, 15-minute lock
- **Roles**: User, Moderator, Admin

### Auth Flow

1. User logs in → API returns `accessToken`, `refreshToken`, `user`
2. Frontend stores tokens in `localStorage`
3. `AuthInterceptor` adds `Authorization: Bearer <token>` to requests
4. Protected routes use `AuthGuard`

---

## 9. Features & User Flows

### Question Filters

| Filter | Behavior |
|--------|----------|
| **Newest** | Order by `CreatedAt` desc |
| **Active** | Order by `LastActivityAt` desc |
| **Unanswered** | Only `AnswerCount == 0` |
| **Most Votes** | Order by `VoteScore` desc |

### Voting

- One vote per user per question/answer (upvote or downvote)
- Clicking the same button again does nothing (no toggle-off)
- Clicking the opposite button changes the vote

### Create Question

- Optional “I have a solution” field
- If provided, question and accepted answer are created together

### Login / Register

- Success popup (SnackBar) with username
- Redirect to Questions page

---

## 10. How to Run

### Prerequisites

- Node.js 18+
- .NET 8 SDK
- (Optional) PostgreSQL for production

### Backend

```bash
cd backend/src/DeepOverflow.API
dotnet run
```

- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- SQLite DB is created automatically if PostgreSQL is not configured

### Frontend

```bash
cd frontend
npm install
npx ng serve
```

- App: `http://localhost:3000`

### Environment

- Frontend `environment.ts`: `apiUrl: 'http://localhost:5000/api'`
- Backend CORS must include `http://localhost:3000`

---

## 11. Configuration

### Backend (appsettings.json)

| Section | Key | Description |
|---------|-----|-------------|
| ConnectionStrings | DefaultConnection | PostgreSQL connection |
| JWT | Secret, Issuer, Audience | JWT settings |
| CORS | AllowedOrigins | Frontend URLs |
| IpRateLimiting | GeneralRules | Rate limits |
| Reputation | QuestionUpvoted, etc. | Reputation points |

### Frontend (environment.ts)

| Key | Description |
|-----|-------------|
| apiUrl | Backend API base URL |
| signalRUrl | SignalR hub URL |
| production | Build mode |

---

## 12. FAQ – Questions You Might Be Asked

### What is Deep Overflow?

A Stack Overflow–style Q&A platform for RMES (Resource Management & Engineering Solutions).

### What technologies are used?

- **Frontend**: Angular 17, TypeScript
- **Backend**: ASP.NET Core 8, C#
- **Database**: SQLite (dev) / PostgreSQL (prod)
- **Auth**: JWT

### What is the architecture?

Clean Architecture with Domain, Application, Infrastructure, and API layers. CQRS with MediatR.

### How is the database structured?

Main entities: Users, Questions, Answers, Tags, Votes, Comments. Polymorphic votes via `TargetType` and `TargetId`. Many-to-many Question–Tag via `QuestionTag`.

### How does authentication work?

JWT-based. Login returns access + refresh tokens. Frontend stores them and sends the access token in the `Authorization` header.

### How does voting work?

One vote per user per question/answer. Upvote = +1, Downvote = -1. No toggle-off on repeated clicks.

### How do the question filters work?

- **Newest**: By creation date
- **Active**: By last activity (answers, edits)
- **Unanswered**: Only questions with 0 answers
- **Most Votes**: By vote count

### How do I run it?

Backend: `dotnet run` in `backend/src/DeepOverflow.API`  
Frontend: `npx ng serve` in `frontend`

### Where is the code?

- Backend: `backend/src/`
- Frontend: `frontend/src/`
- Docs: `DEEP_OVERFLOW_DOCUMENTATION.md`

---

*Document generated for Deep Overflow – Enterprise Stack Overflow for RMES*
