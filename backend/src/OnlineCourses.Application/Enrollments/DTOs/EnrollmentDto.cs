namespace OnlineCourses.Application.Enrollments.DTOs;

public record EnrollmentDto(
    Guid Id,
    Guid CourseId,
    string CourseTitle,
    int ProgressPercent,
    string Status,
    DateTime EnrolledAt
);