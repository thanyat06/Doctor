import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <header class="site-header" *ngIf="showHeader">
      <a class="brand" routerLink="/doctors" aria-label="Carepoint home">
        <span class="brand-mark">+</span>
        <span>carepoint<span class="brand-period">.</span></span>
      </a>

      <nav class="site-nav" aria-label="Main navigation">
        <a routerLink="/doctors" routerLinkActive="active">Find a doctor</a>
        <a *ngIf="user" routerLink="/dashboard" routerLinkActive="active">Overview</a>
        <a *ngIf="user?.role === 'Patient'" routerLink="/appointments/book" routerLinkActive="active">Book visit</a>
        <a *ngIf="user" routerLink="/appointments" routerLinkActive="active">Appointments</a>
        <a *ngIf="user?.role === 'Doctor'" routerLink="/availability" routerLinkActive="active">Availability</a>
        <ng-container *ngIf="user?.role === 'Admin'">
          <a routerLink="/admin/doctors" routerLinkActive="active">Doctors</a>
          <a routerLink="/admin/patients" routerLinkActive="active">Patients</a>
          <a routerLink="/admin/specializations" routerLinkActive="active">Specialties</a>
        </ng-container>
      </nav>

      <div class="header-actions">
        <ng-container *ngIf="user; else signedOut">
          <a class="user-link" routerLink="/profile">
            <span class="user-avatar">{{ user.name.charAt(0) }}</span>
            <span class="user-label">{{ user.name }}</span>
          </a>
          <button class="button button-quiet button-small" type="button" (click)="logout()">Sign out</button>
        </ng-container>
        <ng-template #signedOut>
          <a class="button button-quiet button-small" routerLink="/auth/login">Sign in</a>
          <a class="button button-primary button-small" routerLink="/auth/register">Get started</a>
        </ng-template>
      </div>
    </header>
    <main>
      <router-outlet></router-outlet>
    </main>
  `
})
export class App {
  user = this.authService.getCurrentUser();
  showHeader = true;

  constructor(private authService: AuthService, private router: Router) {
    this.updateHeader(this.router.url);
    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe(event => {
        this.user = this.authService.getCurrentUser();
        this.updateHeader(event.urlAfterRedirects);
      });
  }

  logout(): void {
    this.authService.logout();
    this.user = null;
    this.router.navigate(['/auth/login']);
  }

  private updateHeader(url: string): void {
    this.showHeader = !url.startsWith('/auth') && !url.startsWith('/dashboard');
  }
}
