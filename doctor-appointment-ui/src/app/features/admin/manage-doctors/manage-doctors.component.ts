import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DoctorDto, DoctorService } from '../../../core/services/doctor.service';
import { SpecializationDto, SpecializationService } from '../../../core/services/specialization.service';

@Component({
  selector: 'app-manage-doctors',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page">
      <div class="section-heading">
        <div><span class="eyebrow">ADMINISTRATION</span><h1>Doctor directory</h1><p class="muted-copy">Manage clinician accounts and their availability.</p></div>
      </div>
      <div class="admin-layout">
        <section class="panel content-panel">
          <span class="eyebrow">NEW CLINICIAN</span><h2>Add a doctor</h2>
          <form [formGroup]="form" (ngSubmit)="createDoctor()" class="stack-form">
            <label class="field"><span>Full name</span><input formControlName="name" placeholder="Dr. Jane Smith" /></label>
            <label class="field"><span>Email</span><input type="email" formControlName="email" placeholder="doctor@clinic.com" /></label>
            <label class="field"><span>Temporary password</span><input type="password" formControlName="password" /></label>
            <label class="field"><span>Medical license</span><input formControlName="licenseNumber" /></label>
            <label class="field"><span>Specialty</span>
              <select formControlName="specializationId"><option value="">Select specialty</option><option *ngFor="let spec of specializations" [value]="spec.id">{{ spec.name }}</option></select>
            </label>
            <div class="form-grid">
              <label class="field"><span>Experience (years)</span><input type="number" min="0" formControlName="experienceYears" /></label>
              <label class="field"><span>Consultation fee (₹)</span><input type="number" min="0" formControlName="consultationFee" /></label>
            </div>
            <label class="field"><span>Profile bio</span><textarea rows="3" formControlName="bio"></textarea></label>
            <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
            <button type="submit" class="button button-primary" [disabled]="form.invalid || saving">{{ saving ? 'Creating…' : 'Create doctor account' }}</button>
          </form>
        </section>

        <section class="panel table-panel">
          <div class="panel-heading"><div><span class="eyebrow">CLINIC TEAM</span><h2>Doctors <span class="count-chip">{{ doctors.length }}</span></h2></div><button class="button button-outline button-small" (click)="loadDoctors()">Refresh</button></div>
          <div class="table-wrap" *ngIf="doctors.length; else noDoctors">
            <table class="data-table">
              <thead><tr><th>Doctor</th><th>Specialty</th><th>Fee</th><th>Availability</th><th>Action</th></tr></thead>
              <tbody><tr *ngFor="let doctor of doctors">
                <td><strong>Dr. {{ doctor.name }}</strong><small>{{ doctor.email }}</small></td>
                <td>{{ doctor.specializationName }}</td><td>₹{{ doctor.consultationFee }}</td>
                <td><span class="status-pill" [attr.data-status]="doctor.isAvailable ? 'Confirmed' : 'Cancelled'">{{ doctor.isAvailable ? 'Available' : 'Paused' }}</span></td>
                <td><button class="text-button" (click)="toggleAvailability(doctor)">{{ doctor.isAvailable ? 'Pause' : 'Enable' }}</button><button class="text-button danger-text" (click)="deleteDoctor(doctor)">Remove</button></td>
              </tr></tbody>
            </table>
          </div>
          <ng-template #noDoctors><div class="empty-state"><p>{{ loading ? 'Loading doctors…' : 'No doctors have been added yet.' }}</p></div></ng-template>
        </section>
      </div>
    </section>
  `
})
export class ManageDoctorsComponent implements OnInit {
  doctors: DoctorDto[] = [];
  specializations: SpecializationDto[] = [];
  loading = true;
  saving = false;
  errorMessage = '';
  form = this.fb.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    licenseNumber: ['', Validators.required],
    specializationId: ['', Validators.required],
    experienceYears: [0, [Validators.required, Validators.min(0)]],
    consultationFee: [0, [Validators.required, Validators.min(0)]],
    bio: ['']
  });

  constructor(private fb: FormBuilder, private doctorService: DoctorService, private specializationService: SpecializationService) {}

  ngOnInit(): void {
    this.loadDoctors();
    this.specializationService.getAll().subscribe({
      next: items => this.specializations = items.filter(item => item.isActive),
      error: () => this.errorMessage = 'Specialties could not be loaded.'
    });
  }

  loadDoctors(): void {
    this.loading = true;
    this.doctorService.getAll().subscribe({
      next: items => { this.doctors = items; this.loading = false; },
      error: () => { this.errorMessage = 'Doctors could not be loaded.'; this.loading = false; }
    });
  }

  createDoctor(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.errorMessage = '';
    const value = this.form.getRawValue();
    this.doctorService.create({
      name: value.name!,
      email: value.email!,
      password: value.password!,
      licenseNumber: value.licenseNumber!,
      specializationId: Number(value.specializationId),
      experienceYears: Number(value.experienceYears),
      consultationFee: Number(value.consultationFee),
      bio: value.bio || undefined
    }).subscribe({
      next: doctor => {
        this.doctors = [doctor, ...this.doctors];
        this.form.reset({ experienceYears: 0, consultationFee: 0 });
        this.saving = false;
      },
      error: error => {
        this.errorMessage = error.error?.message || 'The doctor account could not be created.';
        this.saving = false;
      }
    });
  }

  toggleAvailability(doctor: DoctorDto): void {
    this.doctorService.update(doctor.id, { isAvailable: !doctor.isAvailable }).subscribe({
      next: updated => this.doctors = this.doctors.map(item => item.id === updated.id ? updated : item),
      error: () => this.errorMessage = 'Availability could not be changed.'
    });
  }

  deleteDoctor(doctor: DoctorDto): void {
    if (!confirm(`Remove Dr. ${doctor.name} from the directory?`)) return;
    this.doctorService.delete(doctor.id).subscribe({
      next: () => this.doctors = this.doctors.filter(item => item.id !== doctor.id),
      error: () => this.errorMessage = 'The doctor could not be removed.'
    });
  }
}
