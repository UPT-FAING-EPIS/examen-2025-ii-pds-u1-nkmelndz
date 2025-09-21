using FluentAssertions;
using OnlineCourses.Application.Courses.Services;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructure.Persistence;

namespace OnlineCourses.Tests;

public class CourseServiceTests
{
    [Fact]
    public async Task GetCoursesAsync_ReturnsInsertedCourses()
    {
        var repo = new InMemoryCourseRepository();
        var service = new CourseService(repo);
        await repo.AddAsync(new Course { Title = "Test 1", Description = "Desc", Category = "Cat", Level = "Beginner", DurationHours = 10, Price = 100 });
        var result = await service.GetCoursesAsync(null, null, null);
        result.Should().NotBeEmpty();
    }
}