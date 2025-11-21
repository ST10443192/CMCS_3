using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CMCS_3.Models;

namespace CMCS_3.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for your models
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Lecturer entity
            builder.Entity<Lecturer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.EmployeeId).IsUnique();

                entity.Property(e => e.HourlyRate)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure Claim entity
            builder.Entity<Claim>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClaimReference).IsUnique();

                // Fix decimal precision warnings
                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.HourlyRate)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalHours)
                    .HasColumnType("decimal(18,2)");

                // Relationship with Lecturer
                entity.HasOne(c => c.Lecturer)
                    .WithMany(l => l.Claims)
                    .HasForeignKey(c => c.LecturerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PaymentRecord entity
            builder.Entity<PaymentRecord>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");
            });
        }
    }
}