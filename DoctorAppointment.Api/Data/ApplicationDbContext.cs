using DoctorAppointment.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointment.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
            });

            // Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.ConsultationFee).HasColumnType("decimal(10,2)");
                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(d => d.Specialization)
                      .WithMany(s => s.Doctors)
                      .HasForeignKey(d => d.SpecializationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DoctorAvailability
            modelBuilder.Entity<DoctorAvailability>(entity =>
            {
                entity.HasKey(da => da.Id);
                entity.HasOne(da => da.Doctor)
                      .WithMany(d => d.Availabilities)
                      .HasForeignKey(da => da.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Specializations
            modelBuilder.Entity<Specialization>().HasData(
                new Specialization { Id = 1, Name = "Cardiology", Description = "Heart and cardiovascular system" },
                new Specialization { Id = 2, Name = "Neurology", Description = "Brain and nervous system" },
                new Specialization { Id = 3, Name = "Orthopedics", Description = "Bones, joints and muscles" },
                new Specialization { Id = 4, Name = "Dermatology", Description = "Skin conditions and diseases" },
                new Specialization { Id = 5, Name = "Pediatrics", Description = "Children's health and development" },
                new Specialization { Id = 6, Name = "General Medicine", Description = "General healthcare and wellness" }
            );
        }
    }
}
