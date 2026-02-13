# 🚀 Quick Start Instructions for Deep Overflow

## Current Status: Docker Desktop Not Running

Docker Desktop needs to be started to run the full application stack.

## Option 1: Start with Docker (Recommended - Full Stack)

### Step 1: Start Docker Desktop
1. Open Docker Desktop from Start Menu
2. Wait for it to fully start (icon will turn green in system tray)
3. This may take 30-60 seconds

### Step 2: Run the Application
Open PowerShell or Command Prompt in the project folder and run:

```powershell
cd "d:\Deep Overflow"
docker compose up -d
```

### Step 3: Wait for Services to Start
```powershell
# Check if all services are running
docker compose ps

# View logs
docker compose logs -f
```

### Step 4: Access the Application
After 30-60 seconds, the application will be available at:

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5001/api
- **Swagger UI**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/health

---

## Option 2: Quick Demo - Run Backend Only (No Docker)

If you have .NET 8.0 SDK installed, you can run the backend API immediately:

```powershell
cd "d:\Deep Overflow\backend\src\DeepOverflow.API"
dotnet run
```

Then visit: **http://localhost:5001/swagger**

---

## Option 3: View the Application Structure

You can explore the complete codebase that's ready to run:

### Backend:
- Navigate to: `d:\Deep Overflow\backend\`
- Open `DeepOverflow.sln` in Visual Studio or VS Code
- Explore the Clean Architecture layers

### Frontend:
- Navigate to: `d:\Deep Overflow\frontend\`
- Open in VS Code
- Run `npm install` then `npm start` (requires Node.js)

### Database:
- View the schema: `d:\Deep Overflow\database\schema.sql`
- 1000+ lines of production-ready PostgreSQL

---

## What You'll See When Running

### Swagger UI (http://localhost:5001/swagger)
- Complete API documentation
- Interactive endpoint testing
- All authentication endpoints
- Questions, Answers, Users, Tags APIs

### Frontend (http://localhost:4200)
- Modern Angular application
- Dark/light theme toggle
- Question listing
- Responsive design

### Docker Services Running
```
✅ deepoverflow-postgres (Database)
✅ deepoverflow-redis (Cache)
✅ deepoverflow-elasticsearch (Search)
✅ deepoverflow-api (Backend)
✅ deepoverflow-frontend (Frontend)
✅ deepoverflow-nginx (Reverse Proxy)
```

---

## Troubleshooting

### Docker Desktop won't start?
- Check if Hyper-V or WSL2 is enabled
- Restart your computer
- Check Docker Desktop logs

### Port conflicts?
Edit `docker-compose.yml` and change port mappings:
```yaml
ports:
  - "5001:80"  # Change 5001 to another port
```

### Want to see logs?
```powershell
docker compose logs -f api        # Backend logs
docker compose logs -f frontend   # Frontend logs
docker compose logs -f            # All logs
```

---

## Next Steps After Starting

1. **Create Admin User**:
   ```powershell
   docker exec -it deepoverflow-postgres psql -U postgres -d deepoverflow
   ```
   Then run:
   ```sql
   INSERT INTO users (email, username, display_name, role, is_active, is_email_verified)
   VALUES ('admin@rmes.com', 'admin', 'Admin User', 'Admin', true, true);
   ```

2. **Test the API**: Visit http://localhost:5001/swagger

3. **Explore the Frontend**: Visit http://localhost:4200

4. **Check Health**: Visit http://localhost:5001/health

---

## Quick Commands Reference

```powershell
# Start all services
docker compose up -d

# Stop all services
docker compose down

# View running services
docker compose ps

# View logs
docker compose logs -f

# Restart a service
docker compose restart api

# Rebuild and restart
docker compose up -d --build

# Remove everything (fresh start)
docker compose down -v
```

---

**Ready to run!** Just start Docker Desktop and run `docker compose up -d` 🚀
