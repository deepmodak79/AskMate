import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { environment } from '@environments/environment';
import { AuthService, LoginResponse } from '@core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatSnackBarModule],
  template: `
    <div class="container">
      <h1>Register</h1>

      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          <span>Email</span>
          <input formControlName="email" type="email" autocomplete="email" />
          <span class="field-error" *ngIf="form.get('email')?.invalid && form.get('email')?.touched">
            {{ form.get('email')?.errors?.['required'] ? 'Email is required' : 'Enter a valid email' }}
          </span>
        </label>

        <label>
          <span>Username</span>
          <input formControlName="username" autocomplete="username" />
          <span class="field-error" *ngIf="form.get('username')?.invalid && form.get('username')?.touched">
            {{ form.get('username')?.errors?.['required'] ? 'Username is required' : 'Username must be at least 3 characters' }}
          </span>
        </label>

        <label>
          <span>Display name</span>
          <input formControlName="displayName" />
          <span class="field-error" *ngIf="form.get('displayName')?.invalid && form.get('displayName')?.touched">
            Display name is required
          </span>
        </label>

        <label>
          <span>Department (optional)</span>
          <input formControlName="department" />
        </label>

        <label>
          <span>Password</span>
          <input formControlName="password" type="password" autocomplete="new-password" />
          <span class="field-error" *ngIf="form.get('password')?.invalid && form.get('password')?.touched">
            {{ form.get('password')?.errors?.['required'] ? 'Password is required' : 'Password must be at least 6 characters' }}
          </span>
        </label>

        <div class="actions">
          <button class="btn btn-primary" type="submit" [disabled]="form.invalid || isSubmitting">
            {{ isSubmitting ? 'Creating...' : 'Create account' }}
          </button>
          <a routerLink="/auth/login">Already have an account?</a>
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
    .field-error { color: #b00020; font-size: 0.85rem; }
    .actions { display: flex; align-items: center; gap: 1rem; }
    .btn { padding: 0.6rem 1rem; border-radius: 6px; border: 1px solid transparent; cursor: pointer; }
    .btn-primary { background: var(--primary-color); color: #fff; }
    .btn:disabled { opacity: 0.6; cursor: not-allowed; }
    .error { color: #b00020; margin: 0; font-weight: 500; }
  `]
})
export class RegisterComponent {
  isSubmitting = false;
  error: string | null = null;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    username: ['', [Validators.required, Validators.minLength(3)]],
    displayName: ['', [Validators.required]],
    department: [''],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private auth: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  submit() {
    this.error = null;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;
    this.isSubmitting = true;

    this.http.post<LoginResponse>(`${environment.apiUrl}/auth/register`, {
      email: this.form.value.email,
      username: this.form.value.username,
      displayName: this.form.value.displayName,
      department: this.form.value.department,
      password: this.form.value.password
    }).subscribe({
      next: (resp) => {
        this.auth.applyLoginResponse(resp);
        this.isSubmitting = false;
        const name = resp.user?.displayName || resp.user?.username || 'User';
        this.snackBar.open(`Registration successful! Welcome, ${name}.`, 'OK', {
          duration: 3000,
          horizontalPosition: 'center',
          verticalPosition: 'top'
        });
        this.router.navigate(['/questions']);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.error = err?.message || 'Registration failed';
      }
    });
  }
}

