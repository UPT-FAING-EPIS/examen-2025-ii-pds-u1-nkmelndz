namespace OnlineCourses.Domain.Entities;

public class Course
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // Beginner, Intermediate, Advanced
    public string Syllabus { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid InstructorId { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}