import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AvailabilityDto {
  id: number;
  doctorId: number;
  doctorName: string;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  slotDurationMinutes: number;
  isActive: boolean;
  availableSlots: string[];
}

export interface CreateAvailabilityDto {
  doctorId: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  slotDurationMinutes: number;
}

export interface UpdateAvailabilityDto {
  startTime?: string;
  endTime?: string;
  slotDurationMinutes?: number;
  isActive?: boolean;
}

@Injectable({ providedIn: 'root' })
export class AvailabilityService {
  private readonly apiUrl = `${environment.apiUrl}/availability`;

  constructor(private http: HttpClient) {}

  getByDoctor(doctorId: number): Observable<AvailabilityDto[]> {
    return this.http.get<AvailabilityDto[]>(`${this.apiUrl}/doctor/${doctorId}`);
  }

  getById(id: number): Observable<AvailabilityDto> {
    return this.http.get<AvailabilityDto>(`${this.apiUrl}/${id}`);
  }

  getAvailableSlots(doctorId: number, date: string): Observable<string[]> {
    let params = new HttpParams().set('doctorId', doctorId.toString()).set('date', date);
    return this.http.get<string[]>(`${this.apiUrl}/slots`, { params });
  }

  create(dto: CreateAvailabilityDto): Observable<AvailabilityDto> {
    return this.http.post<AvailabilityDto>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateAvailabilityDto): Observable<AvailabilityDto> {
    return this.http.put<AvailabilityDto>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
