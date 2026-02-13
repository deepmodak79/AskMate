# Deep Overflow - Quick Start Guide

Get Deep Overflow running in 5 minutes! ⚡

## 🚀 Fastest Way to Start (Docker Compose)

### Prerequisites
- Docker & Docker Compose installed
- 8GB RAM available
- Ports 80, 443, 5001, 4200, 5432, 6379, 9200 available

### 1. Clone & Configure

```bash
# Navigate to project directory
cd "d:\Deep Overflow"

# Create environment file
cp .env.template .env

# Edit .env and set at minimum:
# - DB_PASSWORD=YourSecurePassword123!
# - JWT_SECRET=YourSuperSecretKeyMin32Chars!
```

### 2. Start Everything

```bash
docker-compose up -d
```

This starts:
- ✅ PostgreSQL database
- ✅ Redis cache
- ✅ ElasticSearch
- ✅ .NET Backend API
- ✅ Angular Frontend
- ✅ Nginx reverse proxy

### 3. Access the Application

**Wait 30-60 seconds for services to initialize, then:**

| Service | URL | Credentials |
|---------|-----|-------------|
| **Frontend** | http://localhost:4200 | Register new user |
| **API (Swagger)** | http://localhost:5001/swagger | N/A |
| **API Direct** | http://localhost:5001/api | Bearer token |

### 4. Create First Admin User

```bash
# Connect to database
docker exec -it deepoverflow-postgres psql -U postgres -d deepoverflow

# Run SQL (change email/username)
INSERT INTO users (email, username, display_name, role, is_active, is_email_verified)
VALUES ('admin@rmes.com', 'admin', 'Admin User', 'Admin', true, true);

# Exit
\q
```

### 5. Verify Everything Works

**Health Check:**
```bash
curl http://localhost:5001/health
```

**Expected response:**
```json
{"status":"Healthy"}
```

**Check all services:**
```bash
docker-compose ps
```

All services should show "Up" status.

---

## 🛠️ Development Setup (No Docker)

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- PostgreSQL 15+
- Redis 7+
- ElasticSearch 8+

### 1. Database Setup

```bash
# Create database
createdb deepoverflow

# Apply schema
psql -d deepoverflow -f database/schema.sql
```

### 2. Backend Setup

```bash
cd backend/src/DeepOverflow.API

# Update appsettings.json with your connection strings

# Restore packages
dotnet restore

# Run API
dotnet run

# API will start at https://localhost:5001
```

### 3. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Update environment.ts if needed

# Start dev server
npm start

# Frontend will start at http://localhost:4200
```

---

## 📊 What You Get Out of the Box

### For Users
- ✅ Ask and answer questions
- ✅ Vote on content
- ✅ Comment on questions/answers
- ✅ Tag-based organization
- ✅ Full-text search
- ✅ Bookmark questions
- ✅ User profiles with reputation
- ✅ Badge system
- ✅ Real-time notifications

### For Moderators
- ✅ Review queue for flags
- ✅ Close/reopen questions
- ✅ Edit any content
- ✅ Tag management
- ✅ Merge duplicate questions

### For Admins
- ✅ User management
- ✅ Role assignment
- ✅ Analytics dashboard
- ✅ System configuration
- ✅ Badge creation
- ✅ Audit log access

---

## 🔍 Testing the Features

### 1. Ask a Question

```bash
curl -X POST http://localhost:5001/api/questions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "title": "How to configure PRP redundancy in IEC-61850?",
    "body": "I need help setting up Parallel Redundancy Protocol...",
    "tags": ["iec-61850", "prp", "networking"]
  }'
```

### 2. Search Questions

```bash
curl "http://localhost:5001/api/search?q=PRP+redundancy"
```

### 3. Vote on a Question

```bash
curl -X POST http://localhost:5001/api/questions/{id}/vote \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"voteType": "upvote"}'
```

---

## 🎨 UI Features

### Dark Mode
Click the theme toggle (🌙/☀️) in the header

### Rich Text Editor
- Markdown support
- Code syntax highlighting
- Image upload
- File attachments

### Real-time Updates
- Live notifications
- Vote count updates
- New answers appear automatically
- Comment updates

---

## 📚 Sample Data

### Create Sample Questions

```sql
-- Insert sample user
INSERT INTO users (id, email, username, display_name, role, reputation, is_active, is_email_verified)
VALUES 
  (gen_random_uuid(), 'john@rmes.com', 'john_doe', 'John Doe', 'User', 250, true, true);

-- Get the user ID
SELECT id FROM users WHERE username = 'john_doe';

-- Insert sample question (use the user ID from above)
INSERT INTO questions (id, title, slug, body, author_id, status)
VALUES (
  gen_random_uuid(),
  'How to troubleshoot SCADA communication issues?',
  'how-to-troubleshoot-scada-communication-issues',
  'We are experiencing intermittent communication drops with our SCADA system. What are the best debugging approaches?',
  '<USER_ID_HERE>',
  'Open'
);
```

---

## 🔧 Common Commands

### Docker Compose

```bash
# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f api

# Restart a service
docker-compose restart api

# Stop everything
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v

# Rebuild and restart
docker-compose up -d --build
```

### Backend

```bash
# Build
dotnet build

# Run tests
dotnet test

# Watch mode (auto-reload)
dotnet watch run

# Clean
dotnet clean
```

### Frontend

```bash
# Development server
npm start

# Build for production
npm run build:prod

# Run tests
npm test

# Lint code
npm run lint
```

---

## 🐛 Troubleshooting

### Port Already in Use

```bash
# Find process using port
netstat -ano | findstr :5001

# Kill process (Windows)
taskkill /PID <PID> /F

# Or change ports in docker-compose.yml
```

### Database Connection Failed

```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# View database logs
docker logs deepoverflow-postgres

# Connect to database manually
docker exec -it deepoverflow-postgres psql -U postgres -d deepoverflow
```

### Frontend Can't Reach API

1. Check API is running: http://localhost:5001/health
2. Check CORS settings in `appsettings.json`
3. Verify `environment.ts` has correct `apiUrl`

### Docker Compose Services Not Starting

```bash
# Check disk space
docker system df

# Prune unused resources
docker system prune -a

# View detailed logs
docker-compose logs --tail=100
```

---

## 📖 Next Steps

Once everything is running:

1. **Read the Full Documentation**
   - `README.md` - Complete feature overview
   - `PROJECT_STRUCTURE.md` - Architecture details
   - `DEPLOYMENT.md` - Production deployment

2. **Explore the API**
   - Visit http://localhost:5001/swagger
   - Try the interactive API documentation
   - Test endpoints with your token

3. **Customize Configuration**
   - Update branding in frontend
   - Configure SSO provider
   - Set up email notifications
   - Configure AI service

4. **Set Up Monitoring**
   - Configure application insights
   - Set up log aggregation
   - Add performance monitoring

5. **Deploy to Production**
   - Follow `DEPLOYMENT.md`
   - Set up Kubernetes cluster
   - Configure CI/CD pipeline
   - Enable SSL/TLS

---

## 💡 Pro Tips

### Performance
- Enable Redis caching for frequently accessed data
- Use ElasticSearch for all search operations
- Implement pagination for large result sets
- Use database indexes effectively

### Security
- Always use HTTPS in production
- Rotate JWT secrets regularly
- Enable rate limiting
- Keep dependencies updated
- Use secrets management (not .env files in production)

### Scalability
- Use horizontal pod autoscaling in Kubernetes
- Implement read replicas for PostgreSQL
- Use CDN for static assets
- Cache API responses with Redis

### Development
- Use hot reload for faster development
- Enable detailed error messages in development
- Use database migrations for schema changes
- Write tests for new features

---

## 🆘 Need Help?

| Issue Type | Contact |
|------------|---------|
| Bug Report | Create GitHub Issue |
| Feature Request | deepoverflow-features@rmes.com |
| Security Issue | security@rmes.com (private) |
| General Questions | #deep-overflow Slack channel |
| Documentation | https://docs.deepoverflow.rmes.internal |

---

## ✅ Checklist

Before considering deployment complete:

- [ ] All services start successfully
- [ ] Health check returns 200 OK
- [ ] Can create user account
- [ ] Can ask a question
- [ ] Can post an answer
- [ ] Can vote on content
- [ ] Search works
- [ ] Real-time notifications work
- [ ] Dark/light theme toggles
- [ ] API documentation accessible
- [ ] Database backup configured
- [ ] Monitoring set up
- [ ] SSL/TLS configured (production)

---

**Ready to build your knowledge base!** 🚀

For detailed information, see:
- 📘 `README.md` - Full documentation
- 🏗️ `PROJECT_STRUCTURE.md` - Architecture guide
- 🚢 `DEPLOYMENT.md` - Production deployment

---

*Built with ❤️ for RMES Engineering Team*
