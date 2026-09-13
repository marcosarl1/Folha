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
public class TransactionsController(AppDbContext context) : ControllerBase
{
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? month, [FromQuery] int? year, [FromQuery] Folha.Domain.Enums.TransactionType? type, [FromQuery] Guid? categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var query = context.Transactions.Include(t => t.Category).Where(t => t.UserId == userId).AsQueryable();

        if (month.HasValue) query = query.Where(t => t.Date.Month == month.Value);
        if (year.HasValue) query = query.Where(t => t.Date.Year == year.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);

        var total = await query.CountAsync();
        var items = await query
        .OrderByDescending(t => t.Date)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => new TransactionResponse(t.Id, t.Description, t.Amount, t.Type, t.Date, t.CategoryId, t.Category.Name, t.Category.Color))
        .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var userId = GetUserId();
        var query = context.Transactions.Where(t => t.UserId == userId);

        if (month.HasValue) query = query.Where(t => t.Date.Month == month.Value);
        if (year.HasValue) query = query.Where(t => t.Date.Year == year.Value);

        var income = await query.Where(t => t.Type == Folha.Domain.Enums.TransactionType.Income).SumAsync(t => t.Amount);
        var expense = await query.Where(t => t.Type == Folha.Domain.Enums.TransactionType.Expense).SumAsync(t => t.Amount);

        return Ok(new SummaryResponse(income, expense, income - expense, await query.CountAsync()));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTransactionRequest request)
    {
        var userId = GetUserId();
        if (request.Amount <= 0) return BadRequest(new { message = "Valor deve ser maior que zero" });

        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.UserId == userId);
        if (category == null) return BadRequest(new { message = "Categoria inválida" });

        var transaction = new Transaction
        {
            Description = request.Description,
            Amount = request.Amount,
            Type = request.Type,
            Date = request.Date,
            CategoryId = request.CategoryId,
            UserId = userId
        };

        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new TransactionResponse(transaction.Id, transaction.Description, transaction.Amount, transaction.Type, transaction.Date, transaction.CategoryId, category.Name, category.Color));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var transaction = await context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (transaction == null) return NotFound(new { message = "Transação não encontrada" });

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
