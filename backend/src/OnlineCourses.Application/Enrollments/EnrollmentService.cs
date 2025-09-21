using OnlineCourses.Application.Enrollments.Dtos;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Application.Courses; // Para ICourseRepository

namespace OnlineCourses.Application.Enrollments;

public class EnrollmentService
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly ICourseRepository _courses;

    public EnrollmentService(IEnrollmentRepository enrollments, ICourseRepository courses)
    {
        _enrollments = enrollments; _courses = courses;
    }

    public async Task<Guid> EnrollAsync(Guid userId, EnrollRequest req, CancellationToken ct = default)
    {
        // Validar existencia curso
        var course = await _courses.GetByIdAsync(req.CourseId, ct);
        if (course == null) throw new KeyNotFoundException("Curso no encontrado");
        // Verificar duplicado
        if (await _enrollments.ExistsAsync(req.CourseId, userId, ct))
            throw new InvalidOperationException("Ya matriculado");
        var enrollment = new Enrollment { CourseId = req.CourseId, UserId = userId };
        await _enrollments.AddAsync(enrollment, ct);
        return enrollment.Id;
    }

    public async Task<IEnumerable<EnrollmentMyItemDto>> ListMineAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _enrollments.GetByUserAsync(userId, ct);
        return list.Select(e => new EnrollmentMyItemDto(e.Id, e.CourseId, e.EnrolledAt, e.ProgressPercent, e.Status.ToString()));
    }
}