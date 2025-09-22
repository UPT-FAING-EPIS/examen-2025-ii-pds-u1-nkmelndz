using Microsoft.EntityFrameworkCore;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Infrastructure.Persistence;

public class OnlineCoursesDbContext : DbContext
{
	public DbSet<Course> Courses => Set<Course>();
	public DbSet<User> Users => Set<User>();
	public DbSet<Enrollment> Enrollments => Set<Enrollment>();

	public OnlineCoursesDbContext(DbContextOptions<OnlineCoursesDbContext> options) : base(options) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// User
		modelBuilder.Entity<User>(e =>
		{
			e.HasKey(u => u.Id);
			e.HasIndex(u => u.Email).IsUnique();
			e.Property(u => u.Email).HasMaxLength(180).IsRequired();
			e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
			e.Property(u => u.PasswordHash).HasMaxLength(400).IsRequired();
		});

		// Course
		modelBuilder.Entity<Course>(e =>
		{
			e.HasKey(c => c.Id);
			e.Property(c => c.Title).HasMaxLength(180).IsRequired();
			e.Property(c => c.Category).HasMaxLength(100);
			e.Property(c => c.Level).HasMaxLength(50);
			e.Property(c => c.Syllabus).HasMaxLength(4000);
			e.Property(c => c.Price).HasColumnType("numeric(10,2)");
			e.HasIndex(c => c.Category);
			e.HasIndex(c => c.Level);
		});

		// Enrollment
		modelBuilder.Entity<Enrollment>(e =>
		{
			e.HasKey(en => en.Id);
			e.HasIndex(en => new { en.CourseId, en.UserId }).IsUnique();
			// Relaciones (sin navegación todavía)
		});

		base.OnModelCreating(modelBuilder);
	}
}