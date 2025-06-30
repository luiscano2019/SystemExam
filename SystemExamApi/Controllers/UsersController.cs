using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemExamApi.Models;

namespace SystemExamApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class UsersController : ControllerBase
    {
        private readonly ExamSystemDbContext _context;

        public UsersController(ExamSystemDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                return StatusCode(500, new { 
                    message = "Error al obtener la lista de usuarios",
                    error = ex.Message
                });
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest("El ID en la URL no coincide con el ID del usuario");
            }

            // Validar que el usuario existe
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Validar que el email sea único (si se está cambiando)
            if (user.Email != existingUser.Email)
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (emailExists)
                {
                    return BadRequest("El email ya está en uso por otro usuario");
                }
            }

            // Validar que el DNI sea único (si se está cambiando y no es null)
            if (!string.IsNullOrEmpty(user.Dni) && user.Dni != existingUser.Dni)
            {
                var dniExists = await _context.Users.AnyAsync(u => u.Dni == user.Dni);
                if (dniExists)
                {
                    return BadRequest("El DNI ya está en uso por otro usuario");
                }
            }

            // Validar el rol usando el enum
            if (!UserRoleExtensions.IsValidRole(user.UserRole.GetDescription()))
            {
                return BadRequest($"El rol debe ser '{UserRole.Admin.GetDescription()}' o '{UserRole.Student.GetDescription()}'");
            }

            // Actualizar solo los campos permitidos
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;
            existingUser.Name = user.Name;
            existingUser.LastName = user.LastName;
            existingUser.Dni = user.Dni;
            existingUser.Phone = user.Phone;
            existingUser.Address = user.Address;
            existingUser.UserRole = user.UserRole;
            existingUser.StartDate = user.StartDate;
            existingUser.EndDate = user.EndDate;
            existingUser.IsActive = user.IsActive;
            existingUser.LastLogin = user.LastLogin;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // No actualizar CreatedAt ni Id

            try
            {
                await _context.SaveChangesAsync();
                return Ok(existingUser);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound("Usuario no encontrado");
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] User user)
        {
            try
            {
                // Validar que el modelo sea válido
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validar que el email no esté en uso
                var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (emailExists)
                {
                    return BadRequest("El email ya está en uso por otro usuario");
                }

                // Validar que el DNI no esté en uso (si se proporciona)
                if (!string.IsNullOrEmpty(user.Dni))
                {
                    var dniExists = await _context.Users.AnyAsync(u => u.Dni == user.Dni);
                    if (dniExists)
                    {
                        return BadRequest("El DNI ya está en uso por otro usuario");
                    }
                }

                // Validar el rol usando el enum
                if (!UserRoleExtensions.IsValidRole(user.UserRole.GetDescription()))
                {
                    return BadRequest($"El rol debe ser '{UserRole.Admin.GetDescription()}' o '{UserRole.Student.GetDescription()}'");
                }

                // Validar que no se cree un admin si ya hay uno (opcional - para limitar admins)
                if (user.UserRole == UserRole.Admin)
                {
                    var adminCount = await _context.Users.CountAsync(u => u.UserRole == UserRole.Admin && u.IsActive);
                    if (adminCount >= 5) // Límite de 5 administradores
                    {
                        return BadRequest("Se ha alcanzado el límite máximo de administradores (5)");
                    }
                }

                // Configurar valores por defecto
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.LastLogin = null;

                // Agregar el usuario a la base de datos
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Retornar el usuario creado (sin la contraseña por seguridad)
                var createdUser = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.LastName,
                    user.Dni,
                    user.Phone,
                    user.Address,
                    role = user.UserRole.GetDescription(),
                    user.StartDate,
                    user.EndDate,
                    user.IsActive,
                    user.CreatedAt,
                    user.UpdatedAt
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, createdUser);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { 
                    message = "Error al crear el usuario en la base de datos",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Error inesperado al crear el usuario",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            // Buscar el usuario con sus relaciones de estudiante
            var user = await _context.Users
                .Include(u => u.ExamAttempts)
                    .ThenInclude(ea => ea.ExamAnswers)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            // Validar que no se elimine el último admin usando el enum
            if (user.UserRole == UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.UserRole == UserRole.Admin && u.IsActive);
                if (adminCount <= 1)
                {
                    return BadRequest("No se puede eliminar el último administrador del sistema");
                }
            }

            // Verificar si el usuario ha creado exámenes (no debería ser el caso para estudiantes)
            var hasCreatedExams = await _context.Exams.AnyAsync(e => e.CreatedBy == id);
            if (hasCreatedExams)
            {
                return BadRequest("No se puede eliminar un usuario que ha creado exámenes. Use el método DELETE completo para administradores.");
            }

            try
            {
                // Eliminar en cascada las relaciones del estudiante
                
                // 1. Eliminar respuestas de examen del estudiante
                var studentExamAnswers = user.ExamAttempts
                    .SelectMany(ea => ea.ExamAnswers)
                    .ToList();
                if (studentExamAnswers.Any())
                {
                    _context.ExamAnswers.RemoveRange(studentExamAnswers);
                }

                // 2. Eliminar intentos de examen del estudiante
                if (user.ExamAttempts.Any())
                {
                    _context.ExamAttempts.RemoveRange(user.ExamAttempts);
                }

                // 3. Eliminar el usuario
                _context.Users.Remove(user);
                
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Estudiante eliminado exitosamente",
                    deletedUser = new {
                        id = user.Id,
                        name = user.Name,
                        email = user.Email,
                        role = user.UserRole.GetDescription()
                    },
                    deletedRelations = new {
                        examAttempts = user.ExamAttempts.Count,
                        examAnswers = studentExamAnswers.Count
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { 
                    message = "Error al eliminar el estudiante y sus relaciones.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
} 