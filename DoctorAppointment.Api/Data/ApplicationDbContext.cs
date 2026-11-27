using Microsoft.EntityFrameworkCore;
using DoctorAppointment.Backend.Enums;
using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Email Unique Index
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Specialization Name Unique Index
            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasIndex(s => s.Name).IsUnique();
            });

            // Appointment Configuration
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasIndex(a => a.AppointmentNumber).IsUnique();

                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Doctor Configuration
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.Property(d => d.ConsultationFee)
                      .HasPrecision(18, 2);

                entity.HasOne(d => d.User)
                      .WithOne(u => u.Doctor)
                      .HasForeignKey<Doctor>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Specialization)
                      .WithMany(s => s.Doctors)
                      .HasForeignKey(d => d.SpecializationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Patient Configuration
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Patient)
                      .HasForeignKey<Patient>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Doctor Availability Configuration
            modelBuilder.Entity<DoctorAvailability>(entity =>
            {
                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Availabilities)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed Data for Specializations
            modelBuilder.Entity<Specialization>().HasData(
                new Specialization { Id = 1, Name = "General Medicine", Description = "Primary healthcare, preventive care, and adult medicine." },
                new Specialization { Id = 2, Name = "Cardiology", Description = "Diagnosis and treatment of heart and blood vessel disorders." },
                new Specialization { Id = 3, Name = "Dermatology", Description = "Care for conditions affecting the skin, hair, and nails." },
                new Specialization { Id = 4, Name = "Pediatrics", Description = "Medical care for infants, children, and adolescents." },
                new Specialization { Id = 5, Name = "Orthopedics", Description = "Musculoskeletal conditions, bones, joints, ligaments, and tendons." },
                new Specialization { Id = 6, Name = "Neurology", Description = "Disorders of the nervous system, brain, and spinal cord." },
                new Specialization { Id = 7, Name = "Gynecology", Description = "Women's reproductive health, pregnancy, and obstetrics." },
                new Specialization { Id = 8, Name = "Ophthalmology", Description = "Diagnosis and treatment of eye disorders and vision care." }
            );

            // Seed Admin User (password: Admin@123)
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "System Administrator",
                    Email = "admin@doctorappointment.com",
                    PasswordHash = adminPasswordHash,
                    Role = UserRole.Admin,
                    PhoneNumber = "1800-000-0000",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
