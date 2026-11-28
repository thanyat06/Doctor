import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DoctorDto {
  id: number;
  userId: number;
  name: string;
  email: string;
  licenseNumber: string;
  specializationId: number;
  specializationName: string;
  experienceYears: number;
  consultationFee: number;
  bio?: string;
  profileImageUrl?: string;
  isAvailable: boolean;
  createdAt: string;
}

export interface CreateDoctorDto {
  name: string;
  email: string;
  password: string;
  licenseNumber: string;
  specializationId: number;
  experienceYears: number;
  consultationFee: number;
  bio?: string;
}

export interface UpdateDoctorDto {
  name?: string;
  licenseNumber?: string;
  specializationId?: number;
  experienceYears?: number;
  consultationFee?: number;
  bio?: string;
  isAvailable?: boolean;
}

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private readonly apiUrl = `${environment.apiUrl}/doctors`;

  constructor(private http: HttpClient) {}

  getAll(search?: string, specializationId?: number, isAvailable?: boolean): Observable<DoctorDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (specializationId) params = params.set('specializationId', specializationId.toString());
    if (isAvailable !== undefined) params = params.set('isAvailable', isAvailable.toString());
    return this.http.get<DoctorDto[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<DoctorDto> {
    return this.http.get<DoctorDto>(`${this.apiUrl}/${id}`);
  }

  getByUserId(userId: number): Observable<DoctorDto> {
    return this.http.get<DoctorDto>(`${this.apiUrl}/by-user/${userId}`);
  }

  create(dto: CreateDoctorDto): Observable<DoctorDto> {
    return this.http.post<DoctorDto>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateDoctorDto): Observable<DoctorDto> {
    return this.http.put<DoctorDto>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
