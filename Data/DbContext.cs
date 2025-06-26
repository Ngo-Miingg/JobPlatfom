using Microsoft.EntityFrameworkCore;
using JobPlatform.Models;

namespace JobPlatform.Data;

public class JobDbContext(DbContextOptions<JobDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<JobPost> JobPosts => Set<JobPost>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- USER ----
        // User vs CandidateProfile (1-1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.CandidateProfile)
            .WithOne(cp => cp.User)
            .HasForeignKey<CandidateProfile>(cp => cp.UserId);

        // User vs CompanyProfile (1-1)
        modelBuilder.Entity<User>()
            .HasOne(u => u.CompanyProfile)
            .WithOne(cp => cp.User)
            .HasForeignKey<CompanyProfile>(cp => cp.UserId);

        // User vs Notification (1-N)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId);

        // ---- COMPANY & JOB POST ----
        // CompanyProfile vs JobPost (1-N)
        modelBuilder.Entity<CompanyProfile>()
            .HasMany(c => c.JobPosts)
            .WithOne(j => j.CompanyProfile)
            .HasForeignKey(j => j.CompanyProfileId)
            .OnDelete(DeleteBehavior.Cascade); // Khi xóa công ty, xóa hết tin tuyển dụng

        // JobPost vs Application (1-N)
        modelBuilder.Entity<JobPost>()
            .HasMany(j => j.Applications)
            .WithOne(a => a.JobPost)
            .HasForeignKey(a => a.JobPostId)
            .OnDelete(DeleteBehavior.Cascade); // Khi xóa tin, xóa hết đơn ứng tuyển

        // ---- APPLICATION ----
        // Application vs User (Candidate) (N-1)
        modelBuilder.Entity<Application>()
            .HasOne(a => a.Candidate)
            .WithMany() // User có thể có nhiều đơn ứng tuyển
            .HasForeignKey(a => a.CandidateId)
            .OnDelete(DeleteBehavior.Restrict); // <<-- SỬA LỖI QUAN TRỌNG NHẤT
                                                // Ngăn chặn vòng lặp cascade. Khi xóa User, không tự động xóa Application qua đường này.
                                                // Application sẽ được xóa khi JobPost bị xóa.

        // --- Cấu hình các thuộc tính DECMIAL ---
        modelBuilder.Entity<CandidateProfile>()
            .Property(c => c.DesiredSalary)
            .HasPrecision(18, 2);

        modelBuilder.Entity<JobPost>()
            .Property(j => j.Salary)
            .HasPrecision(18, 2);
    }


}
