using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

using SystemExamApi.Models;
using SystemExamApi.Requests;
using SystemExamApi.Responses;
using SystemExamApi.Services;

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
    public ActionResult<ApiResponse<object>> Register([FromBody] User usuario)
    {
        if (!UserRoleExtensions.IsValidRole(usuario.UserRole.GetDescription()))
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = $"Rol inválido. Usa '{UserRole.Admin.GetDescription()}' o '{UserRole.Student.GetDescription()}'.",
                Data = null,
                Errors = new[] { "Rol inválido." }
            });

        _context.Users.Add(usuario);
        _context.SaveChanges();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Usuario registrado.",
            Data = new { usuario.Name, role = usuario.UserRole.GetDescription() },
            Errors = null
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest login)
    {
        //login.Email = "admin@demo.com";
        //login.Password = "admin123";

        login.Email = "student2@demo.com";
        login.Password = "student123";

        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == login.Email &&
                u.Password == login.Password);

        if (user == null)
            return Unauthorized(new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "Credenciales incorrectas.",
                Data = null,
                Errors = new[] { "Usuario o contraseña incorrectos." }
            });

        var token = _jwt.GenerateToken(user.Id, user.Name, user.UserRole.GetDescription());

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Inicio de sesión exitoso.",
            Data = new LoginResponse
            {
                Token = token,
                Role = user.UserRole.GetDescription()
            },
            Errors = null
        });
    }
}

