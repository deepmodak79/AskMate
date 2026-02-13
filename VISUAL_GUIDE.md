# 🎨 Deep Overflow - Visual Walkthrough

## What You'll See When Running

### 1️⃣ **Swagger API Documentation** (http://localhost:5001/swagger)

```
┌─────────────────────────────────────────────────────────────┐
│  Deep Overflow API v1.0                                     │
│  Enterprise Stack Overflow for RMES                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  🔐 Authentication                                           │
│    POST /api/auth/login         Login with credentials      │
│    POST /api/auth/sso           SSO authentication          │
│    POST /api/auth/refresh       Refresh access token        │
│    POST /api/auth/register      Register new user           │
│                                                               │
│  ❓ Questions                                                │
│    GET    /api/questions        List all questions          │
│    GET    /api/questions/{id}   Get question details        │
│    POST   /api/questions        Create new question         │
│    PUT    /api/questions/{id}   Update question             │
│    DELETE /api/questions/{id}   Delete question             │
│    POST   /api/questions/{id}/vote    Vote on question      │
│    GET    /api/questions/{id}/similar Similar questions     │
│                                                               │
│  💬 Answers                                                  │
│    GET    /api/answers/{id}     Get answer                  │
│    POST   /api/answers          Create answer               │
│    PUT    /api/answers/{id}     Update answer               │
│    POST   /api/answers/{id}/accept  Accept answer           │
│    POST   /api/answers/{id}/vote    Vote on answer          │
│                                                               │
│  👥 Users                                                    │
│    GET    /api/users/{id}       Get user profile            │
│    GET    /api/users/leaderboard  Top users                 │
│    PUT    /api/users/profile    Update profile              │
│                                                               │
│  🏷️  Tags                                                    │
│    GET    /api/tags             List all tags               │
│    GET    /api/tags/popular     Popular tags                │
│    POST   /api/tags             Create new tag              │
│                                                               │
│  🔍 Search                                                   │
│    GET    /api/search           Full-text search            │
│    GET    /api/search/suggestions  Search suggestions       │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

### 2️⃣ **Frontend Application** (http://localhost:4200)

#### **Home Page / Question List**
```
┌────────────────────────────────────────────────────────────────────┐
│  🔷 Deep Overflow          Questions  Tags  Users        🌙 ⚙️ 👤  │
├────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  All Questions                              [Ask Question Button]  │
│                                                                     │
│  [Newest] [Active] [Unanswered] [Most Votes]                      │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │ [15 votes] [3 answers] [127 views]                           │ │
│  │                                                                │ │
│  │ How to configure PRP redundancy in IEC-61850?                │ │
│  │                                                                │ │
│  │ [iec-61850] [prp] [networking] [redundancy]                  │ │
│  │                                                                │ │
│  │ Asked by john_doe (2.5k rep) • 2 hours ago                   │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │ [8 votes] [1 answer] ✓ [45 views]                           │ │
│  │                                                                │ │
│  │ SCADA communication timeout troubleshooting                  │ │
│  │                                                                │ │
│  │ [scada] [modbus] [troubleshooting]                            │ │
│  │                                                                │ │
│  │ Asked by jane_smith (1.8k rep) • 5 hours ago                 │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │ [23 votes] [7 answers] ✓ [892 views]                        │ │
│  │                                                                │ │
│  │ Best practices for securing substation networks?             │ │
│  │                                                                │ │
│  │ [security] [networking] [substation] [firewalls]              │ │
│  │                                                                │ │
│  │ Asked by security_expert (5.2k rep) • 2 days ago             │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                     │
│  [Load More]                                                       │
│                                                                     │
└────────────────────────────────────────────────────────────────────┘
```

#### **Question Detail Page**
```
┌────────────────────────────────────────────────────────────────────┐
│  🔷 Deep Overflow          Questions  Tags  Users        🌙 ⚙️ 👤  │
├────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ▲                                                                  │
│  15    How to configure PRP redundancy in IEC-61850?              │
│  ▼                                                                  │
│                                                                     │
│  [Bookmark] [Share] [Flag]                      Asked 2 hours ago  │
│  Modified 1 hour ago   Viewed 127 times                            │
│                                                                     │
│  ─────────────────────────────────────────────────────────────     │
│                                                                     │
│  I'm working on implementing Parallel Redundancy Protocol (PRP)   │
│  in our IEC-61850 substation automation system. We have two       │
│  Ethernet networks and need to configure seamless redundancy.     │
│                                                                     │
│  Current setup:                                                    │
│  ```                                                                │
│  Network A: 192.168.1.0/24                                        │
│  Network B: 192.168.2.0/24                                        │
│  IEDs: Siemens 7SA86, ABB RET615                                 │
│  ```                                                                │
│                                                                     │
│  Questions:                                                        │
│  1. How do I configure the duplicate detection?                   │
│  2. What VLAN tagging scheme should I use?                        │
│  3. Any gotchas with multicast in PRP mode?                       │
│                                                                     │
│  [iec-61850] [prp] [networking] [redundancy] [siemens] [abb]     │
│                                                                     │
│  ─────────────────────────────────────────────────────────────     │
│                                                                     │
│  💬 Add a comment                                                  │
│                                                                     │
│  ───────────────────────────────────────────────────────────────   │
│                                                                     │
│  3 Answers                          [Sorted by: Votes ▼]          │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │ ▲                                                    ✓        │  │
│  │ 12  [Answer from network_guru • 1.5 hours ago]              │  │
│  │ ▼                                                             │  │
│  │                                                               │  │
│  │ I've implemented PRP across 15+ substations. Here's the     │  │
│  │ step-by-step process:                                        │  │
│  │                                                               │  │
│  │ ## Configuration Steps                                       │  │
│  │                                                               │  │
│  │ 1. **Enable PRP on IEDs**                                   │  │
│  │    - Go to Communication settings                            │  │
│  │    - Enable "Redundancy Protocol"                            │  │
│  │    - Select "PRP" mode                                       │  │
│  │                                                               │  │
│  │ 2. **Configure Network Interfaces**                          │  │
│  │    ```                                                        │  │
│  │    LAN A: 192.168.1.10                                       │  │
│  │    LAN B: 192.168.2.10                                       │  │
│  │    ```                                                        │  │
│  │                                                               │  │
│  │ [Full detailed answer with code examples...]                 │  │
│  │                                                               │  │
│  │ 💬 3 comments     [Add comment]       [Edit] [Share] [Flag] │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  [Your Answer]                                                     │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │ [Rich Text Editor]                                           │  │
│  │                                                               │  │
│  │ [B] [I] [Code] [Link] [Image] [Markdown Preview]            │  │
│  │                                                               │  │
│  └─────────────────────────────────────────────────────────────┘  │
│  [Post Your Answer]                                                │
│                                                                     │
└────────────────────────────────────────────────────────────────────┘
```

#### **User Profile Page**
```
┌────────────────────────────────────────────────────────────────────┐
│  🔷 Deep Overflow          Questions  Tags  Users        🌙 ⚙️ 👤  │
├────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌────────┐                                                         │
│  │  [👤]  │   network_guru                                         │
│  │        │   Network & Automation Specialist                      │
│  │  5,234 │   Engineering Department                               │
│  │  rep   │   Member for 2 years                                   │
│  └────────┘                                                         │
│                                                                     │
│  🥇 Gold: 5    🥈 Silver: 23    🥉 Bronze: 47                      │
│                                                                     │
│  [Network Guru] [Automation Master] [Great Answer] [Deputy]        │
│                                                                     │
│  ─────────────────────────────────────────────────────────────     │
│                                                                     │
│  Stats                                                             │
│  • 127 questions asked                                             │
│  • 342 answers given                                               │
│  • 89 accepted answers (26% acceptance rate)                       │
│  • 1,234 helpful votes                                             │
│                                                                     │
│  ─────────────────────────────────────────────────────────────     │
│                                                                     │
│  Top Tags                                                          │
│  [iec-61850 × 89] [networking × 67] [prp × 45] [scada × 34]      │
│                                                                     │
│  ─────────────────────────────────────────────────────────────     │
│                                                                     │
│  Recent Activity                                                   │
│  • Answered: How to configure PRP redundancy...  (2 hours ago)    │
│  • Asked: Best VLAN configuration for...         (1 day ago)      │
│  • Earned badge: Network Guru                    (2 days ago)     │
│                                                                     │
└────────────────────────────────────────────────────────────────────┘
```

---

### 3️⃣ **Database Tables** (When Connected)

```sql
deepoverflow=# \dt

                  List of relations
 Schema |        Name         | Type  |  Owner   
--------+---------------------+-------+----------
 public | answers             | table | postgres
 public | audit_logs          | table | postgres
 public | badge_definitions   | table | postgres
 public | bookmarks           | table | postgres
 public | comments            | table | postgres
 public | daily_stats         | table | postgres
 public | edit_history        | table | postgres
 public | flags               | table | postgres
 public | notifications       | table | postgres
 public | question_tags       | table | postgres
 public | questions           | table | postgres
 public | reputation_history  | table | postgres
 public | saved_searches      | table | postgres
 public | tag_synonyms        | table | postgres
 public | tags                | table | postgres
 public | user_badges         | table | postgres
 public | users               | table | postgres
(17 rows)

deepoverflow=# SELECT COUNT(*) FROM badge_definitions;
 count 
-------
    17
(1 row)

deepoverflow=# SELECT name, badge_type, category FROM badge_definitions LIMIT 5;
      name       | badge_type |  category    
-----------------+------------+--------------
 Welcome         | Bronze     | Participation
 Student         | Bronze     | Participation
 Teacher         | Bronze     | Participation
 Network Guru    | Silver     | Expertise
 Automation Master| Silver    | Expertise
(5 rows)
```

---

### 4️⃣ **Docker Services Running**

```powershell
PS> docker compose ps

NAME                        STATUS              PORTS
deepoverflow-postgres       Up (healthy)        0.0.0.0:5432->5432/tcp
deepoverflow-redis          Up (healthy)        0.0.0.0:6379->6379/tcp
deepoverflow-elasticsearch  Up (healthy)        0.0.0.0:9200->9200/tcp
deepoverflow-api            Up (healthy)        0.0.0.0:5001->80/tcp
deepoverflow-frontend       Up                  0.0.0.0:4200->80/tcp
deepoverflow-nginx          Up                  0.0.0.0:80->80/tcp, 443/tcp
```

---

### 5️⃣ **Real-Time Features** (SignalR WebSocket)

When you open the application:

```
Browser → WebSocket Connection
  ↓
  ws://localhost:5001/hubs/notifications
  ↓
✅ Connected to notification hub
  ↓
🔔 New answer received!
🔔 Your question was upvoted!
🔔 Badge earned: Network Guru!
🔔 Someone commented on your answer
```

---

### 6️⃣ **Health Check Response**

```bash
curl http://localhost:5001/health
```

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": "PostgreSQL connection is healthy",
      "duration": "00:00:00.0456789"
    },
    "redis": {
      "status": "Healthy",
      "description": "Redis cache is responding",
      "duration": "00:00:00.0123456"
    }
  }
}
```

---

### 7️⃣ **Sample API Request**

**Create a Question:**

```bash
POST http://localhost:5001/api/questions
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

{
  "title": "How to configure Modbus TCP in SCADA system?",
  "body": "I need help configuring Modbus TCP communication...",
  "tags": ["scada", "modbus", "tcp"]
}
```

**Response:**

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "slug": "how-to-configure-modbus-tcp-in-scada-system",
  "title": "How to configure Modbus TCP in SCADA system?",
  "createdAt": "2026-01-31T10:30:00Z"
}
```

---

## 🎨 Theme Support

### Light Theme
```
Background: #FFFFFF
Text: #212529
Primary: #007BFF
Borders: #DEE2E6
```

### Dark Theme (Toggle with 🌙)
```
Background: #1A1A1A
Text: #E0E0E0
Primary: #4A9EFF
Borders: #333333
```

---

## 📊 What's Happening Behind the Scenes

```
User Action → Frontend (Angular)
                ↓
           HTTP Request
                ↓
        API Controller
                ↓
          MediatR Handler
                ↓
     Domain Business Logic
                ↓
         Repository Layer
                ↓
    Entity Framework Core
                ↓
        PostgreSQL Database
                ↓
     Result ← All the way back
                ↓
        JSON Response
                ↓
     Angular Component
                ↓
       Updated UI

Meanwhile:
- SignalR sends real-time updates
- ElasticSearch indexes the content
- Redis caches frequently accessed data
- Audit logs record the action
```

---

**This is what you'll see when you run `docker compose up -d` and visit the URLs!** 🚀
