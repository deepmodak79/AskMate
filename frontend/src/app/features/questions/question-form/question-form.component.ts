import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { QuestionService } from '@core/services/question.service';

@Component({
  selector: 'app-question-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="container">
      <h1>Ask a Question</h1>

      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          <span>Title</span>
          <input formControlName="title" placeholder="Eg. How to install XYZ on Windows?" />
          <span class="field-error" *ngIf="form.get('title')?.invalid && form.get('title')?.touched">
            {{ form.get('title')?.errors?.['required'] ? 'Title is required' : (form.get('title')?.errors?.['minlength'] ? 'Title must be at least 10 characters' : 'Title max 300 characters') }}
          </span>
        </label>

        <label>
          <span>Body</span>
          <textarea formControlName="body" rows="10" placeholder="Describe the problem, what you tried, errors/logs..."></textarea>
          <span class="field-error" *ngIf="form.get('body')?.invalid && form.get('body')?.touched">
            {{ form.get('body')?.errors?.['required'] ? 'Body is required' : 'Body must be at least 20 characters' }}
          </span>
        </label>

        <label>
          <span>Tags (comma separated)</span>
          <input formControlName="tags" placeholder="eg. windows, scada, networking" />
          <span class="field-error" *ngIf="form.get('tags')?.invalid && form.get('tags')?.touched">
            At least one tag is required
          </span>
        </label>

        <div class="actions">
          <button class="btn btn-primary" type="submit" [disabled]="form.invalid || isSubmitting">
            {{ isSubmitting ? 'Posting...' : 'Post Question' }}
          </button>
          <button class="btn btn-secondary" type="button" (click)="cancel()">Cancel</button>
        </div>

        <p class="hint">
          Note: you must be logged in to post.
        </p>

        <p class="error" *ngIf="error">{{ error }}</p>
      </form>
    </div>
  `,
  styles: [`
    .container { max-width: 900px; }
    form { display: grid; gap: 1rem; }
    label { display: grid; gap: 0.5rem; }
    input, textarea {
      padding: 0.75rem;
      border: 1px solid var(--border-color);
      border-radius: 6px;
      font: inherit;
    }
    .actions { display: flex; gap: 0.75rem; }
    .btn {
      padding: 0.6rem 1rem;
      border-radius: 6px;
      border: 1px solid transparent;
      cursor: pointer;
    }
    .btn-primary { background: var(--primary-color); color: #fff; }
    .btn:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-secondary { background: transparent; border-color: var(--border-color); }
    .field-error { color: #b00020; font-size: 0.85rem; }
    .hint { color: var(--secondary-text); margin: 0; }
    .error { color: #b00020; margin: 0; }
  `]
})
export class QuestionFormComponent {
  isSubmitting = false;
  error: string | null = null;

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(300)]],
    body: ['', [Validators.required, Validators.minLength(20)]],
    tags: ['', [Validators.required]]
  });

  constructor(
    private fb: FormBuilder,
    private questionService: QuestionService,
    private router: Router
  ) {}

  submit() {
    this.error = null;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const title = this.form.value.title!.trim();
    const body = this.form.value.body!.trim();
    const tags = (this.form.value.tags || '')
      .split(',')
      .map(t => t.trim().toLowerCase())
      .filter(Boolean)
      .slice(0, 5);

    this.isSubmitting = true;
    this.questionService.createQuestion({ title, body, tags }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/questions']);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.error = err?.message || 'Failed to post question';
      }
    });
  }

  cancel() {
    this.router.navigate(['/questions']);
  }
}

