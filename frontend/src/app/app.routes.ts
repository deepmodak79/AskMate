import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/main-layout/main-layout.component')
      .then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'questions',
        pathMatch: 'full'
      },
      {
        path: 'questions',
        loadChildren: () => import('./features/questions/questions.routes')
          .then(m => m.QUESTION_ROUTES)
      },
      {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes')
          .then(m => m.AUTH_ROUTES)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'questions'
  }
];
