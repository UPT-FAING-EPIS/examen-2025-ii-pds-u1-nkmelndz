namespace OnlineCourses.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // Student, Instructor, Admin
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}