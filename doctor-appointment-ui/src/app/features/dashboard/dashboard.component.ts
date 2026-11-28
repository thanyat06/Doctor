import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AppointmentService, AppointmentDto } from '../../core/services/appointment.service';
import { DoctorService } from '../../core/services/doctor.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard">
      <!-- Navbar -->
      <nav class="navbar">
        <div class="nav-brand">
          <span class="brand-icon">🏥</span>
          <span class="brand-name">DocConnect</span>
        </div>
        <div class="nav-links">
          <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
          <a routerLink="/doctors">Find Doctors</a>
          <a *ngIf="isPatient" routerLink="/appointments/book">Book Appointment</a>
          <a routerLink="/appointments">My Appointments</a>
          <a *ngIf="isAdmin" routerLink="/admin/doctors">Admin</a>
        </div>
        <div class="nav-user">
          <span class="user-name">{{ currentUser?.name }}</span>
          <span class="user-badge" [class]="'badge-' + currentUser?.role?.toLowerCase()">{{ currentUser?.role }}</span>
          <button class="btn-logout" (click)="logout()">Logout</button>
        </div>
      </nav>

      <!-- Main Content -->
      <main class="main-content">
        <!-- Welcome Banner -->
        <div class="welcome-banner">
          <div class="welcome-text">
            <h1>Welcome back, {{ currentUser?.name }}! 👋</h1>
            <p>Here's your healthcare overview for today</p>
          </div>
          <div class="welcome-action" *ngIf="isPatient">
            <button class="btn-book" routerLink="/appointments/book">
              📅 Book Appointment
            </button>
          </div>
        </div>

        <!-- Stats Cards -->
        <div class="stats-grid">
          <ng-container *ngIf="!isPatient">
          <div class="stat-card stat-blue">
            <div class="stat-icon">📋</div>
            <div class="stat-info">
              <span class="stat-value">{{ appointments.length }}</span>
              <span class="stat-label">Total Appointments</span>
            </div>
          </div>
          <div class="stat-card stat-green">
            <div class="stat-icon">✅</div>
            <div class="stat-info">
              <span class="stat-value">{{ confirmedCount }}</span>
              <span class="stat-label">Confirmed</span>
            </div>
          </div>
          <div class="stat-card stat-orange">
            <div class="stat-icon">⏳</div>
            <div class="stat-info">
              <span class="stat-value">{{ pendingCount }}</span>
              <span class="stat-label">Pending</span>
            </div>
          </div>
          <div class="stat-card stat-purple">
            <div class="stat-icon">👨‍⚕️</div>
            <div class="stat-info">
              <span class="stat-value">{{ availableDoctors }}</span>
              <span class="stat-label">Available Doctors</span>
            </div>
          </div>
          </ng-container>
          <div class="stat-card stat-blue" *ngIf="isPatient">
            <div class="stat-icon">📅</div>
            <div class="stat-info">
              <span class="stat-value">Care</span>
              <span class="stat-label">Start with a new appointment</span>
            </div>
          </div>
          <div class="stat-card stat-green" *ngIf="isPatient">
            <div class="stat-icon">👨‍⚕️</div>
            <div class="stat-info">
              <span class="stat-value">{{ availableDoctors }}</span>
              <span class="stat-label">Available Doctors</span>
            </div>
          </div>
        </div>

        <!-- Recent Appointments -->
        <div class="section">
          <div class="error-alert" *ngIf="errorMessage">{{ errorMessage }}</div>
          <div class="section-header">
            <h2>{{ isPatient ? 'Your care' : 'Recent Appointments' }}</h2>
            <a routerLink="/appointments/book" class="view-all" *ngIf="isPatient">Book a visit →</a>
            <a routerLink="/appointments" class="view-all" *ngIf="!isPatient">View All →</a>
          </div>
          <div class="empty-state" *ngIf="isPatient">
            <span>📅</span>
            <p>Find a specialist and book your next visit. Your appointment history is not available through the current API yet.</p>
            <a routerLink="/doctors">Explore doctors</a>
          </div>
          <div class="appointments-table" *ngIf="!isPatient && appointments.length > 0; else noAppointments">
            <div class="table-header">
              <span>Doctor</span>
              <span>Date</span>
              <span>Time</span>
              <span>Mode</span>
              <span>Status</span>
            </div>
            <div class="table-row" *ngFor="let appt of appointments.slice(0, 5)">
              <span class="doctor-name">Dr. {{ appt.doctorName }}</span>
              <span>{{ appt.appointmentDate | date: 'MMM d, y' }}</span>
              <span>{{ appt.timeSlot }}</span>
              <span>{{ appt.consultationMode }}</span>
              <span class="status-badge" [class]="'status-' + appt.status?.toLowerCase()">{{ appt.status }}</span>
            </div>
          </div>
          <ng-template #noAppointments>
            <div class="empty-state" *ngIf="!isPatient">
              <span>📅</span>
              <p>No appointments to show yet.</p>
            </div>
          </ng-template>
        </div>
      </main>
    </div>
  `,
  styles: [`
    .dashboard { min-height: 100vh; background: #0d1117; color: #fff; }
    .navbar {
      display: flex; align-items: center; justify-content: space-between;
      padding: 1rem 2rem;
      background: rgba(255,255,255,0.03);
      border-bottom: 1px solid rgba(255,255,255,0.08);
      position: sticky; top: 0; z-index: 100;
      backdrop-filter: blur(10px);
    }
    .nav-brand { display: flex; align-items: center; gap: 0.5rem; }
    .brand-icon { font-size: 1.5rem; }
    .brand-name { font-size: 1.25rem; font-weight: 700; color: #4f8ef7; }
    .nav-links { display: flex; gap: 1.5rem; }
    .nav-links a { color: rgba(255,255,255,0.7); text-decoration: none; font-size: 0.9rem; transition: color 0.2s; }
    .nav-links a:hover, .nav-links a.active { color: #4f8ef7; }
    .nav-user { display: flex; align-items: center; gap: 0.75rem; }
    .user-name { color: rgba(255,255,255,0.9); font-weight: 500; font-size: 0.9rem; }
    .user-badge {
      padding: 0.25rem 0.75rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600;
    }
    .badge-admin { background: rgba(239,68,68,0.2); color: #ef4444; }
    .badge-doctor { background: rgba(79,142,247,0.2); color: #4f8ef7; }
    .badge-patient { background: rgba(16,185,129,0.2); color: #10b981; }
    .btn-logout {
      padding: 0.4rem 0.9rem; background: rgba(239,68,68,0.1); border: 1px solid rgba(239,68,68,0.3);
      color: #ef4444; border-radius: 8px; cursor: pointer; font-size: 0.85rem; transition: all 0.2s;
    }
    .btn-logout:hover { background: rgba(239,68,68,0.2); }
    .main-content { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .welcome-banner {
      display: flex; align-items: center; justify-content: space-between;
      background: linear-gradient(135deg, rgba(79,142,247,0.15), rgba(124,58,237,0.15));
      border: 1px solid rgba(79,142,247,0.2);
      border-radius: 20px; padding: 2rem; margin-bottom: 2rem;
    }
    .welcome-text h1 { font-size: 1.75rem; font-weight: 700; margin: 0 0 0.5rem; }
    .welcome-text p { color: rgba(255,255,255,0.6); margin: 0; }
    .btn-book {
      background: linear-gradient(135deg, #4f8ef7, #7c3aed);
      color: #fff; border: none; border-radius: 12px;
      padding: 0.875rem 1.5rem; font-size: 1rem; font-weight: 600;
      cursor: pointer; transition: transform 0.2s, opacity 0.2s; white-space: nowrap;
    }
    .btn-book:hover { transform: translateY(-2px); opacity: 0.9; }
    .stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 1.5rem; margin-bottom: 2rem; }
    .stat-card {
      border-radius: 16px; padding: 1.5rem;
      display: flex; align-items: center; gap: 1rem;
      border: 1px solid rgba(255,255,255,0.08);
    }
    .stat-blue { background: rgba(79,142,247,0.1); }
    .stat-green { background: rgba(16,185,129,0.1); }
    .stat-orange { background: rgba(245,158,11,0.1); }
    .stat-purple { background: rgba(124,58,237,0.1); }
    .stat-icon { font-size: 2rem; }
    .stat-value { display: block; font-size: 2rem; font-weight: 700; }
    .stat-label { color: rgba(255,255,255,0.6); font-size: 0.875rem; }
    .section { background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.08); border-radius: 16px; padding: 1.5rem; }
    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.25rem; }
    .section-header h2 { margin: 0; font-size: 1.25rem; font-weight: 600; }
    .view-all { color: #4f8ef7; text-decoration: none; font-size: 0.875rem; }
    .table-header, .table-row {
      display: grid; grid-template-columns: 2fr 1.5fr 1fr 1fr 1fr;
      padding: 0.75rem 0; gap: 1rem; align-items: center;
    }
    .table-header { color: rgba(255,255,255,0.4); font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid rgba(255,255,255,0.08); }
    .table-row { border-bottom: 1px solid rgba(255,255,255,0.05); font-size: 0.9rem; }
    .table-row:last-child { border-bottom: none; }
    .doctor-name { font-weight: 500; color: #4f8ef7; }
    .status-badge { padding: 0.25rem 0.75rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600; display: inline-block; }
    .status-pending { background: rgba(245,158,11,0.2); color: #f59e0b; }
    .status-confirmed { background: rgba(16,185,129,0.2); color: #10b981; }
    .status-cancelled { background: rgba(239,68,68,0.2); color: #ef4444; }
    .status-completed { background: rgba(79,142,247,0.2); color: #4f8ef7; }
    .empty-state { text-align: center; padding: 3rem; color: rgba(255,255,255,0.4); }
    .empty-state span { font-size: 3rem; display: block; margin-bottom: 1rem; }
    .empty-state a { color: #4f8ef7; }
    @media (max-width: 768px) {
      .stats-grid { grid-template-columns: repeat(2, 1fr); }
      .nav-links { display: none; }
      .navbar { padding: 1rem; }
    }
  `]
})
export class DashboardComponent implements OnInit {
  currentUser: any = null;
  appointments: AppointmentDto[] = [];
  availableDoctors = 0;
  confirmedCount = 0;
  pendingCount = 0;
  errorMessage = '';

  get isAdmin(): boolean { return this.currentUser?.role === 'Admin'; }
  get isDoctor(): boolean { return this.currentUser?.role === 'Doctor'; }
  get isPatient(): boolean { return this.currentUser?.role === 'Patient'; }

  constructor(
    private authService: AuthService,
    private appointmentService: AppointmentService,
    private doctorService: DoctorService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();

    // Load appointments
    if (!this.isPatient) {
      const doctorId = this.isDoctor ? this.currentUser?.profileId : undefined;
      this.appointmentService.getAll(doctorId ?? undefined).subscribe({
        next: appts => {
          this.appointments = appts;
          this.confirmedCount = appts.filter(a => a.status === 'Confirmed').length;
          this.pendingCount = appts.filter(a => a.status === 'Pending').length;
        },
        error: () => this.errorMessage = 'Appointments could not be loaded. Please try again.'
      });
    }

    // Load doctor count
    this.doctorService.getAll(undefined, undefined, true).subscribe(docs => {
      this.availableDoctors = docs.length;
    }, () => this.errorMessage = 'Doctor availability could not be loaded.');
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
