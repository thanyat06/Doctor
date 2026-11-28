import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PatientDto, PatientService } from '../../../core/services/patient.service';

@Component({
  selector: 'app-manage-patients',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule],
  template: `
    <section class="page">
      <div class="section-heading">
        <div><span class="eyebrow">ADMINISTRATION</span><h1>Patient directory</h1><p class="muted-copy">Review patient records and maintain accurate contact information.</p></div>
      </div>
      <section class="panel filter-bar">
        <label class="search-field patient-search"><span aria-hidden="true">⌕</span><input [(ngModel)]="search" (ngModelChange)="searchPatients()" placeholder="Search by name, email or phone" aria-label="Search patients" /></label>
        <span class="result-count">{{ patients.length }} patients</span>
      </section>
      <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
      <section class="panel table-panel">
        <div class="table-wrap" *ngIf="patients.length; else emptyPatients">
          <table class="data-table">
            <thead><tr><th>Patient</th><th>Contact</th><th>Date of birth</th><th>Blood group</th><th>Joined</th><th></th></tr></thead>
            <tbody>
              <ng-container *ngFor="let patient of patients">
                <tr>
                  <td><strong>{{ patient.name }}</strong><small>Patient #{{ patient.id }}</small></td>
                  <td><strong>{{ patient.email }}</strong><small>{{ patient.phoneNumber || 'No phone provided' }}</small></td>
                  <td>{{ patient.dateOfBirth ? (patient.dateOfBirth | date:'MMM d, y') : '—' }}</td>
                  <td>{{ patient.bloodGroup || '—' }}</td>
                  <td>{{ patient.createdAt | date:'MMM d, y' }}</td>
                  <td><button class="text-button" (click)="toggleEdit(patient)">{{ editingId === patient.id ? 'Close' : 'Edit' }}</button><button class="text-button danger-text" (click)="deletePatient(patient)">Remove</button></td>
                </tr>
                <tr *ngIf="editingId === patient.id" class="edit-row">
                  <td colspan="6">
                    <div class="edit-grid">
                      <label class="field"><span>Name</span><input [(ngModel)]="editValue.name" /></label>
                      <label class="field"><span>Phone</span><input [(ngModel)]="editValue.phoneNumber" /></label>
                      <label class="field"><span>Date of birth</span><input type="date" [(ngModel)]="editValue.dateOfBirth" /></label>
                      <label class="field"><span>Blood group</span><input [(ngModel)]="editValue.bloodGroup" placeholder="e.g. O+" /></label>
                      <label class="field"><span>Emergency contact</span><input [(ngModel)]="editValue.emergencyContact" /></label>
                      <label class="field"><span>Address</span><input [(ngModel)]="editValue.address" /></label>
                      <button class="button button-primary button-small" (click)="savePatient(patient)">Save changes</button>
                    </div>
                  </td>
                </tr>
              </ng-container>
            </tbody>
          </table>
        </div>
        <ng-template #emptyPatients><div class="empty-state"><span class="empty-icon">♡</span><h3>{{ loading ? 'Loading patients…' : 'No patients found' }}</h3><p *ngIf="!loading">Try a different search.</p></div></ng-template>
      </section>
    </section>
  `
})
export class ManagePatientsComponent implements OnInit {
  patients: PatientDto[] = [];
  loading = true;
  errorMessage = '';
  search = '';
  editingId: number | null = null;
  editValue: { name: string; phoneNumber: string; dateOfBirth: string; bloodGroup: string; address: string; emergencyContact: string } = this.emptyEdit();
  private searchTimer?: ReturnType<typeof setTimeout>;

  constructor(private patientService: PatientService) {}

  ngOnInit(): void {
    this.loadPatients();
  }

  searchPatients(): void {
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.loadPatients(), 200);
  }

  loadPatients(): void {
    this.loading = true;
    this.patientService.getAll(this.search.trim() || undefined).subscribe({
      next: items => { this.patients = items; this.loading = false; },
      error: () => { this.errorMessage = 'Patient records could not be loaded.'; this.loading = false; }
    });
  }

  toggleEdit(patient: PatientDto): void {
    if (this.editingId === patient.id) {
      this.editingId = null;
      return;
    }
    this.editingId = patient.id;
    this.editValue = {
      name: patient.name,
      phoneNumber: patient.phoneNumber || '',
      dateOfBirth: patient.dateOfBirth?.slice(0, 10) || '',
      bloodGroup: patient.bloodGroup || '',
      address: patient.address || '',
      emergencyContact: patient.emergencyContact || ''
    };
  }

  savePatient(patient: PatientDto): void {
    if (!this.editValue.name.trim()) {
      this.errorMessage = 'Patient name is required.';
      return;
    }
    this.patientService.update(patient.id, {
      name: this.editValue.name.trim(),
      phoneNumber: this.editValue.phoneNumber || undefined,
      dateOfBirth: this.editValue.dateOfBirth ? `${this.editValue.dateOfBirth}T00:00:00` : undefined,
      bloodGroup: this.editValue.bloodGroup || undefined,
      address: this.editValue.address || undefined,
      emergencyContact: this.editValue.emergencyContact || undefined
    }).subscribe({
      next: updated => {
        this.patients = this.patients.map(item => item.id === updated.id ? updated : item);
        this.editingId = null;
        this.errorMessage = '';
      },
      error: () => this.errorMessage = 'Patient details could not be saved.'
    });
  }

  deletePatient(patient: PatientDto): void {
    if (!confirm(`Remove ${patient.name}'s patient account?`)) return;
    this.patientService.delete(patient.id).subscribe({
      next: () => this.patients = this.patients.filter(item => item.id !== patient.id),
      error: () => this.errorMessage = 'The patient account could not be removed.'
    });
  }

  private emptyEdit(): { name: string; phoneNumber: string; dateOfBirth: string; bloodGroup: string; address: string; emergencyContact: string } {
    return { name: '', phoneNumber: '', dateOfBirth: '', bloodGroup: '', address: '', emergencyContact: '' };
  }
}
