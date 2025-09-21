namespace OnlineCourses.Domain.Entities;

public enum EnrollmentStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2
}

public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public int ProgressPercent { get; set; } = 0;
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
}