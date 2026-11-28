import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
      }
    ]
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'doctors',
    children: [
      {
        path: '',
        loadComponent: () => import('./features/doctors/doctor-list/doctor-list.component').then(m => m.DoctorListComponent)
      },
      {
        path: ':id',
        loadComponent: () => import('./features/doctors/doctor-detail/doctor-detail.component').then(m => m.DoctorDetailComponent)
      }
    ]
  },
  {
    path: 'appointments',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/appointments/appointment-list/appointment-list.component').then(m => m.AppointmentListComponent)
      },
      {
        path: 'book',
        canActivate: [roleGuard(['Patient'])],
        loadComponent: () => import('./features/appointments/book-appointment/book-appointment.component').then(m => m.BookAppointmentComponent)
      }
    ]
  },
  {
    path: 'admin',
    canActivate: [roleGuard(['Admin'])],
    children: [
      {
        path: 'doctors',
        loadComponent: () => import('./features/admin/manage-doctors/manage-doctors.component').then(m => m.ManageDoctorsComponent)
      },
      {
        path: 'patients',
        loadComponent: () => import('./features/admin/manage-patients/manage-patients.component').then(m => m.ManagePatientsComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
