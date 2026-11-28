import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DoctorDto, DoctorService } from '../../../core/services/doctor.service';
import { SpecializationDto, SpecializationService } from '../../../core/services/specialization.service';

@Component({
  selector: 'app-doctor-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <section class="page">
      <div class="page-hero">
        <div>
          <span class="eyebrow">PERSONALIZED CARE, MADE SIMPLE</span>
          <h1>Find the right doctor<br><span class="text-accent">for you.</span></h1>
          <p>Browse trusted specialists and book an appointment that fits your schedule.</p>
        </div>
        <div class="hero-illustration" aria-hidden="true"><span>✚</span><i></i><b></b></div>
      </div>

      <section class="search-panel panel">
        <label class="search-field">
          <span aria-hidden="true">⌕</span>
          <input [(ngModel)]="search" (ngModelChange)="loadDoctors()" placeholder="Doctor name or specialty" aria-label="Search doctors" />
        </label>
        <label class="filter-field">
          <span>Specialty</span>
          <select [(ngModel)]="specializationId" (ngModelChange)="loadDoctors()">
            <option value="">All specialties</option>
            <option *ngFor="let item of specializations" [value]="item.id">{{ item.name }}</option>
          </select>
        </label>
      </section>

      <div class="section-heading">
        <div><span class="eyebrow">OUR SPECIALISTS</span><h2>Meet our doctors</h2></div>
        <span class="result-count">{{ doctors.length }} {{ doctors.length === 1 ? 'doctor' : 'doctors' }}</span>
      </div>

      <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
      <div class="doctor-grid" *ngIf="!loading && doctors.length; else loadingOrEmpty">
        <article class="doctor-card panel" *ngFor="let doctor of doctors">
          <div class="doctor-card-top">
            <div class="doctor-avatar">{{ initials(doctor.name) }}</div>
            <span class="availability" [class.unavailable]="!doctor.isAvailable">
              <i></i>{{ doctor.isAvailable ? 'Accepting patients' : 'Unavailable' }}
            </span>
          </div>
          <div class="doctor-card-body">
            <span class="doctor-specialty">{{ doctor.specializationName }}</span>
            <h3>Dr. {{ doctor.name }}</h3>
            <p class="doctor-experience">{{ doctor.experienceYears }} years experience</p>
            <p class="doctor-bio">{{ doctor.bio || 'Dedicated to providing thoughtful, patient-centered care.' }}</p>
          </div>
          <div class="doctor-card-footer">
            <div><span class="fee-label">Consultation</span><strong>₹{{ doctor.consultationFee }}</strong></div>
            <a class="button button-outline button-small" [routerLink]="['/doctors', doctor.id]">View profile <span>→</span></a>
          </div>
        </article>
      </div>
      <ng-template #loadingOrEmpty>
        <div class="empty-state panel" *ngIf="loading"><span class="loading-mark"></span><p>Finding available doctors…</p></div>
        <div class="empty-state panel" *ngIf="!loading && !doctors.length"><span class="empty-icon">⌕</span><h3>No doctors found</h3><p>Try a different name or specialty.</p></div>
      </ng-template>
    </section>
  `
})
export class DoctorListComponent implements OnInit {
  doctors: DoctorDto[] = [];
  specializations: SpecializationDto[] = [];
  search = '';
  specializationId = '';
  loading = true;
  errorMessage = '';
  private searchTimer?: ReturnType<typeof setTimeout>;

  constructor(private doctorService: DoctorService, private specializationService: SpecializationService) {}

  ngOnInit(): void {
    this.specializationService.getAll().subscribe({
      next: items => this.specializations = items.filter(item => item.isActive),
      error: () => this.errorMessage = 'Specialties are temporarily unavailable.'
    });
    this.loadDoctors();
  }

  loadDoctors(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.loading = true;
      this.errorMessage = '';
      this.doctorService.getAll(
        this.search.trim() || undefined,
        this.specializationId ? Number(this.specializationId) : undefined
      ).subscribe({
        next: items => {
          this.doctors = items;
          this.loading = false;
        },
        error: () => {
          this.errorMessage = 'We could not load doctors. Please try again.';
          this.loading = false;
        }
      });
    }, 180);
  }

  initials(name: string): string {
    return name.split(/\s+/).slice(0, 2).map(part => part.charAt(0)).join('').toUpperCase();
  }
}
