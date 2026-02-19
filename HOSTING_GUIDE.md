# Complete Hosting Guide – Deep Overflow

**Learn how to host your app so you can do it yourself in the future.**

---

## Table of Contents

1. [Understanding Your App Structure](#1-understanding-your-app-structure)
2. [Why Two Separate Hosting Services?](#2-why-two-separate-hosting-services)
3. [Hosting Options Overview](#3-hosting-options-overview)
4. [Option A: GitHub Pages + Render (Recommended, Free)](#4-option-a-github-pages--render-recommended-free)
5. [Option B: Vercel + Render](#5-option-b-vercel--render)
6. [Option C: Netlify + Railway](#6-option-c-netlify--railway)
7. [Option D: Azure (Full Microsoft Stack)](#7-option-d-azure-full-microsoft-stack)
8. [Database Hosting](#8-database-hosting)
9. [Step-by-Step: First-Time Setup Checklist](#9-step-by-step-first-time-setup-checklist)
10. [Troubleshooting Common Issues](#10-troubleshooting-common-issues)
11. [Quick Reference: What to Change When Hosting](#11-quick-reference-what-to-change-when-hosting)

---

## 1. Understanding Your App Structure

Your Deep Overflow app has **two parts**:

| Part | Technology | What It Does | Where the Code Lives |
|------|------------|--------------|----------------------|
| **Frontend** | Angular (HTML/CSS/JavaScript) | The UI users see in the browser | `frontend/` folder |
| **Backend** | .NET Web API (C#) | Handles data, auth, business logic | `backend/` folder |

When a user opens your app:
1. The **frontend** loads in their browser (from a static hosting service).
2. The **frontend** calls the **backend** API (e.g. `GET /api/questions`) to get data.
3. The **backend** talks to the **database** and returns JSON.
4. The **frontend** displays that data.

So you need to host:
- **Frontend** → somewhere that serves static files (HTML, JS, CSS)
- **Backend** → somewhere that runs a .NET server 24/7
- **Database** → somewhere that stores data (optional: SQLite file on the backend server, or a separate DB service)

---

## 2. Why Two Separate Hosting Services?

- **Frontend**: Built into static files. Services like GitHub Pages, Vercel, Netlify are built for this and are often free.
- **Backend**: Must run a .NET process. Needs a server (Render, Railway, Azure, etc.).

You *could* host both on one server (e.g. a VPS), but using separate services is simpler and often free for small projects.

---

## 3. Hosting Options Overview

| Frontend Hosting | Backend Hosting | Cost | Difficulty |
|------------------|-----------------|------|------------|
| GitHub Pages | Render | Free | Easy |
| Vercel | Render | Free | Easy |
| Netlify | Railway | Free tier | Easy |
| Azure Static Web Apps | Azure App Service | Free tier | Medium |
| Your own VPS (DigitalOcean, etc.) | Same VPS | ~$5–10/mo | Harder |

---

## 4. Option A: GitHub Pages + Render (Recommended, Free)

### Part 1: Host the Frontend on GitHub Pages

**What is GitHub Pages?**  
GitHub can serve your built frontend from a URL like `https://yourusername.github.io/YourRepoName/`.

**Step 1: Build your frontend locally (to verify it works)**

```bash
cd frontend
npm install
npm run build:prod
```

This creates `frontend/dist/deep-overflow/browser/` with the built files.

**Step 2: Create a GitHub Actions workflow**

Create `.github/workflows/deploy-pages.yml` in your repo:

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [main]

permissions:
  contents: read
  pages: write
  id-token: write

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '18.x'
          cache: 'npm'
          cache-dependency-path: ./frontend/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: ./frontend

      - name: Build
        run: npx ng build --configuration production --base-href /AskMate/
        working-directory: ./frontend

      - name: Setup Pages
        uses: actions/configure-pages@v4

      - name: Upload artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: ./frontend/dist/deep-overflow/browser

      - name: Deploy to GitHub Pages
        uses: actions/deploy-pages@v4
```

**Important:** Replace `/AskMate/` with `/<YourRepoName>/` if your repo has a different name.

**Step 3: Enable GitHub Pages**

1. Go to your repo on GitHub.
2. **Settings** → **Pages**.
3. Under **Build and deployment** → **Source**, select **GitHub Actions**.
4. Save.

**Step 4: Push your code**

```bash
git add .
git commit -m "Add GitHub Pages deployment"
git push origin main
```

GitHub Actions will run, build the frontend, and deploy it.  
Your site will be at: `https://<your-github-username>.github.io/<repo-name>/`

---

### Part 2: Host the Backend on Render

**What is Render?**  
Render runs your .NET API as a web service and gives you a public URL.

**Step 1: Create a Dockerfile for the backend**

Create `backend/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/DeepOverflow.API/DeepOverflow.API.csproj", "DeepOverflow.API/"]
COPY ["src/DeepOverflow.Application/DeepOverflow.Application.csproj", "DeepOverflow.Application/"]
COPY ["src/DeepOverflow.Domain/DeepOverflow.Domain.csproj", "DeepOverflow.Domain/"]
COPY ["src/DeepOverflow.Infrastructure/DeepOverflow.Infrastructure.csproj", "DeepOverflow.Infrastructure/"]

RUN dotnet restore "DeepOverflow.API/DeepOverflow.API.csproj"

COPY src/ .
WORKDIR "/src/DeepOverflow.API"
RUN dotnet build "DeepOverflow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeepOverflow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DeepOverflow.API.dll"]
```

**Step 2: Sign up and create a Web Service on Render**

1. Go to **https://render.com** and sign up (GitHub login works).
2. Click **New** → **Web Service**.
3. Connect your GitHub account and select your repo.
4. Configure:
   - **Name**: e.g. `askmate-api`
   - **Region**: Choose nearest to you
   - **Branch**: `main`
   - **Root Directory**: leave empty
   - **Runtime**: **Docker**
   - **Dockerfile Path**: `./backend/Dockerfile`
   - **Docker Context**: `./backend`
5. **Instance Type**: **Free**
6. Click **Create Web Service**.

Render will build and deploy. After a few minutes you’ll get a URL like:  
`https://askmate-api.onrender.com`

**Step 3: Point your frontend to the backend**

Edit `frontend/src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://askmate-api.onrender.com/api',
  signalRUrl: 'https://askmate-api.onrender.com/hubs',
  // ... rest
};
```

Replace `askmate-api` with your actual Render service name if different.

**Step 4: Allow your frontend URL in CORS**

Edit `backend/src/DeepOverflow.API/appsettings.json`:

```json
"CORS": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://yourusername.github.io"
  ],
  ...
}
```

Add your GitHub Pages URL (e.g. `https://deepmodak79.github.io`).

**Step 5: Push and redeploy**

```bash
git add .
git commit -m "Configure production API URL"
git push origin main
```

GitHub Actions will redeploy the frontend. Render will redeploy the backend if you have auto-deploy on.

---

## 5. Option B: Vercel + Render

**Vercel** is another popular frontend host (similar to GitHub Pages).

**Frontend on Vercel:**

1. Go to **https://vercel.com** and sign up with GitHub.
2. **Add New** → **Project**.
3. Import your repo.
4. Configure:
   - **Root Directory**: `frontend`
   - **Framework Preset**: Angular
   - **Build Command**: `npm run build:prod`
   - **Output Directory**: `dist/deep-overflow/browser`
5. Add **Environment Variable**: `API_URL` = `https://askmate-api.onrender.com/api` (if you use env vars).
6. Deploy.

**Backend:** Same as Option A (Render).

---

## 6. Option C: Netlify + Railway

**Netlify** for frontend, **Railway** for backend.

**Frontend on Netlify:**

1. Go to **https://netlify.com** and sign up.
2. **Add new site** → **Import an existing project**.
3. Connect GitHub and select your repo.
4. Configure:
   - **Base directory**: `frontend`
   - **Build command**: `npm run build:prod`
   - **Publish directory**: `frontend/dist/deep-overflow/browser`
5. Deploy.

**Backend on Railway:**

1. Go to **https://railway.app** and sign up.
2. **New Project** → **Deploy from GitHub repo**.
3. Select your repo.
4. Set **Root Directory** to `backend`.
5. Add a **Dockerfile** (same as Render) or use Railway’s .NET detection.
6. Deploy and copy the generated URL.

---

## 7. Option D: Azure (Full Microsoft Stack)

**Azure Static Web Apps** for frontend, **Azure App Service** for backend.

**Frontend:**

1. Azure Portal → **Static Web Apps** → **Create**.
2. Connect GitHub, select repo.
3. Build details:
   - **App location**: `/frontend`
   - **Output location**: `dist/deep-overflow/browser`
4. Create.

**Backend:**

1. Azure Portal → **App Service** → **Create**.
2. Runtime: **.NET 8**.
3. Connect to GitHub for deployment.
4. Set **Application settings** (connection strings, etc.).

---

## 8. Database Hosting

Your app can use **SQLite** (file-based) or **PostgreSQL**.

| Option | When to Use | Hosting |
|--------|-------------|---------|
| **SQLite** | Dev, demos, simple setups | File lives on the backend server (data lost on redeploy on free tiers) |
| **PostgreSQL** | Production, persistent data | Render PostgreSQL, Railway, Supabase, Neon, Azure Database |

**Render PostgreSQL (free tier):**

1. Render Dashboard → **New** → **PostgreSQL**.
2. Create the database.
3. Copy the **Internal Database URL**.
4. In your backend Web Service, add **Environment Variable**:
   - Key: `ConnectionStrings__DefaultConnection`
   - Value: (paste the URL)

**Supabase (free PostgreSQL):**

1. Go to **https://supabase.com**.
2. Create a project.
3. Copy the connection string from **Settings** → **Database**.
4. Use it as `ConnectionStrings__DefaultConnection` in your backend.

---

## 9. Step-by-Step: First-Time Setup Checklist

Use this as a checklist when hosting from scratch:

### Before You Start

- [ ] Code is pushed to GitHub
- [ ] `npm run build:prod` works in `frontend/`
- [ ] `dotnet run` works in `backend/src/DeepOverflow.API/`

### Frontend Hosting

- [ ] Create `.github/workflows/deploy-pages.yml` (or use Vercel/Netlify)
- [ ] Set `--base-href` to match your repo/URL
- [ ] Enable GitHub Pages (Source: GitHub Actions) or connect to Vercel/Netlify
- [ ] Push and wait for deployment
- [ ] Open the frontend URL and confirm the app loads

### Backend Hosting

- [ ] Create `backend/Dockerfile`
- [ ] Sign up on Render (or Railway/Azure)
- [ ] Create Web Service, connect repo
- [ ] Set Dockerfile path and context
- [ ] Add CORS origin for your frontend URL
- [ ] Deploy and test `/health` (or similar)

### Connect Frontend to Backend

- [ ] Update `environment.prod.ts` with backend URL
- [ ] Push to trigger frontend redeploy
- [ ] Test login, questions, answers

### Optional: Database

- [ ] Create PostgreSQL on Render/Supabase/Neon
- [ ] Add `ConnectionStrings__DefaultConnection` to backend env
- [ ] Run migrations if needed

---

## 10. Troubleshooting Common Issues

### Frontend blank or white screen

- **Cause**: Wrong `base-href`, wrong API URL, or CORS.
- **Fix**: Check browser console (F12). Ensure `base-href` matches your URL path. Ensure `apiUrl` in `environment.prod.ts` is correct. Ensure backend CORS includes your frontend URL.

### CORS errors in browser

- **Cause**: Backend does not allow your frontend origin.
- **Fix**: Add your frontend URL (e.g. `https://yourusername.github.io`) to `AllowedOrigins` in `appsettings.json` and redeploy the backend.

### API returns 404

- **Cause**: Wrong URL or backend not running.
- **Fix**: Test `https://your-backend-url/health`. If it fails, check Render/Railway logs. Ensure the backend is deployed and running.

### Login/Register fails

- **Cause**: Auth or database issues.
- **Fix**: Check backend logs. Ensure JWT secret is set. If using SQLite on Render, the DB file is ephemeral; consider PostgreSQL for production.

### Backend sleeps (Render free tier)

- **Cause**: Free tier spins down after inactivity.
- **Fix**: First request after sleep can take 30–60 seconds. For always-on, use a paid plan or another host.

---

## 11. Quick Reference: What to Change When Hosting

| What | Where | Example |
|------|-------|---------|
| Frontend base URL path | Build command `--base-href` | `/AskMate/` or `/` |
| Backend API URL | `frontend/src/environments/environment.prod.ts` | `apiUrl: 'https://xxx.onrender.com/api'` |
| CORS allowed origins | `backend/.../appsettings.json` | Add `https://yoursite.com` |
| Database connection | Backend environment variables | `ConnectionStrings__DefaultConnection` |
| JWT secret (production) | Backend environment variables | `JWT__Secret` |

---

## Summary

1. **Frontend** = static files → GitHub Pages, Vercel, or Netlify.
2. **Backend** = .NET API → Render, Railway, or Azure.
3. **Database** = SQLite (simple) or PostgreSQL (production).
4. **Connect them** by setting `apiUrl` in the frontend and CORS in the backend.
5. **Deploy** by pushing to GitHub; most platforms auto-deploy from the repo.

Use this guide whenever you need to host this or a similar app in the future.
