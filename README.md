# Doctor Appointment System

Doctor appointment management API with an Angular frontend for patients, doctors, and administrators.

## Frontend

The Angular application is in [`doctor-appointment-ui`](./doctor-appointment-ui).

```powershell
cd doctor-appointment-ui
npm install
npm start
```

Open `http://localhost:4200`. The app expects the API at `http://localhost:5000/api`; update `src/environments/environment.ts` if your API uses another address.

### User areas

- **Patients:** create an account, browse doctors, book an available time, and edit their profile.
- **Doctors:** review appointments, update appointment status, edit their profile, and manage weekly availability.
- **Administrators:** manage doctor and patient accounts, medical specialties, availability, and appointments.
- **Public:** browse doctor profiles and specialties without signing in.

The current API does not expose a patient-scoped appointment-list endpoint, so the patient appointment history screen explains that limitation. Patient bookings still use the booking endpoint.
