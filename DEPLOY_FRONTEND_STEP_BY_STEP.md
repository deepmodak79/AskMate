# Deploy Your Frontend to GitHub – Step-by-Step Guide

**Assume you know nothing. Follow each step in order.**

---

## What You Will Achieve

By the end, your Angular frontend will be live at:

**https://deepmodak79.github.io/AskMate/**

(Replace `deepmodak79` with your GitHub username and `AskMate` with your repo name if different.)

---

## Prerequisites (Before You Start)

### 1. GitHub Account

- Go to **https://github.com**
- Sign up or log in
- You need a GitHub account to host on GitHub Pages

### 2. Git Installed on Your Computer

- Open **Command Prompt** (Windows) or **Terminal** (Mac)
- Type: `git --version`
- If you see a version number (e.g. `git version 2.43.0`), Git is installed
- If you see "command not found", install Git from **https://git-scm.com/downloads**

### 3. Your Project on Your Computer

- Your Deep Overflow project should be in a folder (e.g. `c:\MIGRATE 2\Deep Overflow`)
- You should be able to open that folder in your code editor

---

## Part 1: Get Your Code on GitHub

### Step 1.1: Open Terminal/Command Prompt in Your Project Folder

**On Windows:**
1. Open File Explorer
2. Go to your project folder: `c:\MIGRATE 2\Deep Overflow`
3. In the address bar, type `cmd` and press Enter  
   - OR right-click inside the folder → "Open in Terminal" (if available)

**On Mac:**
1. Open Terminal
2. Type: `cd /path/to/your/Deep Overflow`
3. Press Enter

You should see something like: `c:\MIGRATE 2\Deep Overflow>`

---

### Step 1.2: Check If Your Code Is Already on GitHub

Type this command and press Enter:

```
git remote -v
```

- If you see `origin` and a URL like `https://github.com/deepmodak79/AskMate.git` → your code is connected to GitHub. Go to **Step 1.5**.
- If you see nothing or an error → continue to **Step 1.3**.

---

### Step 1.3: Create a New Repository on GitHub (Only If You Don't Have One)

1. Go to **https://github.com**
2. Click the **+** icon (top right) → **New repository**
3. Fill in:
   - **Repository name**: `AskMate` (or any name you like)
   - **Description**: optional
   - **Public**
   - Do **NOT** check "Add a README" (you already have code)
4. Click **Create repository**
5. You will see a page with setup instructions. Keep this page open.

---

### Step 1.4: Connect Your Local Project to GitHub (Only If Not Already Connected)

If you created a new repo in Step 1.3, run these commands (replace `YOUR_USERNAME` and `YOUR_REPO` with your actual GitHub username and repo name):

```
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO.git
git push -u origin main
```

Example for user `deepmodak79` and repo `AskMate`:

```
git remote add origin https://github.com/deepmodak79/AskMate.git
git push -u origin main
```

If Git asks for username/password, use your GitHub username and a **Personal Access Token** (not your normal password). To create one: GitHub → Settings → Developer settings → Personal access tokens.

---

### Step 1.5: Make Sure Your Latest Code Is Pushed

Run:

```
git status
```

- If it says "nothing to commit, working tree clean" → you're up to date. Go to **Part 2**.
- If it lists modified files, run:

```
git add .
git commit -m "Update before deployment"
git push origin main
```

---

## Part 2: Set Up the Deployment Workflow

### Step 2.1: Check If the Workflow File Exists

In your project folder, look for:

```
.github/workflows/deploy-pages.yml
```

- If it exists → go to **Step 2.3**
- If it does NOT exist → do **Step 2.2**

---

### Step 2.2: Create the Workflow File (Only If It Doesn't Exist)

1. In your project folder, create a folder named `.github` (with the dot at the start)
2. Inside `.github`, create a folder named `workflows`
3. Inside `workflows`, create a file named `deploy-pages.yml`
4. Open `deploy-pages.yml` in your editor and paste this:

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [main]

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: "pages"
  cancel-in-progress: false

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
        id: deployment
        uses: actions/deploy-pages@v4
```

**Important:** Replace `/AskMate/` with `/<YOUR_REPO_NAME>/` if your repo has a different name. For example, if your repo is `MyProject`, use `--base-href /MyProject/`.

5. Save the file.

---

### Step 2.3: Push the Workflow to GitHub

Run:

```
git add .
git commit -m "Add GitHub Pages deployment workflow"
git push origin main
```

---

## Part 3: Enable GitHub Pages

### Step 3.1: Open Your Repository on GitHub

1. Go to **https://github.com**
2. Click on your repository (e.g. **AskMate**)

---

### Step 3.2: Go to Settings

1. Click the **Settings** tab (top menu of the repo)
2. In the left sidebar, scroll down to **Pages** (under "Code and automation")

---

### Step 3.3: Configure GitHub Pages

1. Under **Build and deployment**
2. Where it says **Source**, click the dropdown
3. Select **GitHub Actions**
4. Do NOT change anything else
5. The page will save automatically

You should see something like: "GitHub Pages is currently disabled" or "Your site is live at...". That's fine for now.

---

## Part 4: Trigger the Deployment

### Step 4.1: Make a Small Change and Push (To Trigger the Workflow)

The workflow runs when you push to `main`. Let's trigger it:

1. Open any file in your project (e.g. `frontend/src/index.html`)
2. Add a space or a comment, then save
3. In your terminal, run:

```
git add .
git commit -m "Trigger deployment"
git push origin main
```

---

### Step 4.2: Watch the Deployment Run

1. On GitHub, go to your repo
2. Click the **Actions** tab (top menu)
3. You should see a workflow run named "Deploy to GitHub Pages" (yellow = running, green = success, red = failed)
4. Click on it to see details
5. Wait until it turns **green** (usually 2–5 minutes)

---

### Step 4.3: Check If Pages Is Enabled

1. Go back to **Settings** → **Pages**
2. After a successful run, you should see: **"Your site is live at https://YOUR_USERNAME.github.io/YOUR_REPO/"**

---

## Part 5: Open Your Live Site

### Step 5.1: Get Your URL

Your frontend URL will be:

**https://deepmodak79.github.io/AskMate/**

Replace:
- `deepmodak79` → your GitHub username
- `AskMate` → your repository name

---

### Step 5.2: Open It in Your Browser

1. Copy the URL
2. Paste it in your browser
3. Press Enter

You should see your Deep Overflow app. It may show errors when you try to log in or load questions because the **backend is not deployed yet**. That is expected. The frontend is deployed; you will deploy the backend on Render later.

---

## Part 6: If Something Goes Wrong

### Problem: "Actions" tab shows a red X (failed)

1. Click the failed run
2. Click the failed job (red)
3. Expand the step that failed (red)
4. Read the error message

**Common fixes:**

| Error | What to do |
|-------|------------|
| `npm ci` failed | Make sure `frontend/package-lock.json` exists. Run `npm install` in the `frontend` folder locally, then commit and push. |
| `ng build` failed | Run `npm run build:prod` in the `frontend` folder locally. Fix any errors, then push. |
| `path: ./frontend/dist/deep-overflow/browser` not found | Check your Angular project name in `angular.json`. The output path might be different. Look in `frontend/dist/` after a local build. |
| "pages" permission denied | In repo Settings → Actions → General, ensure "Read and write permissions" is allowed for workflows. |

---

### Problem: Site loads but shows a blank white page

1. Open browser Developer Tools (F12)
2. Go to the **Console** tab
3. Look for red errors

**Common cause:** Wrong `base-href`. If your repo is `AskMate`, the base href must be `/AskMate/` (with leading and trailing slashes). Update it in the workflow file and push again.

---

### Problem: "404 - File not found" when opening the URL

- Wait 2–3 minutes after the workflow succeeds. GitHub Pages can take a moment to update.
- Make sure you're using the correct URL: `https://USERNAME.github.io/REPO_NAME/` (with the trailing slash).
- In Settings → Pages, ensure Source is **GitHub Actions**.

---

### Problem: I don't see the "Pages" option in Settings

- GitHub Pages is available for public repos. If your repo is private, you need a paid GitHub account for Pages, or make the repo public.
- Make sure you're in the correct repository.

---

## Summary Checklist

- [ ] Git installed
- [ ] Code pushed to GitHub
- [ ] `.github/workflows/deploy-pages.yml` exists
- [ ] `--base-href /AskMate/` matches your repo name
- [ ] Settings → Pages → Source = **GitHub Actions**
- [ ] Pushed to `main` to trigger the workflow
- [ ] Actions tab shows green checkmark
- [ ] Opened `https://YOUR_USERNAME.github.io/YOUR_REPO/` in browser

---

## Next Step

After your frontend is live, follow the backend deployment guide to deploy on Render. Once the backend is deployed, you will update the frontend's API URL so the app works fully.
