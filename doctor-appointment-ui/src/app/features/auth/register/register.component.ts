import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SpecializationService, SpecializationDto } from '../../../core/services/specialization.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="auth-header">
          <div class="logo-icon">🏥</div>
          <h1>Create Account</h1>
          <p>Join our healthcare platform</p>
        </div>

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="role-selector">
            <button type="button" class="role-btn" [class.active]="selectedRole === 'Patient'" (click)="setRole('Patient')">
              👤 Patient
            </button>
            <button type="button" class="role-btn" [class.active]="selectedRole === 'Doctor'" (click)="setRole('Doctor')">
              👨‍⚕️ Doctor
            </button>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Full Name</label>
              <input type="text" formControlName="name" placeholder="Your full name" />
            </div>
            <div class="form-group">
              <label>Email</label>
              <input type="email" formControlName="email" placeholder="Email address" />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Password</label>
              <input type="password" formControlName="password" placeholder="Create password" />
            </div>
            <div class="form-group">
              <label>Phone Number</label>
              <input type="text" formControlName="phoneNumber" placeholder="Phone number" />
            </div>
          </div>

          <!-- Doctor-specific fields -->
          <ng-container *ngIf="selectedRole === 'Doctor'">
            <div class="form-row">
              <div class="form-group">
                <label>License Number</label>
                <input type="text" formControlName="licenseNumber" placeholder="Medical license #" />
              </div>
              <div class="form-group">
                <label>Specialization</label>
                <select formControlName="specializationId">
                  <option value="">Select specialization</option>
                  <option *ngFor="let spec of specializations" [value]="spec.id">{{ spec.name }}</option>
                </select>
              </div>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Experience (years)</label>
                <input type="number" formControlName="experienceYears" placeholder="Years of experience" />
              </div>
              <div class="form-group">
                <label>Consultation Fee (₹)</label>
                <input type="number" formControlName="consultationFee" placeholder="Fee amount" />
              </div>
            </div>
          </ng-container>

          <div class="error-alert" *ngIf="errorMessage">{{ errorMessage }}</div>

          <button type="submit" class="btn-primary" [disabled]="registerForm.invalid || loading">
            <span *ngIf="loading">Creating account...</span>
            <span *ngIf="!loading">Create Account</span>
          </button>
        </form>

        <div class="auth-footer">
          <p>Already have an account? <a routerLink="/auth/login">Sign in</a></p>
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
      padding: 1.5rem;
    }
    .auth-card {
      background: rgba(255,255,255,0.05);
      backdrop-filter: blur(20px);
      border: 1px solid rgba(255,255,255,0.1);
      border-radius: 24px;
      padding: 2.5rem;
      width: 100%;
      max-width: 600px;
      box-shadow: 0 25px 50px rgba(0,0,0,0.5);
    }
    .auth-header { text-align: center; margin-bottom: 2rem; }
    .logo-icon { font-size: 2.5rem; margin-bottom: 0.5rem; }
    .auth-header h1 { color: #fff; font-size: 1.75rem; font-weight: 700; margin: 0.5rem 0; }
    .auth-header p { color: rgba(255,255,255,0.6); margin: 0; }
    .role-selector { display: flex; gap: 1rem; margin-bottom: 0.5rem; }
    .role-btn {
      flex: 1;
      padding: 0.75rem;
      border: 1px solid rgba(255,255,255,0.15);
      border-radius: 10px;
      background: rgba(255,255,255,0.05);
      color: rgba(255,255,255,0.7);
      cursor: pointer;
      font-size: 0.95rem;
      transition: all 0.2s;
    }
    .role-btn.active {
      background: linear-gradient(135deg, #4f8ef7, #7c3aed);
      border-color: transparent;
      color: #fff;
      font-weight: 600;
    }
    .auth-form { display: flex; flex-direction: column; gap: 1rem; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
    label { color: rgba(255,255,255,0.8); font-size: 0.875rem; font-weight: 500; }
    input, select {
      background: rgba(255,255,255,0.08);
      border: 1px solid rgba(255,255,255,0.15);
      border-radius: 10px;
      padding: 0.75rem 1rem;
      color: #fff;
      font-size: 0.875rem;
      outline: none;
      transition: border-color 0.2s;
    }
    select option { background: #1a1a2e; }
    input:focus, select:focus { border-color: #4f8ef7; }
    input::placeholder { color: rgba(255,255,255,0.3); }
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
      transition: opacity 0.2s;
    }
    .btn-primary:hover:not(:disabled) { opacity: 0.9; }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    .auth-footer { text-align: center; margin-top: 1.5rem; }
    .auth-footer p { color: rgba(255,255,255,0.6); margin: 0; }
    .auth-footer a { color: #4f8ef7; text-decoration: none; font-weight: 500; }
    @media (max-width: 480px) { .form-row { grid-template-columns: 1fr; } }
  `]
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  loading = false;
  errorMessage = '';
  selectedRole = 'Patient';
  specializations: SpecializationDto[] = [];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private specializationService: SpecializationService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      phoneNumber: [''],
      licenseNumber: [''],
      specializationId: [''],
      experienceYears: [null],
      consultationFee: [null]
    });
  }

  ngOnInit(): void {
    this.specializationService.getAll().subscribe(specs => {
      this.specializations = specs;
    });
  }

  setRole(role: string): void {
    this.selectedRole = role;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    const formValue = this.registerForm.value;
    const payload: any = {
      name: formValue.name,
      email: formValue.email,
      password: formValue.password,
      role: this.selectedRole,
      phoneNumber: formValue.phoneNumber || undefined
    };

    if (this.selectedRole === 'Doctor') {
      payload.licenseNumber = formValue.licenseNumber;
      payload.specializationId = +formValue.specializationId;
      payload.experienceYears = formValue.experienceYears;
      payload.consultationFee = formValue.consultationFee;
    }

    this.authService.register(payload).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
      }
    });
  }
}
