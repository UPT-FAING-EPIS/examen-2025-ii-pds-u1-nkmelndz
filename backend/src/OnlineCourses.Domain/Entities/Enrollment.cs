namespace OnlineCourses.Domain.Entities;

public class Enrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public int ProgressPercent { get; set; }
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled
    public Course? Course { get; set; }
}