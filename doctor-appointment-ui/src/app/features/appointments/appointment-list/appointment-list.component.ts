import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AppointmentDto, AppointmentService } from '../../../core/services/appointment.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-appointment-list',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule, RouterLink],
  template: `
    <section class="page">
      <div class="section-heading">
        <div><span class="eyebrow">{{ isAdmin ? 'CLINIC OPERATIONS' : 'YOUR PRACTICE' }}</span><h1>{{ isAdmin ? 'All appointments' : 'Appointments' }}</h1><p class="muted-copy">Review visits, update their status, and keep care moving.</p></div>
        <a *ngIf="isPatient" class="button button-primary" routerLink="/appointments/book">+ Book a visit</a>
      </div>

      <div class="notice notice-info" *ngIf="isPatient">
        You can book a new appointment here. The current API does not provide a patient appointment-list endpoint, so your existing bookings cannot be displayed yet.
      </div>

      <ng-container *ngIf="!isPatient">
        <section class="panel filter-bar">
          <label class="field"><span>From</span><input type="date" [(ngModel)]="fromDate" /></label>
          <label class="field"><span>To</span><input type="date" [(ngModel)]="toDate" /></label>
          <label class="field"><span>Status</span>
            <select [(ngModel)]="status">
              <option value="">All statuses</option><option>Pending</option><option>Confirmed</option><option>Cancelled</option><option>Completed</option><option>NoShow</option>
            </select>
          </label>
          <button type="button" class="button button-outline" (click)="loadAppointments()">Apply filters</button>
        </section>

        <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
        <section class="panel table-panel">
          <div class="table-wrap" *ngIf="appointments.length; else emptyResults">
            <table class="data-table">
              <thead><tr><th>Patient</th><th>Doctor</th><th>Date & time</th><th>Type</th><th>Status</th><th *ngIf="canManage">Update</th></tr></thead>
              <tbody>
                <tr *ngFor="let appointment of appointments">
                  <td><strong>{{ appointment.patientName }}</strong><small>Ref #{{ appointment.id }}</small></td>
                  <td><strong>Dr. {{ appointment.doctorName }}</strong><small>{{ appointment.specializationName }}</small></td>
                  <td><strong>{{ appointment.appointmentDate | date:'MMM d, y' }}</strong><small>{{ appointment.timeSlot }}</small></td>
                  <td>{{ appointment.consultationMode }}</td>
                  <td><span class="status-pill" [attr.data-status]="appointment.status">{{ appointment.status }}</span></td>
                  <td *ngIf="canManage">
                    <select class="table-select" [value]="appointment.status" (change)="setStatus(appointment, $any($event.target).value)">
                      <option>Pending</option><option>Confirmed</option><option>Completed</option><option>Cancelled</option><option>NoShow</option>
                    </select>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <ng-template #emptyResults>
            <div class="empty-state" *ngIf="loading"><span class="loading-mark"></span><p>Loading appointments…</p></div>
            <div class="empty-state" *ngIf="!loading"><span class="empty-icon">◷</span><h3>No appointments found</h3><p>Try changing your filters.</p></div>
          </ng-template>
        </section>
      </ng-container>
    </section>
  `
})
export class AppointmentListComponent implements OnInit {
  appointments: AppointmentDto[] = [];
  loading = true;
  errorMessage = '';
  fromDate = '';
  toDate = '';
  status = '';
  private readonly user = this.authService.getCurrentUser();

  get isPatient(): boolean { return this.user?.role === 'Patient'; }
  get isAdmin(): boolean { return this.user?.role === 'Admin'; }
  get canManage(): boolean { return this.isAdmin || this.user?.role === 'Doctor'; }

  constructor(private authService: AuthService, private appointmentService: AppointmentService) {}

  ngOnInit(): void {
    if (!this.isPatient) this.loadAppointments();
  }

  loadAppointments(): void {
    this.loading = true;
    this.errorMessage = '';
    const doctorId = this.user?.role === 'Doctor' ? this.user.profileId : undefined;
    this.appointmentService.getAll(doctorId ?? undefined, undefined, this.status || undefined, this.fromDate || undefined, this.toDate || undefined)
      .subscribe({
        next: items => {
          this.appointments = items;
          this.loading = false;
        },
        error: () => {
          this.errorMessage = 'Appointments could not be loaded. Please try again.';
          this.loading = false;
        }
      });
  }

  setStatus(appointment: AppointmentDto, status: string): void {
    if (appointment.status === status) return;
    this.appointmentService.update(appointment.id, { status }).subscribe({
      next: updated => {
        this.appointments = this.appointments.map(item => item.id === updated.id ? updated : item);
      },
      error: () => this.errorMessage = 'The appointment status could not be updated.'
    });
  }
}
