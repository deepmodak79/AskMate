import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
      } else if (error.status === 0) {
        // Network/CORS failure (no response)
        errorMessage = 'Cannot reach server. Is the API running at ' + (error.url || 'localhost:5000') + '? Check CORS and try again.';
      } else {
        // Backend returns { error: "..." } or { errors: ["..."] } for validation
        const backendError = error.error?.error || error.error?.errorMessage
          || (Array.isArray(error.error?.errors) ? error.error.errors.join('. ') : null);
        switch (error.status) {
          case 401:
            errorMessage = backendError || 'Unauthorized. Please log in.';
            // Don't redirect when already on auth pages (login/register)
            const isAuthRequest = req.url.includes('/auth/login') || req.url.includes('/auth/register');
            if (!isAuthRequest) {
              router.navigate(['/auth/login']);
            }
            break;
          case 403:
            errorMessage = backendError || 'You do not have permission to perform this action.';
            break;
          case 404:
            errorMessage = backendError || 'Resource not found.';
            break;
          case 400:
            errorMessage = backendError || 'Invalid request. Please check your input.';
            break;
          case 429:
            errorMessage = backendError || 'Too many requests. Please try again later.';
            break;
          case 500:
            errorMessage = backendError || 'Server error. Please try again later.';
            break;
          default:
            errorMessage = backendError || error.message || 'An error occurred';
        }
      }

      console.error('HTTP Error:', error);

      return throwError(() => new Error(errorMessage));
    })
  );
};
