namespace OnlineCourses.Application.Courses.Dtos;

public record CourseListItemDto(Guid Id, string Title, string Category, string Level, bool IsPublished, decimal Price);
public record CourseDetailDto(Guid Id, string Title, string Description, string Category, string Level, string Syllabus, int DurationHours, decimal Price, bool IsPublished, Guid InstructorId, DateTime CreatedAt);
public record CreateCourseRequest(string Title, string Description, string Category, string Level, string Syllabus, int DurationHours, decimal Price, bool IsPublished);
public record UpdateCourseRequest(string Title, string Description, string Category, string Level, string Syllabus, int DurationHours, decimal Price, bool IsPublished);