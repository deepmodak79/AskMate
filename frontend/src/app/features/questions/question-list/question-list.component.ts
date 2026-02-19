import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { QuestionService, Question, QuestionFilters } from '@core/services/question.service';

@Component({
  selector: 'app-question-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="question-list-container">
      <div class="list-header">
        <h1>Questions</h1>
      </div>

      <div class="filters">
        <button 
          *ngFor="let filter of filters" 
          [class.active]="currentFilter === filter.value"
          [attr.title]="filter.title"
          (click)="setFilter(filter.value)">
          {{ filter.label }}
        </button>
      </div>

      <div class="questions">
        @for (question of questions; track question.id) {
          <div class="question-item">
            <div class="question-stats">
              <div class="stat">
                <span class="number">{{ question.voteScore }}</span>
                <span class="label">votes</span>
              </div>
              <div class="stat" [class.answered]="question.answerCount > 0">
                <span class="number">{{ question.answerCount }}</span>
                <span class="label">answers</span>
              </div>
              <div class="stat">
                <span class="number">{{ question.viewCount }}</span>
                <span class="label">views</span>
              </div>
            </div>

            <div class="question-content">
              <h3>
                <a [routerLink]="['/questions', question.slug]">{{ question.title }}</a>
              </h3>
              <div class="question-meta">
                <div class="tags">
                  @for (tag of question.tags; track tag.id) {
                    <span class="tag">{{ tag.name }}</span>
                  }
                </div>
                <div class="author-info">
                  <img [src]="getAvatarUrl(question.author?.avatarUrl)" 
                       [alt]="question.author.displayName"
                       (error)="$any($event.target).src=getDefaultAvatarPath()" />
                  <span>{{ question.author.displayName }}</span>
                  <span class="reputation">{{ question.author.reputation }}</span>
                  <span class="timestamp">{{ question.createdAt | date:'short' }}</span>
                </div>
              </div>
            </div>
          </div>
        } @empty {
          <div class="empty-state">
            <p>No questions found. Be the first to ask!</p>
          </div>
        }
      </div>

      @if (hasMore) {
        <div class="load-more">
          <button class="btn btn-secondary" (click)="loadMore()">Load More</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .question-list-container {
      max-width: 1200px;
    }

    .list-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }

    .filters {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
      border-bottom: 1px solid var(--border-color);
      padding-bottom: 1rem;
    }

    .filters button {
      padding: 0.5rem 1rem;
      background: none;
      border: 1px solid var(--border-color);
      border-radius: 4px;
      cursor: pointer;
      transition: all 0.2s;
    }

    .filters button.active {
      background: var(--primary-color);
      color: white;
      border-color: var(--primary-color);
    }

    .question-item {
      display: flex;
      gap: 1.5rem;
      padding: 1.5rem;
      border: 1px solid var(--border-color);
      border-radius: 8px;
      margin-bottom: 1rem;
      transition: box-shadow 0.2s;
    }

    .question-item:hover {
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    }

    .question-stats {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      min-width: 100px;
    }

    .stat {
      text-align: center;
      padding: 0.25rem 0.5rem;
      border: 1px solid var(--border-color);
      border-radius: 4px;
    }

    .stat.answered {
      background: #48c774;
      color: white;
      border-color: #48c774;
    }

    .stat .number {
      display: block;
      font-size: 1.25rem;
      font-weight: 600;
    }

    .stat .label {
      display: block;
      font-size: 0.75rem;
      text-transform: uppercase;
    }

    .question-content {
      flex: 1;
    }

    .question-content h3 {
      margin: 0 0 0.75rem 0;
      font-size: 1.25rem;
    }

    .question-content h3 a {
      color: var(--primary-color);
      text-decoration: none;
    }

    .question-content h3 a:hover {
      text-decoration: underline;
    }

    .question-meta {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
    }

    .tags {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .tag {
      padding: 0.25rem 0.75rem;
      background: var(--tag-bg);
      color: var(--tag-color);
      border-radius: 4px;
      font-size: 0.875rem;
    }

    .author-info {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
    }

    .author-info img {
      width: 24px;
      height: 24px;
      border-radius: 50%;
    }

    .reputation {
      font-weight: 600;
      color: var(--primary-color);
    }

    .timestamp {
      color: var(--secondary-text);
    }

    .empty-state {
      text-align: center;
      padding: 3rem;
      color: var(--secondary-text);
    }

    .load-more {
      text-align: center;
      margin-top: 2rem;
    }
  `]
})
export class QuestionListComponent implements OnInit {
  questions: Question[] = [];
  currentFilter = 'newest';
  hasMore = false;
  searchQuery: string | null = null;
  
  filters = [
    { label: 'Newest', value: 'newest', title: 'Questions ordered by creation date (newest first)' },
    { label: 'Active', value: 'active', title: 'Questions ordered by recent activity (new answers, edits)' },
    { label: 'Unanswered', value: 'unanswered', title: 'Only questions with no answers' },
    { label: 'Most Votes', value: 'votes', title: 'Questions ordered by vote count (highest first)' }
  ];

  constructor(private questionService: QuestionService, private route: ActivatedRoute) {}

  ngOnInit() {
    this.route.queryParamMap.subscribe(qp => {
      this.searchQuery = qp.get('q');
      this.loadQuestions();
    });
  }

  loadQuestions() {
    const filters: QuestionFilters = {
      page: 1,
      pageSize: 20,
      sortBy: this.currentFilter as any,
      searchQuery: this.searchQuery || undefined
    };

    this.questionService.getQuestions(filters).subscribe({
      next: (result) => {
        this.questions = result.items;
        this.hasMore = result.hasNextPage;
      },
      error: (error) => {
        console.error('Error loading questions:', error);
      }
    });
  }

  setFilter(filter: string) {
    this.currentFilter = filter;
    this.loadQuestions();
  }

  loadMore() {
    // Implementation for pagination
  }

  getAvatarUrl(avatarUrl?: string | null): string {
    const url = (avatarUrl || '').trim();
    return url && (url.startsWith('http://') || url.startsWith('https://'))
      ? url
      : this.getDefaultAvatarPath();
  }

  getDefaultAvatarPath(): string {
    const base = typeof document !== 'undefined' ? (document.querySelector('base')?.getAttribute('href') || '/') : '/';
    const normalized = base.endsWith('/') ? base : base + '/';
    return normalized + 'assets/default-avatar.svg';
  }
}
