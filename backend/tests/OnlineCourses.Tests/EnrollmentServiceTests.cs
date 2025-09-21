using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Enrollments.Services;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructure.Persistence;
using OnlineCourses.Application.Courses.Services;
using OnlineCourses.Domain.Repositories;

namespace OnlineCourses.Tests;

public class EnrollmentServiceTests
{
    private AppDbContext CreateDb()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opts);
    }

    [Fact]
    public async Task EnrollAsync_CreatesEnrollment_WhenValid()
    {
        var db = CreateDb();
        var course = new Course { Title = "C#", Description = "Basics", Category = "Dev", Level = "Beginner", IsPublished = true };
        db.Courses.Add(course); db.SaveChanges();
        ICourseRepository courseRepo = new EfCourseRepository(db);
        var enrollmentRepo = new EfEnrollmentRepository(db);
        var service = new EnrollmentService(courseRepo, enrollmentRepo);

        var dto = await service.EnrollAsync(course.Id, Guid.NewGuid());

        dto.CourseId.Should().Be(course.Id);
        db.Enrollments.Count().Should().Be(1);
    }

    [Fact]
    public async Task EnrollAsync_Throws_IfAlreadyEnrolled()
    {
        var db = CreateDb();
        var userId = Guid.NewGuid();
        var course = new Course { Title = "C#", Description = "Basics", Category = "Dev", Level = "Beginner", IsPublished = true };
        db.Courses.Add(course);
        db.Enrollments.Add(new Enrollment { CourseId = course.Id, UserId = userId });
        db.SaveChanges();
        ICourseRepository courseRepo = new EfCourseRepository(db);
        var enrollmentRepo = new EfEnrollmentRepository(db);
        var service = new EnrollmentService(courseRepo, enrollmentRepo);

        var act = () => service.EnrollAsync(course.Id, userId);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Already enrolled");
    }
}
