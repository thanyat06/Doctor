import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="auth-header">
          <div class="logo-icon">🏥</div>
          <h1>Doctor Appointment</h1>
          <p>Sign in to your account</p>
        </div>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label for="email">Email Address</label>
            <input id="email" type="email" formControlName="email" placeholder="Enter your email"
              [class.error]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched" />
            <span class="error-msg" *ngIf="loginForm.get('email')?.invalid && loginForm.get('email')?.touched">
              Valid email is required
            </span>
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input id="password" type="password" formControlName="password" placeholder="Enter your password"
              [class.error]="loginForm.get('password')?.invalid && loginForm.get('password')?.touched" />
            <span class="error-msg" *ngIf="loginForm.get('password')?.invalid && loginForm.get('password')?.touched">
              Password is required
            </span>
          </div>

          <div class="error-alert" *ngIf="errorMessage">{{ errorMessage }}</div>

          <button type="submit" class="btn-primary" [disabled]="loginForm.invalid || loading">
            <span *ngIf="loading">Signing in...</span>
            <span *ngIf="!loading">Sign In</span>
          </button>
        </form>

        <div class="auth-footer">
          <p>Don't have an account? <a routerLink="/auth/register">Register here</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
      padding: 1rem;
    }
    .auth-card {
      background: rgba(255,255,255,0.05);
      backdrop-filter: blur(20px);
      border: 1px solid rgba(255,255,255,0.1);
      border-radius: 24px;
      padding: 2.5rem;
      width: 100%;
      max-width: 420px;
      box-shadow: 0 25px 50px rgba(0,0,0,0.5);
    }
    .auth-header { text-align: center; margin-bottom: 2rem; }
    .logo-icon { font-size: 3rem; margin-bottom: 0.5rem; }
    .auth-header h1 { color: #fff; font-size: 1.75rem; font-weight: 700; margin: 0.5rem 0; }
    .auth-header p { color: rgba(255,255,255,0.6); margin: 0; }
    .auth-form { display: flex; flex-direction: column; gap: 1.25rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
    label { color: rgba(255,255,255,0.8); font-size: 0.875rem; font-weight: 500; }
    input {
      background: rgba(255,255,255,0.08);
      border: 1px solid rgba(255,255,255,0.15);
      border-radius: 10px;
      padding: 0.75rem 1rem;
      color: #fff;
      font-size: 0.95rem;
      outline: none;
      transition: border-color 0.2s;
    }
    input:focus { border-color: #4f8ef7; }
    input.error { border-color: #ef4444; }
    input::placeholder { color: rgba(255,255,255,0.3); }
    .error-msg { color: #ef4444; font-size: 0.75rem; }
    .error-alert {
      background: rgba(239,68,68,0.15);
      border: 1px solid rgba(239,68,68,0.3);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      color: #ef4444;
      font-size: 0.875rem;
    }
    .btn-primary {
      background: linear-gradient(135deg, #4f8ef7, #7c3aed);
      color: #fff;
      border: none;
      border-radius: 10px;
      padding: 0.875rem;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: opacity 0.2s, transform 0.1s;
      margin-top: 0.5rem;
    }
    .btn-primary:hover:not(:disabled) { opacity: 0.9; transform: translateY(-1px); }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .auth-footer { text-align: center; margin-top: 1.5rem; }
    .auth-footer p { color: rgba(255,255,255,0.6); margin: 0; }
    .auth-footer a { color: #4f8ef7; text-decoration: none; font-weight: 500; }
    .auth-footer a:hover { text-decoration: underline; }
  `]
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Login failed. Please check your credentials.';
      }
    });
  }
}
