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

        <div class="solution-toggle">
          <label class="checkbox-label">
            <input type="checkbox" [checked]="hasSolution" (change)="toggleSolution($event)" />
            <span>I have a solution to share</span>
          </label>
          <p class="hint-inline">Post your question along with your solution to help others facing the same problem.</p>
        </div>

        <label *ngIf="hasSolution">
          <span>Your Solution</span>
          <textarea formControlName="solution" rows="8" placeholder="Describe how you solved the problem..."></textarea>
          <span class="field-error" *ngIf="form.get('solution')?.invalid && form.get('solution')?.touched">
            Solution must be at least 20 characters
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
            {{ isSubmitting ? 'Posting...' : (hasSolution ? 'Post Question & Solution' : 'Post Question') }}
          </button>
          <button class="btn btn-secondary" type="button" (click)="cancel()">Cancel</button>
        </div>

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
    .hint-inline { color: var(--secondary-text); margin: 0.25rem 0 0; font-size: 0.9rem; }
    .solution-toggle { margin: 0.5rem 0; }
    .checkbox-label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; }
    .checkbox-label input { width: auto; }
    .error { color: #b00020; margin: 0; }
  `]
})
export class QuestionFormComponent {
  isSubmitting = false;
  error: string | null = null;
  hasSolution = false;

  form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(300)]],
    body: ['', [Validators.required, Validators.minLength(20)]],
    solution: [''],
    tags: ['', [Validators.required]]
  });

  constructor(
    private fb: FormBuilder,
    private questionService: QuestionService,
    private router: Router
  ) {}

  toggleSolution(ev: Event) {
    this.hasSolution = (ev.target as HTMLInputElement).checked;
    const solutionControl = this.form.get('solution');
    if (this.hasSolution) {
      solutionControl?.setValidators([Validators.required, Validators.minLength(20)]);
    } else {
      solutionControl?.clearValidators();
      solutionControl?.setValue('');
    }
    solutionControl?.updateValueAndValidity();
  }

  submit() {
    this.error = null;
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const title = this.form.value.title!.trim();
    const body = this.form.value.body!.trim();
    const solution = this.hasSolution ? this.form.value.solution?.trim() : undefined;
    const tags = [...new Set(
      (this.form.value.tags || '')
        .split(',')
        .map(t => t.trim().toLowerCase().replace(/[^a-z0-9\-_]/g, ''))
        .filter(Boolean)
    )].slice(0, 5);

    const payload: { title: string; body: string; tags: string[]; solution?: string } = { title, body, tags };
    if (solution && solution.length >= 20) payload.solution = solution;

    this.isSubmitting = true;
    this.questionService.createQuestion(payload).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigate(['/questions']);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.error = err?.message || 'Failed to post question. Please ensure you are logged in.';
      }
    });
  }

  cancel() {
    this.router.navigate(['/questions']);
  }
}

