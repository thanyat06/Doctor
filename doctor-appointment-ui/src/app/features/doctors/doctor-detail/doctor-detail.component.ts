import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AvailabilityDto, AvailabilityService } from '../../../core/services/availability.service';
import { DoctorDto, DoctorService } from '../../../core/services/doctor.service';

@Component({
  selector: 'app-doctor-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <section class="page page-narrow">
      <a class="back-link" routerLink="/doctors">← All doctors</a>
      <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
      <ng-container *ngIf="doctor as item">
        <section class="profile-hero panel">
          <div class="doctor-avatar doctor-avatar-large">{{ initials(item.name) }}</div>
          <div class="profile-hero-copy">
            <span class="doctor-specialty">{{ item.specializationName }}</span>
            <h1>Dr. {{ item.name }}</h1>
            <p>{{ item.experienceYears }} years of experience <span class="dot-separator">·</span> License {{ item.licenseNumber }}</p>
            <span class="availability" [class.unavailable]="!item.isAvailable"><i></i>{{ item.isAvailable ? 'Available for appointments' : 'Not currently accepting appointments' }}</span>
          </div>
          <div class="profile-hero-action">
            <span class="fee-label">Consultation fee</span>
            <strong>₹{{ item.consultationFee }}</strong>
            <a *ngIf="item.isAvailable" class="button button-primary" [routerLink]="['/appointments/book']" [queryParams]="{ doctorId: item.id }">Book appointment</a>
          </div>
        </section>

        <div class="detail-grid">
          <section class="panel content-panel">
            <span class="eyebrow">ABOUT THE DOCTOR</span>
            <h2>Care that starts with listening.</h2>
            <p>{{ item.bio || 'Dr. ' + item.name + ' is committed to providing compassionate, evidence-based care tailored to each patient.' }}</p>
          </section>
          <section class="panel content-panel">
            <span class="eyebrow">PRACTICE HOURS</span>
            <h2>Weekly availability</h2>
            <div class="schedule-list" *ngIf="availability.length; else noSchedule">
              <div class="schedule-row" *ngFor="let slot of availability">
                <strong>{{ slot.dayOfWeek }}</strong><span>{{ slot.startTime }} – {{ slot.endTime }}</span>
                <span class="schedule-duration">{{ slot.slotDurationMinutes }} min</span>
              </div>
            </div>
            <ng-template #noSchedule><p class="muted-copy">Schedule details are not published yet.</p></ng-template>
          </section>
        </div>
      </ng-container>
      <div class="empty-state panel" *ngIf="loading"><span class="loading-mark"></span><p>Loading doctor profile…</p></div>
    </section>
  `
})
export class DoctorDetailComponent implements OnInit {
  doctor: DoctorDto | null = null;
  availability: AvailabilityDto[] = [];
  loading = true;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private doctorService: DoctorService,
    private availabilityService: AvailabilityService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id < 1) {
      this.errorMessage = 'That doctor profile could not be found.';
      this.loading = false;
      return;
    }

    forkJoin({
      doctor: this.doctorService.getById(id),
      availability: this.availabilityService.getByDoctor(id)
    }).subscribe({
      next: result => {
        this.doctor = result.doctor;
        this.availability = result.availability.filter(slot => slot.isActive);
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'We could not load this doctor profile.';
        this.loading = false;
      }
    });
  }

  initials(name: string): string {
    return name.split(/\s+/).slice(0, 2).map(part => part.charAt(0)).join('').toUpperCase();
  }
}
