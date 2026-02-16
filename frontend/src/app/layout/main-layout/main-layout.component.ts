import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="layout-container">
      <!-- Header -->
      <header class="header">
        <div class="header-content">
          <div class="logo">
            <a routerLink="/">
              <h1>Deep Overflow</h1>
              <span class="company">RMES</span>
            </a>
          </div>
          
          <nav class="main-nav">
            <a routerLink="/questions" routerLinkActive="active">Questions</a>
          </nav>

          <div class="header-actions">
            <div class="search-box">
              <input
                type="search"
                placeholder="Search questions..."
                [(ngModel)]="searchText"
                (keydown.enter)="runSearch()"
              />
            </div>
            
            @if (isAuthenticated) {
              <button class="btn btn-primary" routerLink="/questions/ask">
                Ask Question
              </button>

              <div class="user-menu">
                <button class="user-avatar">
                  <img [src]="getAvatarUrl(currentUser?.avatarUrl)" 
                       [alt]="currentUser?.displayName"
                       (error)="$any($event.target).src='/assets/default-avatar.svg'" />
                  <span class="reputation">{{ currentUser?.reputation }}</span>
                </button>
              </div>
              <button class="btn btn-secondary" type="button" (click)="logout()">
                Logout
              </button>
            } @else {
              <button class="btn btn-secondary" routerLink="/auth/login">
                Log In
              </button>
              <button class="btn btn-secondary" routerLink="/auth/register">
                Register
              </button>
            }

            <button class="theme-toggle" (click)="toggleTheme()">
              <span>{{ isDarkTheme ? '☀' : '🌙' }}</span>
            </button>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <main class="main-content">
        <router-outlet></router-outlet>
      </main>

      <!-- Footer -->
      <footer class="footer">
        <div class="footer-content">
          <div class="footer-links">
            <a href="#">About</a>
            <a href="#">Help</a>
            <a href="#">Contact</a>
          </div>
          <div class="footer-info">
            <p>&copy; 2026 RMES. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    .layout-container {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
    }

    .header {
      background: var(--header-bg);
      border-bottom: 1px solid var(--border-color);
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .header-content {
      max-width: 1280px;
      margin: 0 auto;
      padding: 0 1rem;
      display: flex;
      align-items: center;
      gap: 2rem;
      height: 60px;
    }

    .logo a {
      display: flex;
      align-items: baseline;
      gap: 0.5rem;
      text-decoration: none;
      color: var(--primary-color);
    }

    .logo h1 {
      font-size: 1.5rem;
      font-weight: 700;
      margin: 0;
    }

    .logo .company {
      font-size: 0.875rem;
      color: var(--secondary-text);
    }

    .main-nav {
      display: flex;
      gap: 1.5rem;
    }

    .main-nav a {
      text-decoration: none;
      color: var(--text-color);
      font-weight: 500;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      transition: background 0.2s;
    }

    .main-nav a:hover,
    .main-nav a.active {
      background: var(--hover-bg);
    }

    .header-actions {
      margin-left: auto;
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .search-box input {
      padding: 0.5rem 1rem;
      border: 1px solid var(--border-color);
      border-radius: 4px;
      width: 300px;
    }

    .btn {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 4px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;
    }

    .btn-primary {
      background: var(--primary-color);
      color: white;
    }

    .btn-secondary {
      background: transparent;
      color: var(--primary-color);
      border: 1px solid var(--primary-color);
    }

    .user-avatar {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background: none;
      border: none;
      cursor: pointer;
    }

    .user-avatar img {
      width: 32px;
      height: 32px;
      border-radius: 50%;
    }

    .reputation {
      font-weight: 600;
      color: var(--primary-color);
    }

    .theme-toggle {
      background: none;
      border: none;
      font-size: 1.5rem;
      cursor: pointer;
    }

    .main-content {
      flex: 1;
      max-width: 1280px;
      width: 100%;
      margin: 0 auto;
      padding: 2rem 1rem;
    }

    .footer {
      background: var(--footer-bg);
      border-top: 1px solid var(--border-color);
      padding: 2rem 1rem;
    }

    .footer-content {
      max-width: 1280px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .footer-links {
      display: flex;
      gap: 1.5rem;
    }

    .footer-links a {
      color: var(--text-color);
      text-decoration: none;
    }

    .footer-info p {
      margin: 0;
      color: var(--secondary-text);
    }
  `]
})
export class MainLayoutComponent implements OnInit {
  isAuthenticated = false;
  currentUser: any = null;
  isDarkTheme = false;
  searchText = '';

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.isDarkTheme = localStorage.getItem('theme') === 'dark';
    this.auth.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.isAuthenticated = !!user && this.auth.isAuthenticated();
    });
  }

  runSearch() {
    const q = this.searchText.trim();
    this.router.navigate(['/questions'], { queryParams: q ? { q } : {} });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/questions']);
  }

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;
    localStorage.setItem('theme', this.isDarkTheme ? 'dark' : 'light');
    document.body.classList.toggle('dark-theme', this.isDarkTheme);
  }

  getAvatarUrl(avatarUrl?: string | null): string {
    const url = (avatarUrl || '').trim();
    return url && (url.startsWith('http://') || url.startsWith('https://'))
      ? url
      : '/assets/default-avatar.svg';
  }
}
