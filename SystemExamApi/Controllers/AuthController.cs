using MiApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SystemExamApi.DTOs;
using SystemExamApi.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ExamSystemDbContext _context;
    private readonly JwtService _jwt;

    public AuthController(ExamSystemDbContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User usuario)
    {
        if (!UserRoleExtensions.IsValidRole(usuario.UserRole.GetDescription()))
            return BadRequest($"Rol inválido. Usa '{UserRole.Admin.GetDescription()}' o '{UserRole.Student.GetDescription()}'.");

        _context.Users.Add(usuario);
        _context.SaveChanges();
        return Ok(new { mensaje = "Usuario registrado", usuario.Name, role = usuario.UserRole.GetDescription() });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        login.Email = "admin@demo.com";
        login.Password = "admin123";



        //login.Email = "student2@demo.com";
        //login.Password = "student123";


        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == login.Email &&
                u.Password == login.Password); // ⚠️ Considera usar contraseña encriptada en producción

        if (user == null)
            return Unauthorized(new { mensaje = "Credenciales incorrectas" });

        var token = _jwt.GenerateToken(user.Name, user.UserRole.GetDescription());

        return Ok(new LoginResponse
        {
            Token = token,
            Role = user.UserRole.GetDescription()
        });
    }
}

