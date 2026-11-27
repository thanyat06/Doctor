import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PatientDto {
  id: number;
  userId: number;
  name: string;
  email: string;
  phoneNumber: string;
  dateOfBirth?: string;
  bloodGroup?: string;
  address?: string;
  emergencyContact?: string;
  createdAt: string;
}

export interface UpdatePatientDto {
  name?: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  bloodGroup?: string;
  address?: string;
  emergencyContact?: string;
}

@Injectable({ providedIn: 'root' })
export class PatientService {
  private readonly apiUrl = `${environment.apiUrl}/patients`;

  constructor(private http: HttpClient) {}

  getAll(search?: string): Observable<PatientDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<PatientDto[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<PatientDto> {
    return this.http.get<PatientDto>(`${this.apiUrl}/${id}`);
  }

  getByUserId(userId: number): Observable<PatientDto> {
    return this.http.get<PatientDto>(`${this.apiUrl}/by-user/${userId}`);
  }

  update(id: number, dto: UpdatePatientDto): Observable<PatientDto> {
    return this.http.put<PatientDto>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
