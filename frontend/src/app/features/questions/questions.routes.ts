import { Routes } from '@angular/router';

export const QUESTION_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./question-list/question-list.component')
      .then(m => m.QuestionListComponent)
  },
  {
    path: 'ask',
    loadComponent: () => import('./question-form/question-form.component')
      .then(m => m.QuestionFormComponent)
  },
  {
    path: ':slug',
    loadComponent: () => import('./question-detail/question-detail.component')
      .then(m => m.QuestionDetailComponent)
  }
];
