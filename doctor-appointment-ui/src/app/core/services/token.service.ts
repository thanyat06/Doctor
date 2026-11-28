import { Injectable } from '@angular/core';

const TOKEN_KEY = 'doctor_app_token';
const USER_KEY = 'doctor_app_user';

@Injectable({ providedIn: 'root' })
export class TokenService {
  saveAuthData(data: any): void {
    localStorage.setItem(TOKEN_KEY, data.token);
    localStorage.setItem(USER_KEY, JSON.stringify(data));
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getAuthData(): any | null {
    const data = localStorage.getItem(USER_KEY);
    return data ? JSON.parse(data) : null;
  }

  clearAuthData(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
