# Deep Overflow – Deployment Guide

## Overview

- **Frontend**: GitHub Pages (`https://deepmodak79.github.io/AskMate/`)
- **Backend**: Render.com (`https://askmate-api.onrender.com`)

---

## Step 1: Push Code to GitHub

Ensure all changes are pushed to your GitHub repo:

```bash
cd "c:\MIGRATE 2\Deep Overflow"
git add .
git commit -m "Add deployment config"
git push origin main
```

---

## Step 2: Enable GitHub Pages

1. Go to **https://github.com/deepmodak79/AskMate**
2. **Settings** → **Pages**
3. Under **Build and deployment**:
   - **Source**: GitHub Actions
4. Save

The `deploy-pages.yml` workflow will run on every push to `main` and deploy the frontend.

**Frontend URL**: `https://deepmodak79.github.io/AskMate/`

---

## Step 3: Deploy Backend to Render

1. Go to **https://dashboard.render.com**
2. Sign up / Log in (use GitHub)
3. **New** → **Web Service**
4. Connect repository: **deepmodak79/AskMate**
5. Configure:
   - **Name**: `askmate-api` (or keep default – the URL will match)
   - **Region**: Oregon (or nearest)
   - **Branch**: `main`
   - **Root Directory**: leave empty
   - **Runtime**: Docker
   - **Dockerfile Path**: `./backend/Dockerfile`
   - **Docker Context**: `./backend`
6. **Instance Type**: Free
7. Click **Create Web Service**

Render will build and deploy. After a few minutes you’ll get a URL like:

`https://askmate-api.onrender.com`

**Note**: On the free tier, the service may sleep after ~15 minutes of inactivity. The first request after sleep can take 30–60 seconds.

---

## Step 4: Verify Configuration

1. **Backend**: Open `https://askmate-api.onrender.com/health` – should return healthy.
2. **Frontend**: Open `https://deepmodak79.github.io/AskMate/` – should load the app.
3. **API**: The frontend is configured to use `https://askmate-api.onrender.com/api`.

If you used a different service name on Render, update:

- `frontend/src/environments/environment.prod.ts` → `apiUrl` and `signalRUrl`
- Push and let the GitHub Actions workflow redeploy the frontend.

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| Frontend shows blank page | Check browser console for CORS or API errors. Ensure backend CORS includes `https://deepmodak79.github.io`. |
| API returns 404 | Confirm backend URL and that `/health` works. |
| Login/Register fails | Check backend logs on Render. Ensure JWT and DB are configured. |
| Backend sleeps | Free tier sleeps after inactivity. First request after sleep will be slow. |

---

## URLs Summary

| Service | URL |
|---------|-----|
| **Frontend (GitHub Pages)** | https://deepmodak79.github.io/AskMate/ |
| **Backend API (Render)** | https://askmate-api.onrender.com |
| **API Health** | https://askmate-api.onrender.com/health |
| **Swagger (if enabled)** | https://askmate-api.onrender.com/swagger |
