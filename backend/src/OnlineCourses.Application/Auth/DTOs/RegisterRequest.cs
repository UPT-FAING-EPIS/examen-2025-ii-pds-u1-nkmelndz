namespace OnlineCourses.Application.Auth.DTOs;

public record RegisterRequest(string Email, string FullName, string Password);