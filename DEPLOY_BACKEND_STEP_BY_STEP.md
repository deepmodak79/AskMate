# Deploy Your Backend to Render – Step-by-Step Guide

**Your frontend is live at https://deepmodak79.github.io/AskMate/**  
**Now let's deploy the backend so the app works fully.**

---

## What You Will Achieve

By the end, your .NET API will be live at something like:

**https://askmate-api.onrender.com**

Then your frontend will be able to load questions, handle login, and work end-to-end.

---

## Prerequisites

- [ ] Frontend deployed (you've done this)
- [ ] GitHub account (same one you used)
- [ ] Your code pushed to GitHub

---

## Part 1: Create a Render Account

### Step 1.1: Go to Render

1. Open your browser
2. Go to **https://render.com**
3. Click **Get Started** or **Sign Up**

---

### Step 1.2: Sign Up with GitHub

1. Click **Sign up with GitHub**
2. Authorize Render to access your GitHub account
3. You may be asked to select which repos to allow – you can choose "All repositories" or just **AskMate**
4. Complete the sign-up

---

## Part 2: Create a New Web Service

### Step 2.1: Start Creating a Web Service

1. On the Render dashboard, click **New +** (top right)
2. Select **Web Service**

---

### Step 2.2: Connect Your Repository

1. You should see a list of your GitHub repositories
2. Find **AskMate** (or your repo name)
3. Click **Connect** next to it
4. If you don't see it, click **Configure account** and grant Render access to the repo

---

### Step 2.3: Configure the Service

Fill in these settings:

| Field | Value |
|-------|-------|
| **Name** | `askmate-api` (or any name – this becomes part of your URL) |
| **Region** | Oregon (US West) or choose nearest to you |
| **Branch** | `main` |
| **Root Directory** | Leave **empty** |
| **Runtime** | **Docker** |
| **Dockerfile Path** | `backend/Dockerfile` |
| **Docker Context** | `.` (repo root) or leave **empty** |

---

### Step 2.4: Instance Type

1. Under **Instance Type**, select **Free**
2. (Free tier may sleep after ~15 min of inactivity; first request after sleep can take 30–60 seconds)

---

### Step 2.5: Create the Service

1. Click **Create Web Service**
2. Render will start building your backend

---

## Part 3: Wait for the Build

### Step 3.1: Watch the Logs

1. You'll see a build log
2. It will:
   - Clone your repo
   - Build the Docker image
   - Start the container
3. This usually takes **5–10 minutes** the first time

---

### Step 3.2: Check for Success

- When the build succeeds, the log will show something like: **"Your service is live at https://askmate-api.onrender.com"**
- The status at the top will turn **green** (Live)

---

### Step 3.3: If the Build Fails

**Common issues:**

| Error | Fix |
|-------|-----|
| "Dockerfile not found" or "read src: is a directory" | **Dockerfile Path** must be exactly `backend/Dockerfile` (NOT `src` or `./src`). **Docker Context** must be `backend`. Fix these in Render Dashboard → your service → Settings. |
| "dotnet restore failed" | Check that your backend project structure is correct. The Dockerfile expects `backend/src/` with the .NET projects. |
| Build times out | Free tier has limits. Try again; sometimes it works on retry. |

---

## Part 4: Verify the Backend Is Working

### Step 4.1: Get Your Backend URL

At the top of your Render service page, you'll see something like:

**https://askmate-api.onrender.com**

Copy this URL. (Your actual URL may differ if you used a different service name.)

---

### Step 4.2: Test the Health Endpoint

1. Open a new browser tab
2. Go to: **https://askmate-api.onrender.com/health**
3. You should see a response (e.g. "Healthy" or similar)
4. If you get a response, the backend is running

---

### Step 4.3: Test the API

1. Go to: **https://askmate-api.onrender.com/api/questions**
2. You might see JSON (list of questions) or an empty array `[]`
3. Either way, if you get a response (not "Site can't be reached"), the API is working

**Note:** If the service was sleeping, the first request can take 30–60 seconds. Wait and try again.

---

## Part 5: Connect Frontend to Backend

### Step 5.1: Update the Frontend's API URL

Your frontend needs to know where the backend is.

1. Open your project in your editor
2. Go to: `frontend/src/environments/environment.prod.ts`
3. Update these lines with your **actual** Render URL:

```typescript
apiUrl: 'https://askmate-api.onrender.com/api',
signalRUrl: 'https://askmate-api.onrender.com/hubs',
```

Replace `askmate-api` with your actual service name if different (e.g. `askmate-api-xyz`).

---

### Step 5.2: Ensure CORS Allows Your Frontend

1. Open: `backend/src/DeepOverflow.API/appsettings.json`
2. Find the `CORS` section
3. Ensure `https://deepmodak79.github.io` is in `AllowedOrigins`:

```json
"AllowedOrigins": [
  "http://localhost:3000",
  "http://localhost:4200",
  "http://localhost:4201",
  "https://deepoverflow.rmes.com",
  "https://deepmodak79.github.io"
],
```

If it's not there, add it. Save the file.

---

### Step 5.3: Push and Redeploy

1. Commit and push your changes:

```
git add .
git commit -m "Configure production API URL for Render backend"
git push origin main
```

2. **Frontend:** GitHub Actions will automatically redeploy (2–5 min)
3. **Backend:** Render will auto-redeploy if you changed backend files. If you only changed frontend files, the backend stays as is.

---

## Part 6: Test the Full App

### Step 6.1: Open Your Frontend

1. Go to **https://deepmodak79.github.io/AskMate/**
2. Wait for it to load (if you just pushed, wait 2–3 min for the redeploy)

---

### Step 6.2: Test Key Features

1. **Questions list** – Should load (or show "No questions found")
2. **Register** – Create a new account
3. **Login** – Log in with that account
4. **Ask Question** – Create a question
5. **View Question** – Open a question and add an answer
6. **Vote** – Try voting on a question

If these work, your full stack is deployed.

---

## Part 7: If Something Doesn't Work

### Problem: Frontend shows "Failed to fetch" or network errors

**Cause:** CORS or wrong API URL.

**Fix:**
1. Confirm `https://deepmodak79.github.io` is in backend CORS
2. Confirm `environment.prod.ts` has the correct Render URL
3. Redeploy both (push to GitHub)
4. Clear browser cache or try incognito

---

### Problem: Backend returns 404 for /api/questions

**Cause:** Wrong URL or backend not running.

**Fix:**
1. Test `https://YOUR-BACKEND-URL/health` – does it respond?
2. Use `https://YOUR-BACKEND-URL/api/questions` (with `/api/`)
3. Check Render dashboard – is the service "Live"?

---

### Problem: Login/Register fails

**Cause:** Database or auth config.

**Fix:**
1. On Render free tier, SQLite is used; the DB file is ephemeral (resets on redeploy)
2. Check Render logs: Dashboard → your service → **Logs**
3. For persistent data, add a PostgreSQL database on Render (see optional section below)

---

### Problem: Posted questions disappear after login again

**Cause:** Render free tier uses **ephemeral SQLite**. The database is wiped when the service restarts or redeploys.

**Fix:** Add a **PostgreSQL database** on Render (see "Optional: Add a PostgreSQL Database" below). After adding it and setting `ConnectionStrings__DefaultConnection`, your questions, users, and votes will persist across restarts.

---

### Problem: Backend is very slow on first request

**Cause:** Render free tier spins down after ~15 min of inactivity.

**Fix:** This is normal. The first request after sleep can take 30–60 seconds. Later requests are fast.

---

## Optional: Add a PostgreSQL Database (For Persistent Data)

On the free tier, SQLite data is lost when the service redeploys. For persistent data:

### Step 1: Create a PostgreSQL Database on Render

1. Render Dashboard → **New +** → **PostgreSQL**
2. Name: `askmate-db`
3. Region: Same as your backend
4. Plan: **Free**
5. Click **Create Database**
6. Wait for it to be created

---

### Step 2: Get the Connection String

1. Open your new database
2. Under **Connection**, find **Internal Database URL**
3. Copy it (looks like: `postgresql://user:pass@host/dbname`)

---

### Step 3: Add to Backend Environment

1. Go to your **Web Service** (askmate-api)
2. **Environment** tab
3. Add:
   - **Key:** `ConnectionStrings__DefaultConnection`
   - **Value:** (paste the Internal Database URL)
4. Click **Save Changes**
5. Render will redeploy automatically

---

### Step 4: Schema Creation

The app creates the database schema automatically on startup (`EnsureCreated`). No migrations needed. After redeploy, your data will persist across restarts.

---

## Summary Checklist

- [ ] Render account created (GitHub sign-in)
- [ ] New Web Service created
- [ ] Repo connected (AskMate)
- [ ] Dockerfile path: `./backend/Dockerfile`
- [ ] Docker context: `./backend`
- [ ] Build succeeded (green status)
- [ ] `/health` returns OK
- [ ] `environment.prod.ts` updated with backend URL
- [ ] CORS includes `https://deepmodak79.github.io`
- [ ] Changes pushed to GitHub
- [ ] Full app tested at https://deepmodak79.github.io/AskMate/

---

## Your Deployed URLs

| Service | URL |
|---------|-----|
| **Frontend** | https://deepmodak79.github.io/AskMate/ |
| **Backend API** | https://askmate-api.onrender.com (or your actual URL) |
| **Health Check** | https://askmate-api.onrender.com/health |

---

**You're done.** Your Deep Overflow app is now fully deployed.
