import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  // Check role-based access
  const requiredRoles = route.data['roles'] as string[];
  if (requiredRoles && requiredRoles.length > 0) {
    const hasRole = requiredRoles.some(role => authService.hasRole(role));
    if (!hasRole) {
      router.navigate(['/']);
      return false;
    }
  }

  return true;
};
