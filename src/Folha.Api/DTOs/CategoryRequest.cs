namespace Folha.Api.DTOs;

public record CreateCategoryRequest(string Name, string? Color);
public record UpdateCategoryRequest(string Name, string? Color);
public record CategoryResponse(Guid Id, string Name, string Color);
