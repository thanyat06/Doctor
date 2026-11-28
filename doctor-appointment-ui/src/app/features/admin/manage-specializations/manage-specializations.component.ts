import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { SpecializationDto, SpecializationService } from '../../../core/services/specialization.service';

@Component({
  selector: 'app-manage-specializations',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page">
      <div class="section-heading">
        <div><span class="eyebrow">ADMINISTRATION</span><h1>Medical specialties</h1><p class="muted-copy">Organize the care your clinic offers.</p></div>
      </div>
      <div class="admin-layout specialty-layout">
        <section class="panel content-panel">
          <span class="eyebrow">DIRECTORY SETTINGS</span><h2>{{ editing ? 'Edit specialty' : 'Add a specialty' }}</h2>
          <form [formGroup]="form" (ngSubmit)="save()" class="stack-form">
            <label class="field"><span>Name</span><input formControlName="name" placeholder="Cardiology" /></label>
            <label class="field"><span>Description</span><textarea rows="4" formControlName="description" placeholder="Short overview of this specialty"></textarea></label>
            <label class="check-field" *ngIf="editing"><input type="checkbox" formControlName="isActive" /><span>Visible in the doctor directory</span></label>
            <div class="notice notice-error" *ngIf="errorMessage">{{ errorMessage }}</div>
            <div class="form-actions"><button type="submit" class="button button-primary" [disabled]="form.invalid || saving">{{ saving ? 'Saving…' : editing ? 'Save changes' : 'Add specialty' }}</button><button *ngIf="editing" type="button" class="button button-quiet" (click)="resetForm()">Cancel</button></div>
          </form>
        </section>
        <section class="specialty-grid">
          <article class="panel specialty-card" *ngFor="let specialty of specializations">
            <div class="specialty-icon">✚</div>
            <span class="status-pill" [attr.data-status]="specialty.isActive ? 'Confirmed' : 'Cancelled'">{{ specialty.isActive ? 'Active' : 'Hidden' }}</span>
            <h2>{{ specialty.name }}</h2>
            <p>{{ specialty.description || 'No description provided.' }}</p>
            <div class="specialty-meta"><span>{{ specialty.doctorCount }} doctors</span><div><button class="text-button" (click)="edit(specialty)">Edit</button><button class="text-button danger-text" (click)="delete(specialty)">Delete</button></div></div>
          </article>
          <div class="empty-state panel" *ngIf="!specializations.length"><p>{{ loading ? 'Loading specialties…' : 'No specialties added yet.' }}</p></div>
        </section>
      </div>
    </section>
  `
})
export class ManageSpecializationsComponent implements OnInit {
  specializations: SpecializationDto[] = [];
  editing: SpecializationDto | null = null;
  loading = true;
  saving = false;
  errorMessage = '';
  form = this.fb.group({
    name: ['', Validators.required],
    description: [''],
    isActive: [true]
  });

  constructor(private fb: FormBuilder, private specializationService: SpecializationService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.specializationService.getAll().subscribe({
      next: items => { this.specializations = items; this.loading = false; },
      error: () => { this.errorMessage = 'Specialties could not be loaded.'; this.loading = false; }
    });
  }

  edit(item: SpecializationDto): void {
    this.editing = item;
    this.form.patchValue({ name: item.name, description: item.description, isActive: item.isActive });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.errorMessage = '';
    const value = this.form.getRawValue();
    const request = this.editing
      ? this.specializationService.update(this.editing.id, { name: value.name!, description: value.description || '', isActive: !!value.isActive })
      : this.specializationService.create({ name: value.name!, description: value.description || '' });
    request.subscribe({
      next: item => {
        this.specializations = this.editing
          ? this.specializations.map(existing => existing.id === item.id ? item : existing)
          : [item, ...this.specializations];
        this.resetForm();
        this.saving = false;
      },
      error: error => {
        this.errorMessage = error.error?.message || 'Specialty changes could not be saved.';
        this.saving = false;
      }
    });
  }

  delete(item: SpecializationDto): void {
    if (!confirm(`Delete ${item.name}?`)) return;
    this.specializationService.delete(item.id).subscribe({
      next: () => this.specializations = this.specializations.filter(existing => existing.id !== item.id),
      error: () => this.errorMessage = 'This specialty could not be deleted. It may still be assigned to doctors.'
    });
  }

  resetForm(): void {
    this.editing = null;
    this.form.reset({ name: '', description: '', isActive: true });
    this.errorMessage = '';
  }
}
