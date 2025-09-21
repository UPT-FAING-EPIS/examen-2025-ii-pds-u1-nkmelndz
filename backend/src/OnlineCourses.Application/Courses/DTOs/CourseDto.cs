namespace OnlineCourses.Application.Courses.DTOs;

public record CourseDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    string Level,
    int DurationHours,
    decimal Price,
    bool IsPublished
);