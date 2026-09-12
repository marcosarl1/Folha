using Folha.Api.DTOs;
using Folha.Api.Services;
using Folha.Domain.Entities;
using Folha.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Folha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext context, TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Email já cadastrado" });
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Name, user.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email ou senha inválidos" });
        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Name, user.Email));
    }
}
