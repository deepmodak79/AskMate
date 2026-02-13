import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '@core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="container">
      <h1>Log In</h1>

      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          <span>Email</span>
          <input formControlName="email" type="email" autocomplete="email" />
        </label>

        <label>
          <span>Password</span>
          <input formControlName="password" type="password" autocomplete="current-password" />
        </label>

        <div class="actions">
          <button class="btn btn-primary" type="submit" [disabled]="form.invalid || isSubmitting">
            {{ isSubmitting ? 'Logging in...' : 'Log In' }}
          </button>
          <a routerLink="/auth/register">Create account</a>
        </div>

        <p class="error" *ngIf="error">{{ error }}</p>
      </form>
    </div>
  `,
  styles: [`
    .container { max-width: 520px; }
    form { display: grid; gap: 1rem; }
    label { display: grid; gap: 0.5rem; }
    input {
      padding: 0.75rem;
      border: 1px solid var(--border-color);
      border-radius: 6px;
      font: inherit;
    }
    .actions { display: flex; align-items: center; gap: 1rem; }
    .btn { padding: 0.6rem 1rem; border-radius: 6px; border: 1px solid transparent; cursor: pointer; }
    .btn-primary { background: var(--primary-color); color: #fff; }
    .error { color: #b00020; margin: 0; }
  `]
})
export class LoginComponent {
  isSubmitting = false;
  error: string | null = null;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {}

  submit() {
    this.error = null;
    if (this.form.invalid) return;
    this.isSubmitting = true;

    this.auth.login({
      email: this.form.value.email!,
      password: this.form.value.password!
    }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/questions']);
        // quick refresh for header state
        window.location.reload();
      },
      error: (err) => {
        this.isSubmitting = false;
        this.error = err?.message || 'Login failed';
      }
    });
  }
}

