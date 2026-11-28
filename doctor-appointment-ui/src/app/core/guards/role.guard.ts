import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isLoggedIn()) {
      router.navigate(['/auth/login']);
      return false;
    }

    const user = authService.getCurrentUser();
    if (user && allowedRoles.some(r => r.toLowerCase() === user.role?.toLowerCase())) {
      return true;
    }

    router.navigate(['/dashboard']);
    return false;
  };
};
