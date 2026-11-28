import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AppointmentService } from '../../../core/services/appointment.service';
import { AvailabilityService } from '../../../core/services/availability.service';
import { DoctorDto, DoctorService } from '../../../core/services/doctor.service';

@Component({
  selector: 'app-book-appointment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <section class="page page-narrow">
      <a class="back-link" routerLink="/doctors">← Browse doctors</a>
      <div class="section-heading">
        <div><span class="eyebrow">YOUR HEALTH, YOUR TIME</span><h1>Book an appointment</h1><p class="muted-copy">Choose a doctor and an available time that works for you.</p></div>
      </div>
      <form class="panel booking-form" [formGroup]="form" (ngSubmit)="submit()">
        <div class="form-grid">
          <label class="field field-span">
            <span>Choose a doctor</span>
            <select formControlName="doctorId" (change)="onDoctorChange()">
              <option value="">Select a doctor</option>
              <option *ngFor="let doctor of doctors" [value]="doctor.id">Dr. {{ doctor.name }} · {{ doctor.specializationName }}</option>
            </select>
          </label>
          <label class="field">
            <span>Appointment date</span>
            <input type="date" formControlName="date" [min]="minDate" (change)="loadSlots()" />
          </label>
          <label class="field">
            <span>Consultation type</span>
            <select formControlName="consultationMode">
              <option value="InPerson">In person</option><option value="Video">Video consultation</option><option value="Phone">Phone call</option>
            </select>
          </label>
        </div>
        <div class="slot-section">
          <div class="field-title">Available times</div>
          <p class="muted-copy" *ngIf="!selectedDoctorId || !form.controls.date.value">Choose a doctor and date to see available times.</p>
          <p class="muted-copy" *ngIf="slotsLoading">Checking available times…</p>
          <p class="muted-copy" *ngIf="!slotsLoading && selectedDoctorId && form.controls.date.value && !slots.length">No open slots on this date. Try another day.</p>
          <div class="slot-list" *ngIf="slots.length">
            <button type="button" class="slot-option" *ngFor="let slot of slots" [class.selected]="form.controls.timeSlot.value === slot" (click)="form.controls.timeSlot.setValue(slot)">{{ slot }}</button>
          </div>
        </div>
        <label class="field">
          <span>What brings you in? <small>(optional)</small></span>
          <textarea rows="3" formControlName="reason" placeholder="Share a brief reason for your visit"></textarea>
        </label>
        <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
        <div class="booking-footer">
          <span class="muted-copy">Your appointment will be pending until confirmed by the clinic.</span>
          <button type="submit" class="button button-primary" [disabled]="form.invalid || submitting || !slots.includes(form.controls.timeSlot.value || '')">
            {{ submitting ? 'Booking…' : 'Confirm appointment' }}
          </button>
        </div>
      </form>
    </section>
  `
})
export class BookAppointmentComponent implements OnInit {
  doctors: DoctorDto[] = [];
  slots: string[] = [];
  slotsLoading = false;
  submitting = false;
  errorMessage = '';
  minDate = new Date().toISOString().slice(0, 10);
  form = this.fb.group({
    doctorId: ['', Validators.required],
    date: ['', Validators.required],
    timeSlot: ['', Validators.required],
    consultationMode: ['InPerson', Validators.required],
    reason: ['']
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private doctorService: DoctorService,
    private availabilityService: AvailabilityService,
    private appointmentService: AppointmentService
  ) {}

  get selectedDoctorId(): number | null {
    const id = Number(this.form.controls.doctorId.value);
    return Number.isInteger(id) && id > 0 ? id : null;
  }

  ngOnInit(): void {
    this.doctorService.getAll(undefined, undefined, true).subscribe({
      next: items => {
        this.doctors = items;
        const queryDoctorId = Number(this.route.snapshot.queryParamMap.get('doctorId'));
        if (items.some(item => item.id === queryDoctorId)) {
          this.form.controls.doctorId.setValue(String(queryDoctorId));
        }
      },
      error: () => this.errorMessage = 'We could not load doctors. Please try again.'
    });
  }

  onDoctorChange(): void {
    this.form.controls.timeSlot.setValue('');
    this.loadSlots();
  }

  loadSlots(): void {
    const doctorId = this.selectedDoctorId;
    const date = this.form.controls.date.value;
    this.slots = [];
    this.form.controls.timeSlot.setValue('');
    if (!doctorId || !date) return;
    this.slotsLoading = true;
    this.availabilityService.getAvailableSlots(doctorId, date).subscribe({
      next: slots => {
        this.slots = slots;
        this.slotsLoading = false;
      },
      error: () => {
        this.errorMessage = 'Available times could not be loaded. Please try again.';
        this.slotsLoading = false;
      }
    });
  }

  submit(): void {
    if (this.form.invalid || !this.selectedDoctorId) return;
    const value = this.form.getRawValue();
    this.submitting = true;
    this.errorMessage = '';
    this.appointmentService.book({
      doctorId: this.selectedDoctorId,
      appointmentDate: `${value.date}T00:00:00`,
      timeSlot: value.timeSlot!,
      consultationMode: value.consultationMode!,
      reason: value.reason || undefined
    }).subscribe({
      next: () => {
        this.submitting = false;
        this.router.navigate(['/appointments']);
      },
      error: error => {
        this.errorMessage = error.error?.message || 'We could not book this appointment. Please choose another time.';
        this.submitting = false;
        this.loadSlots();
      }
    });
  }
}
