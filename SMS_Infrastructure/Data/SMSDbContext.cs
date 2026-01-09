using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Entities.Application;
namespace Infrastructure.Data
{
    public class SMSDbContext:DbContext
    {
        public SMSDbContext(DbContextOptions<SMSDbContext> options)
            : base(options) { }

        public DbSet<Banda> Bandas { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<ApplicantGuardian> ApplicantGuardians { get; set; }
        public DbSet<ApplicantSibling> ApplicantSiblings { get; set; }
        public DbSet<Classes> Classes { get; set; }
        public DbSet<QuickLink> QuickLinks { get; set; }
        public DbSet<Schools> schools { get; set; }
        public DbSet<AdmissionChallan> admissionChallan { get; set; }
        public DbSet<AdmissionChallanDetails> admissionChallanDetails { get; set; }
        public DbSet<BankDetails> bankDetails { get; set; }
        public DbSet<FeeAndCharges> feeAndCharges { get; set; }
        public DbSet<FeeAndChargesAmount> feeAndChargesAmounts { get; set; }
        public DbSet<ClassSections> classSections { get; set; }
        public DbSet<SchoolStaff> schoolStaff { get; set; }
        public DbSet<StaffEducation> staffEducations { get; set; }
        public DbSet<SectionTeachers> SectionTeachers { get; set; }
        public DbSet<TeacherSubjects> TeacherSubjects { get; set; }
        public DbSet<ClassSubjects> ClassSubjects { get; set; }
        public DbSet<ClassTimeTable> ClassTimeTable { get; set; }
        public DbSet<SchoolParams> SchoolParams { get; set; }
        public DbSet<Subjects> Subjects { get; set; }
        public DbSet<ChallanHeads> ChallanHeads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Banda>().HasIndex(b => b.Ban_username).IsUnique();

            modelBuilder.Entity<Classes>(e =>
            {
                e.ToTable("classes");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("cls_id");
                e.Property(x => x.SchoolId).HasColumnName("cls_schid");
                e.Property(x => x.Prefix).HasColumnName("cls_prefix").HasMaxLength(3).IsRequired();
                e.Property(x => x.Category).HasColumnName("cls_category").HasMaxLength(100).IsRequired();
                e.Property(x => x.Description).HasColumnName("cls_description").HasMaxLength(100).IsRequired();
                e.Property(x => x.Active).HasColumnName("cls_active").IsRequired();
            });

            modelBuilder.Entity<ApplicantGuardian>()
            .HasOne(g => g.Applicant)
            .WithMany(a => a.Guardians)
            .HasForeignKey(g => g.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict); // important

            // ApplicantSibling FK without cascade
            modelBuilder.Entity<ApplicantSibling>()
                .HasOne(s => s.Applicant)
                .WithMany(a => a.Siblings)
                .HasForeignKey(s => s.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict); // important
        }

    }
}
