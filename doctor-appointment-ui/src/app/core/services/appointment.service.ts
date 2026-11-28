import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AppointmentDto {
  id: number;
  doctorId: number;
  doctorName: string;
  specializationName: string;
  patientId: number;
  patientName: string;
  appointmentDate: string;
  timeSlot: string;
  status: string;
  consultationMode: string;
  reason?: string;
  notes?: string;
  createdAt: string;
}

export interface BookAppointmentDto {
  doctorId: number;
  appointmentDate: string;
  timeSlot: string;
  consultationMode: string;
  reason?: string;
}

export interface UpdateAppointmentDto {
  appointmentDate?: string;
  timeSlot?: string;
  consultationMode?: string;
  reason?: string;
  notes?: string;
  status?: string;
}

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly apiUrl = `${environment.apiUrl}/appointments`;

  constructor(private http: HttpClient) {}

  getAll(doctorId?: number, patientId?: number, status?: string, fromDate?: string, toDate?: string): Observable<AppointmentDto[]> {
    let params = new HttpParams();
    if (doctorId) params = params.set('doctorId', doctorId.toString());
    if (patientId) params = params.set('patientId', patientId.toString());
    if (status) params = params.set('status', status);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<AppointmentDto[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<AppointmentDto> {
    return this.http.get<AppointmentDto>(`${this.apiUrl}/${id}`);
  }

  book(dto: BookAppointmentDto): Observable<AppointmentDto> {
    return this.http.post<AppointmentDto>(`${this.apiUrl}/book`, dto);
  }

  update(id: number, dto: UpdateAppointmentDto): Observable<AppointmentDto> {
    return this.http.put<AppointmentDto>(`${this.apiUrl}/${id}`, dto);
  }

  cancel(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/cancel`, {});
  }
}
