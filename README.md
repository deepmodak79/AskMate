# Deep Overflow - Enterprise Knowledge Platform for RMES

## Overview

**Deep Overflow** is a production-ready, enterprise-grade Stack Overflow clone built specifically for RMES (Rugged Monitoring & Engineering Systems). This platform enables internal knowledge sharing for technical topics including rugged monitoring systems, substation automation, SCADA, networking, hardware, firmware, and DevOps.

## 🏗️ Architecture

### Clean Architecture Layers

```
┌────────────────────────────────────────────────────────────┐
│                      API Layer (Presentation)                │
│  ▸ REST Controllers                                          │
│  ▸ SignalR Hubs (Real-time)                                 │
│  ▸ Authentication/Authorization Middleware                   │
│  ▸ Rate Limiting & Security                                  │
└──────────────────────┬─────────────────────────────────────┘
                       │
┌──────────────────────▼─────────────────────────────────────┐
│              Infrastructure Layer                            │
│  ▸ Entity Framework Core (PostgreSQL)                       │
│  ▸ ElasticSearch Integration                                │
│  ▸ Redis Caching                                            │
│  ▸ Azure Storage / File System                              │
│  ▸ External Services (Email, SSO, AI)                       │
└──────────────────────┬─────────────────────────────────────┘
                       │
┌──────────────────────▼─────────────────────────────────────┐
│              Application Layer (CQRS)                        │
│  ▸ MediatR Commands & Queries                               │
│  ▸ FluentValidation                                         │
│  ▸ AutoMapper DTOs                                          │
│  ▸ Business Logic & Use Cases                               │
└──────────────────────┬─────────────────────────────────────┘
                       │
┌──────────────────────▼─────────────────────────────────────┐
│                  Domain Layer (Core)                         │
│  ▸ Entities                                                  │
│  ▸ Value Objects                                             │
│  ▸ Domain Events                                             │
│  ▸ Repository Interfaces                                     │
│  ▸ Business Rules                                            │
└────────────────────────────────────────────────────────────┘
```

## ✨ Core Features

### 1. **User Management & Authentication**
- Corporate SSO integration (SAML 2.0 / OAuth 2.0)
- Email-based authentication fallback
- Role-based access control (User, Moderator, Admin)
- Comprehensive user profiles with department and skills
- Reputation system and contribution tracking

### 2. **Question & Answer System**
- Rich text editor with Markdown + code blocks
- Tagging system with auto-suggestions
- Vote system (upvote/downvote)
- Accept answer functionality
- Comments on questions and answers
- Edit history with diff tracking
- Bookmark functionality

### 3. **Advanced Search**
- Full-text search powered by ElasticSearch
- Filter by tags, department, status, date
- Search by answered/unanswered
- Sort by relevance, votes, date, activity
- Real-time search suggestions

### 4. **Knowledge Intelligence (AI-Powered)**
- Auto-suggest similar questions while typing
- Duplicate question detection
- Answer summarization for long responses
- Expert recommendation based on expertise
- Spam detection
- Auto-tag suggestions

### 5. **Moderation & Quality Control**
- Flag system (spam, offensive, low quality)
- Review queue for moderators
- Merge duplicate questions
- Tag management and synonyms
- Question closing/reopening
- Content locking

### 6. **Reputation & Gamification**
- **Points System:**
  - Question upvoted: +5
  - Answer upvoted: +10
  - Answer accepted: +15
  - Accept an answer: +2
  - Edit approved: +2
- **Badges:**
  - Bronze, Silver, Gold badges
  - Categories: Participation, Expertise, Moderation, Contribution
  - Domain-specific badges (Network Guru, Automation Master, etc.)
- **Leaderboards:**
  - Overall, monthly, department-wise
  - Domain expertise rankings

### 7. **Enterprise Features**
- **Audit Logs:** Complete activity tracking for compliance
- **Analytics Dashboard:**
  - Most common issues
  - Knowledge gaps analysis
  - Response time metrics
  - User engagement statistics
  - Department performance
- **Export Capabilities:** PDF, Confluence integration
- **Attachments:** Support for logs, configs, screenshots (up to 50MB)
- **Versioned Answers:** Track changes for evolving solutions

### 8. **Security (Production-Grade)**
- JWT-based authentication with refresh tokens
- Role-based authorization (RBAC)
- XSS and CSRF protection
- SQL injection prevention (parameterized queries)
- Rate limiting (IP-based and user-based)
- Audit trails for all critical actions
- Internal-only access controls
- Account lockout after failed attempts
- Password complexity requirements

### 9. **Real-Time Features (SignalR)**
- Live notifications
- Real-time vote updates
- New answer notifications
- Comment updates
- Badge earned notifications

## 🛠️ Technology Stack

### Backend
- **Framework:** .NET 8.0
- **Architecture:** Clean Architecture + CQRS (MediatR)
- **ORM:** Entity Framework Core 8.0
- **Database:** PostgreSQL 15+
- **Search:** ElasticSearch / OpenSearch
- **Cache:** Redis
- **Real-time:** SignalR
- **Validation:** FluentValidation
- **Mapping:** AutoMapper
- **Logging:** Serilog
- **API Documentation:** Swagger/OpenAPI

### Frontend
- **Framework:** Angular 17+
- **UI Components:** Angular Material / PrimeNG
- **State Management:** NgRx
- **Forms:** Reactive Forms
- **HTTP:** HttpClient with interceptors
- **Rich Text:** ngx-markdown, ngx-monaco-editor
- **Dark/Light Theme:** Custom theming
- **Responsive:** Mobile-first design

### Infrastructure
- **Containerization:** Docker
- **Orchestration:** Kubernetes
- **CI/CD:** GitHub Actions
- **Storage:** Azure Blob Storage / Local File System
- **Monitoring:** Application Insights / ELK Stack

## 📁 Project Structure

```
Deep Overflow/
├── backend/
│   ├── src/
│   │   ├── DeepOverflow.Domain/           # Core business logic
│   │   │   ├── Entities/
│   │   │   ├── Enums/
│   │   │   ├── Interfaces/
│   │   │   └── Common/
│   │   ├── DeepOverflow.Application/      # Use cases (CQRS)
│   │   │   ├── Questions/
│   │   │   ├── Answers/
│   │   │   ├── Users/
│   │   │   ├── Common/
│   │   │   └── DTOs/
│   │   ├── DeepOverflow.Infrastructure/   # External concerns
│   │   │   ├── Persistence/
│   │   │   ├── Identity/
│   │   │   ├── Search/
│   │   │   ├── Cache/
│   │   │   └── Services/
│   │   └── DeepOverflow.API/              # Web API
│   │       ├── Controllers/
│   │       ├── Hubs/
│   │       ├── Middleware/
│   │       └── Program.cs
│   └── tests/
├── frontend/
│   └── src/
│       ├── app/
│       │   ├── core/                       # Singletons
│       │   ├── shared/                     # Shared components
│       │   ├── features/                   # Feature modules
│       │   │   ├── questions/
│       │   │   ├── answers/
│       │   │   ├── users/
│       │   │   ├── tags/
│       │   │   ├── search/
│       │   │   └── admin/
│       │   └── layout/
│       ├── assets/
│       └── environments/
├── database/
│   └── schema.sql                          # PostgreSQL schema
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.frontend
│   └── docker-compose.yml
└── k8s/                                     # Kubernetes manifests
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm
- PostgreSQL 15+
- Redis 7+
- ElasticSearch 8+ / OpenSearch 2+
- Docker & Docker Compose (optional)

### Database Setup

1. Create PostgreSQL database:
```bash
createdb deepoverflow
```

2. Run database schema:
```bash
psql -d deepoverflow -f database/schema.sql
```

### Backend Setup

1. Navigate to backend directory:
```bash
cd backend
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Update `appsettings.json` with your connection strings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=deepoverflow;Username=postgres;Password=yourpassword"
  },
  "ElasticSearch": {
    "Uri": "http://localhost:9200"
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "JWT": {
    "Secret": "your-super-secret-key-min-32-chars",
    "Issuer": "DeepOverflow",
    "Audience": "DeepOverflowUsers",
    "ExpiryMinutes": 60
  }
}
```

4. Run migrations (if using EF migrations):
```bash
cd src/DeepOverflow.API
dotnet ef database update
```

5. Run the API:
```bash
dotnet run
```

API will be available at `https://localhost:5001`

### Frontend Setup

1. Navigate to frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Update environment configuration in `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  signalRUrl: 'https://localhost:5001/hubs'
};
```

4. Run development server:
```bash
npm start
```

Frontend will be available at `http://localhost:4200`

### Docker Setup (Recommended)

Run entire stack with Docker Compose:

```bash
docker-compose up -d
```

This will start:
- PostgreSQL
- Redis
- ElasticSearch
- Backend API
- Frontend
- Nginx reverse proxy

## 🔐 Authentication & Authorization

### JWT Authentication Flow
1. User logs in with credentials or SSO
2. Backend validates and returns JWT access token + refresh token
3. Frontend stores tokens in HttpOnly cookies (secure)
4. All API requests include Authorization header
5. Refresh token used to obtain new access token

### Roles & Permissions

| Role | Permissions |
|------|-------------|
| **User** | Ask/answer questions, comment, vote, edit own content |
| **Moderator** | + Close questions, handle flags, edit any content, tag management |
| **Admin** | + User management, badge creation, analytics access, system config |

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - Login with credentials
- `POST /api/auth/sso` - SSO authentication
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout
- `POST /api/auth/register` - Register new user (if enabled)

### Questions
- `GET /api/questions` - List questions (paginated, filterable)
- `GET /api/questions/{id}` - Get question details
- `GET /api/questions/slug/{slug}` - Get question by slug
- `POST /api/questions` - Create question [Auth]
- `PUT /api/questions/{id}` - Update question [Auth, Owner]
- `DELETE /api/questions/{id}` - Delete question [Auth, Owner/Mod]
- `POST /api/questions/{id}/close` - Close question [Moderator]
- `POST /api/questions/{id}/reopen` - Reopen question [Moderator]
- `GET /api/questions/{id}/similar` - Get similar questions

### Answers
- `GET /api/questions/{questionId}/answers` - List answers
- `POST /api/questions/{questionId}/answers` - Create answer [Auth]
- `PUT /api/answers/{id}` - Update answer [Auth, Owner]
- `DELETE /api/answers/{id}` - Delete answer [Auth, Owner/Mod]
- `POST /api/answers/{id}/accept` - Accept answer [Auth, Question Owner]

### Voting
- `POST /api/vote/question/{id}` - Vote on question [Auth]
- `POST /api/vote/answer/{id}` - Vote on answer [Auth]
- `DELETE /api/vote/question/{id}` - Remove vote [Auth]

### Comments
- `GET /api/{type}/{id}/comments` - List comments
- `POST /api/{type}/{id}/comments` - Create comment [Auth]
- `PUT /api/comments/{id}` - Update comment [Auth, Owner]
- `DELETE /api/comments/{id}` - Delete comment [Auth, Owner/Mod]

### Search
- `GET /api/search?q={query}` - Full-text search
- `GET /api/search/suggestions?q={query}` - Search suggestions
- `GET /api/search/similar?title={title}&body={body}` - Find similar

### Users
- `GET /api/users/{id}` - Get user profile
- `GET /api/users/leaderboard` - Get leaderboard
- `PUT /api/users/profile` - Update own profile [Auth]
- `GET /api/users/{id}/questions` - User's questions
- `GET /api/users/{id}/answers` - User's answers

### Tags
- `GET /api/tags` - List all tags
- `GET /api/tags/popular` - Popular tags
- `GET /api/tags/{name}` - Tag details
- `POST /api/tags` - Create tag [Auth]

### Moderation (Moderator+)
- `GET /api/moderation/flags` - Review queue
- `POST /api/moderation/flags/{id}/resolve` - Resolve flag
- `POST /api/questions/{id}/merge` - Merge duplicates

### Analytics (Admin)
- `GET /api/analytics/overview` - System overview
- `GET /api/analytics/knowledge-gaps` - Knowledge gaps
- `GET /api/analytics/response-times` - Response metrics

## 🎨 Frontend Features

### Responsive Design
- Mobile-first approach
- Breakpoints: mobile (<768px), tablet (768-1024px), desktop (>1024px)
- Touch-friendly controls
- Progressive Web App (PWA) ready

### Dark/Light Mode
- User preference stored in localStorage
- System preference detection
- Smooth theme transitions

### Rich Text Editor
- Markdown support with preview
- Code highlighting (supports 100+ languages)
- Image upload and embedding
- File attachment
- @mention autocomplete

### Real-Time Updates
- WebSocket connection via SignalR
- Live notifications
- Real-time vote counts
- New answer alerts

## 🧪 Testing

### Backend Tests
```bash
cd backend/tests
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test              # Unit tests
npm run e2e           # E2E tests
```

## 📦 Deployment

### Docker Deployment
```bash
docker build -t deepoverflow-api -f docker/Dockerfile.api .
docker build -t deepoverflow-frontend -f docker/Dockerfile.frontend .
docker-compose -f docker-compose.prod.yml up -d
```

### Kubernetes Deployment
```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/configmaps.yaml
kubectl apply -f k8s/postgres.yaml
kubectl apply -f k8s/redis.yaml
kubectl apply -f k8s/elasticsearch.yaml
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/frontend-deployment.yaml
kubectl apply -f k8s/ingress.yaml
```

## 🔧 Configuration

### Environment Variables

**Backend:**
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Staging/Production)
- `DATABASE_CONNECTION` - PostgreSQL connection string
- `REDIS_CONNECTION` - Redis connection string
- `ELASTICSEARCH_URI` - ElasticSearch endpoint
- `JWT_SECRET` - JWT signing key
- `STORAGE_TYPE` - Storage provider (Local/Azure/AWS)
- `AI_SERVICE_ENDPOINT` - AI service endpoint for intelligent features

**Frontend:**
- `API_URL` - Backend API URL
- `ENVIRONMENT` - Environment name

## 📈 Performance Optimizations

1. **Caching Strategy:**
   - Redis caching for frequently accessed data
   - Browser caching with proper headers
   - CDN for static assets

2. **Database Optimizations:**
   - Proper indexing on frequently queried columns
   - Connection pooling
   - Read replicas for analytics queries

3. **Search Optimization:**
   - ElasticSearch for sub-second full-text search
   - Autocomplete with edge n-grams
   - Faceted search for filtering

4. **Frontend Optimizations:**
   - Lazy loading of feature modules
   - Virtual scrolling for long lists
   - Image optimization and lazy loading
   - Service worker for offline support

## 🔒 Security Measures

- HTTPS enforcement
- CORS configuration
- Rate limiting (100 requests/minute per IP)
- SQL injection prevention
- XSS protection
- CSRF tokens
- Content Security Policy (CSP)
- Input validation and sanitization
- Password hashing with bcrypt
- Account lockout mechanism
- Audit logging

## 📝 License

Proprietary - Internal use by RMES only

## 👥 Support

For issues, questions, or feature requests:
- Internal Slack: #deep-overflow
- Email: deepoverflow-support@rmes.com
- Documentation: https://docs.deepoverflow.rmes.internal

---

**Built with ❤️ for RMES Engineering Team**
