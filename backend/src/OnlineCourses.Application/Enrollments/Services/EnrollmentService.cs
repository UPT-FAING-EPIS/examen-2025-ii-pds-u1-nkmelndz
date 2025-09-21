using OnlineCourses.Application.Enrollments.DTOs;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Repositories;

namespace OnlineCourses.Application.Enrollments.Services;

public class EnrollmentService
{
    private readonly ICourseRepository _courseRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;

    public EnrollmentService(ICourseRepository courseRepo, IEnrollmentRepository enrollmentRepo)
    {
        _courseRepo = courseRepo;
        _enrollmentRepo = enrollmentRepo;
    }

    public async Task<EnrollmentDto> EnrollAsync(Guid courseId, Guid userId, CancellationToken ct = default)
    {
        var course = await _courseRepo.GetByIdAsync(courseId, ct) ?? throw new InvalidOperationException("Course not found");
        if (!course.IsPublished) throw new InvalidOperationException("Course not published");
        if (await _enrollmentRepo.ExistsAsync(courseId, userId, ct)) throw new InvalidOperationException("Already enrolled");
        var enrollment = new Enrollment { CourseId = courseId, UserId = userId, ProgressPercent = 0, Status = "Active" };
        await _enrollmentRepo.AddAsync(enrollment, ct);
        return new EnrollmentDto(enrollment.Id, courseId, course.Title, enrollment.ProgressPercent, enrollment.Status, enrollment.EnrolledAt);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetMyAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _enrollmentRepo.GetByUserAsync(userId, ct);
        return list.Select(e => new EnrollmentDto(e.Id, e.CourseId, e.Course?.Title ?? string.Empty, e.ProgressPercent, e.Status, e.EnrolledAt));
    }
}