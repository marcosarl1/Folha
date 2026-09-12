using System.Security.Claims;
using Folha.Api.DTOs;
using Folha.Domain.Entities;
using Folha.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Folha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(AppDbContext context) : ControllerBase
{
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var categories = await context.Categories
        .Where(c => c.UserId == userId)
        .Select(c => new CategoryResponse(c.Id, c.Name, c.Color))
        .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        var userId = GetUserId();
        var category = new Category
        {
            Name = request.Name,
            Color = request.Color ?? "#000000",
            UserId = userId
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new CategoryResponse(category.Id, category.Name, category.Color));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request)
    {
        var userId = GetUserId();
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null) return NotFound(new { message = "Categoria não encontrada" });

        category.Name = request.Name;
        category.Color = request.Color ?? category.Color;
        await context.SaveChangesAsync();

        return Ok(new CategoryResponse(category.Id, category.Name, category.Color));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category == null) return NotFound(new { message = "Categoria não encontrada" });

        var hasTransacations = await context.Transactions.AnyAsync(t => t.CategoryId == id);
        if (hasTransacations) return BadRequest(new { message = "Não pode apagar categoria com transações" });

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
