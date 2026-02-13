import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <div class="app-container" [class.dark-theme]="isDarkTheme">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background-color: var(--background-color);
      color: var(--text-color);
    }

    .dark-theme {
      --background-color: #1a1a1a;
      --text-color: #e0e0e0;
      --primary-color: #4a9eff;
      --secondary-color: #6c757d;
      --border-color: #333;
    }

    :host:not(.dark-theme) {
      --background-color: #ffffff;
      --text-color: #212529;
      --primary-color: #007bff;
      --secondary-color: #6c757d;
      --border-color: #dee2e6;
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'Deep Overflow';
  isDarkTheme = false;

  ngOnInit() {
    // Load theme preference from localStorage
    const savedTheme = localStorage.getItem('theme');
    this.isDarkTheme = savedTheme === 'dark';
  }

  toggleTheme() {
    this.isDarkTheme = !this.isDarkTheme;
    localStorage.setItem('theme', this.isDarkTheme ? 'dark' : 'light');
  }
}
