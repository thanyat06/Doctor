import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { DoctorDto, DoctorService } from '../../core/services/doctor.service';
import { PatientDto, PatientService } from '../../core/services/patient.service';
import { SpecializationDto, SpecializationService } from '../../core/services/specialization.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page page-narrow">
      <div class="section-heading">
        <div><span class="eyebrow">ACCOUNT SETTINGS</span><h1>Your profile</h1><p class="muted-copy">Keep your information up to date.</p></div>
      </div>
      <section class="panel profile-card">
        <div class="profile-card-heading">
          <div class="doctor-avatar">{{ initials }}</div>
          <div><h2>{{ user?.name }}</h2><p>{{ user?.email }}</p></div>
          <span class="role-chip">{{ user?.role }}</span>
        </div>
        <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
        <div class="notice notice-success" *ngIf="successMessage">{{ successMessage }}</div>

        <form *ngIf="user?.role === 'Patient'" [formGroup]="patientForm" (ngSubmit)="savePatient()" class="stack-form profile-form">
          <div class="form-grid">
            <label class="field"><span>Full name</span><input formControlName="name" /></label>
            <label class="field"><span>Phone number</span><input formControlName="phoneNumber" /></label>
            <label class="field"><span>Date of birth</span><input type="date" formControlName="dateOfBirth" /></label>
            <label class="field"><span>Blood group</span><input formControlName="bloodGroup" placeholder="e.g. O+" /></label>
            <label class="field"><span>Emergency contact</span><input formControlName="emergencyContact" /></label>
            <label class="field field-span"><span>Address</span><textarea rows="3" formControlName="address"></textarea></label>
          </div>
          <button type="submit" class="button button-primary" [disabled]="patientForm.invalid || saving">{{ saving ? 'Saving…' : 'Save profile' }}</button>
        </form>

        <form *ngIf="user?.role === 'Doctor'" [formGroup]="doctorForm" (ngSubmit)="saveDoctor()" class="stack-form profile-form">
          <div class="form-grid">
            <label class="field"><span>Full name</span><input formControlName="name" /></label>
            <label class="field"><span>Medical license</span><input formControlName="licenseNumber" /></label>
            <label class="field"><span>Specialty</span><select formControlName="specializationId"><option *ngFor="let spec of specializations" [value]="spec.id">{{ spec.name }}</option></select></label>
            <label class="field"><span>Experience (years)</span><input type="number" min="0" formControlName="experienceYears" /></label>
            <label class="field"><span>Consultation fee (₹)</span><input type="number" min="0" formControlName="consultationFee" /></label>
            <label class="check-field"><input type="checkbox" formControlName="isAvailable" /><span>Accepting appointments</span></label>
            <label class="field field-span"><span>Professional bio</span><textarea rows="4" formControlName="bio"></textarea></label>
          </div>
          <button type="submit" class="button button-primary" [disabled]="doctorForm.invalid || saving">{{ saving ? 'Saving…' : 'Save profile' }}</button>
        </form>

        <div class="admin-profile" *ngIf="user?.role === 'Admin'">
          <p>Administrator access is active for this account. Profile details are managed by your system administrator.</p>
        </div>
        <div class="empty-state" *ngIf="loading"><span class="loading-mark"></span><p>Loading your profile…</p></div>
      </section>
    </section>
  `
})
export class ProfileComponent implements OnInit {
  user = this.authService.getCurrentUser();
  patient: PatientDto | null = null;
  doctor: DoctorDto | null = null;
  specializations: SpecializationDto[] = [];
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';
  patientForm = this.fb.group({
    name: ['', Validators.required],
    phoneNumber: [''],
    dateOfBirth: [''],
    bloodGroup: [''],
    address: [''],
    emergencyContact: ['']
  });
  doctorForm = this.fb.group({
    name: ['', Validators.required],
    licenseNumber: ['', Validators.required],
    specializationId: ['', Validators.required],
    experienceYears: [0, [Validators.required, Validators.min(0)]],
    consultationFee: [0, [Validators.required, Validators.min(0)]],
    bio: [''],
    isAvailable: [true]
  });

  get initials(): string {
    return (this.user?.name || 'U').split(/\s+/).slice(0, 2).map(part => part.charAt(0)).join('').toUpperCase();
  }

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private patientService: PatientService,
    private doctorService: DoctorService,
    private specializationService: SpecializationService
  ) {}

  ngOnInit(): void {
    if (this.user?.role === 'Patient') this.loadPatient();
    if (this.user?.role === 'Doctor') this.loadDoctor();
  }

  loadPatient(): void {
    if (!this.user) return;
    this.loading = true;
    this.patientService.getByUserId(this.user.userId).subscribe({
      next: patient => {
        this.patient = patient;
        this.patientForm.patchValue({
          name: patient.name,
          phoneNumber: patient.phoneNumber || '',
          dateOfBirth: patient.dateOfBirth?.slice(0, 10) || '',
          bloodGroup: patient.bloodGroup || '',
          address: patient.address || '',
          emergencyContact: patient.emergencyContact || ''
        });
        this.loading = false;
      },
      error: () => { this.errorMessage = 'Your patient profile could not be loaded.'; this.loading = false; }
    });
  }

  loadDoctor(): void {
    if (!this.user) return;
    this.loading = true;
    this.doctorService.getByUserId(this.user.userId).subscribe({
      next: doctor => {
        this.doctor = doctor;
        this.doctorForm.patchValue({
          name: doctor.name,
          licenseNumber: doctor.licenseNumber,
          specializationId: String(doctor.specializationId),
          experienceYears: doctor.experienceYears,
          consultationFee: doctor.consultationFee,
          bio: doctor.bio || '',
          isAvailable: doctor.isAvailable
        });
        this.loading = false;
      },
      error: () => { this.errorMessage = 'Your doctor profile could not be loaded.'; this.loading = false; }
    });
    this.specializationService.getAll().subscribe({
      next: items => this.specializations = items.filter(item => item.isActive),
      error: () => this.errorMessage = 'Specialties could not be loaded.'
    });
  }

  savePatient(): void {
    if (!this.patient || this.patientForm.invalid) return;
    const value = this.patientForm.getRawValue();
    this.saving = true;
    this.errorMessage = '';
    this.patientService.update(this.patient.id, {
      name: value.name!.trim(),
      phoneNumber: value.phoneNumber || undefined,
      dateOfBirth: value.dateOfBirth ? `${value.dateOfBirth}T00:00:00` : undefined,
      bloodGroup: value.bloodGroup || undefined,
      address: value.address || undefined,
      emergencyContact: value.emergencyContact || undefined
    }).subscribe({
      next: patient => {
        this.patient = patient;
        this.saving = false;
        this.successMessage = 'Your profile has been saved.';
      },
      error: () => { this.errorMessage = 'Your profile could not be saved.'; this.saving = false; }
    });
  }

  saveDoctor(): void {
    if (!this.doctor || this.doctorForm.invalid) return;
    const value = this.doctorForm.getRawValue();
    this.saving = true;
    this.errorMessage = '';
    this.doctorService.update(this.doctor.id, {
      name: value.name!.trim(),
      licenseNumber: value.licenseNumber!,
      specializationId: Number(value.specializationId),
      experienceYears: Number(value.experienceYears),
      consultationFee: Number(value.consultationFee),
      bio: value.bio || undefined,
      isAvailable: !!value.isAvailable
    }).subscribe({
      next: doctor => {
        this.doctor = doctor;
        this.saving = false;
        this.successMessage = 'Your profile has been saved.';
      },
      error: () => { this.errorMessage = 'Your profile could not be saved.'; this.saving = false; }
    });
  }
}
