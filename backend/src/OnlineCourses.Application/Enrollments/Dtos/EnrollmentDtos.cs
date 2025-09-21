namespace OnlineCourses.Application.Enrollments.Dtos;

public record EnrollRequest(Guid CourseId);
public record EnrollmentMyItemDto(Guid EnrollmentId, Guid CourseId, DateTime EnrolledAt, int ProgressPercent, string Status);