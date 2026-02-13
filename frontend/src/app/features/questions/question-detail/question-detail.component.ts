import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '@environments/environment';
import { QuestionDetail, QuestionService } from '@core/services/question.service';

@Component({
  selector: 'app-question-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  template: `
    @if (loading) {
      <p>Loading...</p>
    } @else if (error) {
      <p class="error">{{ error }}</p>
    } @else if (question) {
      <div class="container">
        <a routerLink="/questions" class="back">← Back to questions</a>

        <h1>{{ question.title }}</h1>
        <div class="meta">
          <span>Asked by <b>{{ question.author.displayName }}</b></span>
          <span class="dot">•</span>
          <span>{{ question.createdAt | date:'short' }}</span>
        </div>

        <div class="body">
          <pre>{{ question.body }}</pre>
        </div>

        <div class="tags">
          @for (tag of question.tags; track tag.id) {
            <span class="tag">{{ tag.name }}</span>
          }
        </div>

        <h2>Answers ({{ question.answers?.length || 0 }})</h2>

        <div class="answers">
          @for (a of question.answers; track a.id) {
            <div class="answer" [class.accepted]="a.isAccepted">
              <div class="answer-body"><pre>{{ a.body }}</pre></div>
              <div class="answer-meta">
                <span>{{ a.createdAt | date:'short' }}</span>
                @if (a.isAccepted) { <span class="accepted-badge">Accepted</span> }
              </div>
            </div>
          } @empty {
            <p>No answers yet. Be the first to answer!</p>
          }
        </div>

        <h3>Your Answer</h3>
        <form [formGroup]="answerForm" (ngSubmit)="postAnswer()">
          <textarea formControlName="body" rows="6" placeholder="Write your answer..."></textarea>
          <div class="actions">
            <button class="btn btn-primary" type="submit" [disabled]="answerForm.invalid || posting">
              {{ posting ? 'Posting...' : 'Post Answer' }}
            </button>
          </div>
          <p class="error" *ngIf="postError">{{ postError }}</p>
          <p class="hint">Answer can be a single word or long text.</p>
        </form>
      </div>
    }
  `,
  styles: [`
    .container { max-width: 960px; }
    .back { display: inline-block; margin-bottom: 1rem; text-decoration: none; color: var(--primary-color); }
    .meta { display:flex; gap: .5rem; color: var(--secondary-text); margin-bottom: 1rem; align-items: center; }
    .dot { opacity: .6; }
    .body pre, .answer-body pre {
      white-space: pre-wrap;
      word-break: break-word;
      background: var(--hover-bg);
      padding: 1rem;
      border-radius: 8px;
      border: 1px solid var(--border-color);
      margin: 0;
      font-family: inherit;
    }
    .tags { display:flex; gap:.5rem; flex-wrap: wrap; margin: 1rem 0 2rem; }
    .tag { padding: .25rem .6rem; background: var(--tag-bg); border-radius: 6px; }
    .answers { display: grid; gap: 1rem; margin-bottom: 2rem; }
    .answer { border: 1px solid var(--border-color); border-radius: 8px; padding: 1rem; }
    .answer.accepted { border-color: #2e7d32; }
    .answer-meta { display:flex; gap: .75rem; align-items:center; margin-top: .75rem; color: var(--secondary-text); }
    .accepted-badge { color: #2e7d32; font-weight: 600; }
    textarea {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid var(--border-color);
      border-radius: 8px;
      font: inherit;
    }
    .actions { margin-top: .75rem; }
    .btn { padding: 0.6rem 1rem; border-radius: 6px; border: 1px solid transparent; cursor: pointer; }
    .btn-primary { background: var(--primary-color); color: #fff; }
    .error { color: #b00020; }
    .hint { color: var(--secondary-text); margin: .5rem 0 0; }
  `]
})
export class QuestionDetailComponent implements OnInit {
  question: QuestionDetail | null = null;
  loading = true;
  error: string | null = null;

  posting = false;
  postError: string | null = null;

  answerForm = this.fb.group({
    body: ['', [Validators.required, Validators.minLength(1)]]
  });

  constructor(
    private route: ActivatedRoute,
    private questionService: QuestionService,
    private http: HttpClient,
    private fb: FormBuilder
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      if (!slug) return;
      this.fetch(slug);
    });
  }

  private fetch(slug: string) {
    this.loading = true;
    this.error = null;

    this.questionService.getQuestionBySlug(slug).subscribe({
      next: (q) => {
        this.question = q;
        this.loading = false;
      },
      error: () => {
        this.error = 'Question not found';
        this.loading = false;
      }
    });
  }

  postAnswer() {
    if (!this.question) return;
    if (this.answerForm.invalid) return;

    this.posting = true;
    this.postError = null;

    const body = this.answerForm.value.body!.trim();

    this.http.post(`${environment.apiUrl}/answers`, {
      questionId: this.question.id,
      body
    }).subscribe({
      next: () => {
        this.posting = false;
        this.answerForm.reset();
        // reload question to show new answer
        this.fetch(this.question!.slug);
      },
      error: (err) => {
        this.posting = false;
        this.postError = err?.error?.error || 'Failed to post answer (are you logged in?)';
      }
    });
  }
}

