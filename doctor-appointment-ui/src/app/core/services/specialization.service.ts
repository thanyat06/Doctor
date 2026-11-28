import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SpecializationDto {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  doctorCount: number;
}

export interface CreateSpecializationDto {
  name: string;
  description: string;
}

export interface UpdateSpecializationDto {
  name?: string;
  description?: string;
  isActive?: boolean;
}

@Injectable({ providedIn: 'root' })
export class SpecializationService {
  private readonly apiUrl = `${environment.apiUrl}/specializations`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<SpecializationDto[]> {
    return this.http.get<SpecializationDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<SpecializationDto> {
    return this.http.get<SpecializationDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateSpecializationDto): Observable<SpecializationDto> {
    return this.http.post<SpecializationDto>(this.apiUrl, dto);
  }

  update(id: number, dto: UpdateSpecializationDto): Observable<SpecializationDto> {
    return this.http.put<SpecializationDto>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
