import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { AvailabilityDto, AvailabilityService } from '../../core/services/availability.service';
import { DoctorDto, DoctorService } from '../../core/services/doctor.service';

@Component({
  selector: 'app-availability',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page">
      <div class="section-heading">
        <div><span class="eyebrow">SCHEDULE MANAGEMENT</span><h1>Availability</h1><p class="muted-copy">Set your recurring clinic hours so patients can find a time that works.</p></div>
      </div>
      <label class="field doctor-picker" *ngIf="isAdmin"><span>Manage doctor</span>
        <select [value]="selectedDoctorId || ''" (change)="selectDoctor($any($event.target).value)"><option value="">Select a doctor</option><option *ngFor="let item of doctors" [value]="item.id">Dr. {{ item.name }}</option></select>
      </label>
      <div class="admin-layout availability-layout">
        <section class="panel content-panel">
          <span class="eyebrow">WEEKLY HOURS</span><h2>Add availability</h2>
          <form [formGroup]="form" (ngSubmit)="create()" class="stack-form">
            <label class="field"><span>Day of week</span><select formControlName="dayOfWeek"><option *ngFor="let day of days; let i = index" [value]="i">{{ day }}</option></select></label>
            <div class="form-grid">
              <label class="field"><span>Starts at</span><input type="time" formControlName="startTime" /></label>
              <label class="field"><span>Ends at</span><input type="time" formControlName="endTime" /></label>
            </div>
            <label class="field"><span>Appointment length</span><select formControlName="slotDurationMinutes"><option [value]="15">15 minutes</option><option [value]="30">30 minutes</option><option [value]="45">45 minutes</option><option [value]="60">60 minutes</option></select></label>
            <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
            <button type="submit" class="button button-primary" [disabled]="form.invalid || saving || !selectedDoctorId">{{ saving ? 'Adding…' : 'Add weekly hours' }}</button>
          </form>
        </section>
        <section class="panel content-panel">
          <span class="eyebrow">CURRENT SCHEDULE</span><h2>Your weekly hours</h2>
          <div class="availability-list" *ngIf="availability.length; else noAvailability">
            <article class="availability-row" *ngFor="let item of availability">
              <div><span class="day-tag">{{ item.dayOfWeek }}</span><strong>{{ item.startTime }} – {{ item.endTime }}</strong><small>{{ item.slotDurationMinutes }} minute visits</small></div>
              <div class="availability-actions"><span class="status-pill" [attr.data-status]="item.isActive ? 'Confirmed' : 'Cancelled'">{{ item.isActive ? 'Active' : 'Paused' }}</span><button class="text-button" (click)="toggle(item)">{{ item.isActive ? 'Pause' : 'Enable' }}</button><button class="text-button danger-text" (click)="remove(item)">Remove</button></div>
            </article>
          </div>
          <ng-template #noAvailability><div class="empty-state"><span class="empty-icon">◷</span><p>{{ loading ? 'Loading schedule…' : 'No weekly hours configured yet.' }}</p></div></ng-template>
        </section>
      </div>
    </section>
  `
})
export class AvailabilityComponent implements OnInit {
  readonly days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
  readonly isAdmin = this.authService.hasRole('Admin');
  doctors: DoctorDto[] = [];
  availability: AvailabilityDto[] = [];
  selectedDoctorId: number | null = this.authService.getCurrentUser()?.profileId || null;
  loading = true;
  saving = false;
  errorMessage = '';
  form = this.fb.group({
    dayOfWeek: [1, Validators.required],
    startTime: ['09:00', Validators.required],
    endTime: ['17:00', Validators.required],
    slotDurationMinutes: [30, [Validators.required, Validators.min(1)]]
  });

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private doctorService: DoctorService,
    private availabilityService: AvailabilityService
  ) {}

  ngOnInit(): void {
    if (this.isAdmin) {
      this.doctorService.getAll().subscribe({
        next: items => this.doctors = items,
        error: () => this.errorMessage = 'Doctor accounts could not be loaded.'
      });
    }
    if (this.selectedDoctorId) this.load();
  }

  selectDoctor(id: string): void {
    this.selectedDoctorId = id ? Number(id) : null;
    this.availability = [];
    if (this.selectedDoctorId) this.load();
  }

  load(): void {
    if (!this.selectedDoctorId) return;
    this.loading = true;
    this.availabilityService.getByDoctor(this.selectedDoctorId).subscribe({
      next: items => { this.availability = items; this.loading = false; },
      error: () => { this.errorMessage = 'Availability could not be loaded.'; this.loading = false; }
    });
  }

  create(): void {
    if (this.form.invalid || !this.selectedDoctorId) return;
    const value = this.form.getRawValue();
    this.saving = true;
    this.errorMessage = '';
    this.availabilityService.create({
      doctorId: this.selectedDoctorId,
      dayOfWeek: Number(value.dayOfWeek),
      startTime: value.startTime!,
      endTime: value.endTime!,
      slotDurationMinutes: Number(value.slotDurationMinutes)
    }).subscribe({
      next: item => { this.availability = [...this.availability, item]; this.saving = false; },
      error: error => { this.errorMessage = error.error?.message || 'Weekly hours could not be added.'; this.saving = false; }
    });
  }

  toggle(item: AvailabilityDto): void {
    this.availabilityService.update(item.id, { isActive: !item.isActive }).subscribe({
      next: updated => this.availability = this.availability.map(existing => existing.id === updated.id ? updated : existing),
      error: () => this.errorMessage = 'Availability could not be updated.'
    });
  }

  remove(item: AvailabilityDto): void {
    if (!confirm(`Remove ${item.dayOfWeek} availability?`)) return;
    this.availabilityService.delete(item.id).subscribe({
      next: () => this.availability = this.availability.filter(existing => existing.id !== item.id),
      error: () => this.errorMessage = 'Availability could not be removed.'
    });
  }
}
