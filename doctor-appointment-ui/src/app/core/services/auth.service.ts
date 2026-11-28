import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';

export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  role: string;
  userId: number;
  profileId: number;
}

export interface RegisterDto {
  name: string;
  email: string;
  password: string;
  role: string;
  phoneNumber?: string;
  specializationId?: number;
  licenseNumber?: string;
  experienceYears?: number;
  consultationFee?: number;
  bio?: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient, private tokenService: TokenService) {}

  register(dto: RegisterDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, dto).pipe(
      tap(res => this.tokenService.saveAuthData(res))
    );
  }

  login(dto: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, dto).pipe(
      tap(res => this.tokenService.saveAuthData(res))
    );
  }

  logout(): void {
    this.tokenService.clearAuthData();
  }

  isLoggedIn(): boolean {
    return !!this.tokenService.getToken();
  }

  getCurrentUser(): AuthResponse | null {
    return this.tokenService.getAuthData();
  }

  hasRole(role: string): boolean {
    return this.getCurrentUser()?.role?.toLowerCase() === role.toLowerCase();
  }
}
