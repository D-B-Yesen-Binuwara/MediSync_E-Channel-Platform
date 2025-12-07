using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Doctor>().ToTable("Doctors");
            modelBuilder.Entity<DoctorSchedule>().ToTable("DoctorSchedules");
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
            modelBuilder.Entity<Transaction>().ToTable("Transactions");
            modelBuilder.Entity<Favorite>().ToTable("Favorites");

            // Transaction User (Patient)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Patient)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment User (Patient)
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment DoctorSchedule
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.DoctorSchedule)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            // DoctorSchedule Doctor
            modelBuilder.Entity<DoctorSchedule>()
                .HasOne(s => s.Doctor)
                .WithMany(d => d.DoctorSchedules)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favorite User (Patient)
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Patient)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favorite Doctor
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Doctor)
                .WithMany(d => d.Favorites)
                .HasForeignKey(f => f.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
