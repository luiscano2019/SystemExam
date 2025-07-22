
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SystemExamApi.Models;
using SystemExamApi.Responses;

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
        public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetUsers()
        {

            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(new ApiResponse<IEnumerable<User>>
                {
                    Success = true,
                    Message = "Usuarios obtenidos correctamente.",
                    Data = users,
                    Errors = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<User>>
                {
                    Success = false,
                    Message = "Error al obtener la lista de usuarios.",
                    Data = null,
                    Errors = new[] { ex.Message }
                });
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponse<User>
                {
                    Success = false,
                    Message = "Usuario no encontrado.",
                    Data = null,
                    Errors = new[] { "No existe un usuario con el ID proporcionado." }
                });
            }
            return Ok(new ApiResponse<User>
            {
                Success = true,
                Message = "Usuario obtenido correctamente.",
                Data = user,
                Errors = null
            });
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<User>>> PutUser(Guid id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest(new ApiResponse<User>
                {
                    Success = false,
                    Message = "El ID en la URL no coincide con el ID del usuario.",
                    Data = null,
                    Errors = new[] { "El ID en la URL no coincide con el ID del usuario." }
                });
            }

            // Validar que el usuario existe
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new ApiResponse<User>
                {
                    Success = false,
                    Message = "Usuario no encontrado.",
                    Data = null,
                    Errors = new[] { "No existe un usuario con el ID proporcionado." }
                });
            }

            // Validar que el email sea único (si se está cambiando)
            if (user.Email != existingUser.Email)
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (emailExists)
                {
                    return BadRequest(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "El email ya está en uso por otro usuario.",
                        Data = null,
                        Errors = new[] { "El email ya está en uso por otro usuario." }
                    });
                }
            }

            // Validar que el DNI sea único (si se está cambiando y no es null)
            if (!string.IsNullOrEmpty(user.Dni) && user.Dni != existingUser.Dni)
            {
                var dniExists = await _context.Users.AnyAsync(u => u.Dni == user.Dni);
                if (dniExists)
                {
                    return BadRequest(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "El DNI ya está en uso por otro usuario.",
                        Data = null,
                        Errors = new[] { "El DNI ya está en uso por otro usuario." }
                    });
                }
            }

            // Validar el rol usando el enum
            if (!UserRoleExtensions.IsValidRole(user.UserRole.GetDescription()))
            {
                return BadRequest(new ApiResponse<User>
                {
                    Success = false,
                    Message = $"El rol debe ser '{UserRole.Admin.GetDescription()}' o '{UserRole.Student.GetDescription()}'",
                    Data = null,
                    Errors = new[] { "Rol inválido." }
                });
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
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Message = "Usuario actualizado exitosamente.",
                    Data = existingUser,
                    Errors = null
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Usuario no encontrado.",
                        Data = null,
                        Errors = new[] { "No existe un usuario con el ID proporcionado." }
                    });
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> PostUser([FromBody] User user)
        {
            try
            {
                // Validar que el modelo sea válido
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Datos de entrada inválidos.",
                        Data = null,
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                // Validar que el email no esté en uso
                var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email);
                if (emailExists)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El email ya está en uso por otro usuario.",
                        Data = null,
                        Errors = new[] { "El email ya está en uso por otro usuario." }
                    });
                }

                // Validar que el DNI no esté en uso (si se proporciona)
                if (!string.IsNullOrEmpty(user.Dni))
                {
                    var dniExists = await _context.Users.AnyAsync(u => u.Dni == user.Dni);
                    if (dniExists)
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "El DNI ya está en uso por otro usuario.",
                            Data = null,
                            Errors = new[] { "El DNI ya está en uso por otro usuario." }
                        });
                    }
                }

                // Validar el rol usando el enum
                if (!UserRoleExtensions.IsValidRole(user.UserRole.GetDescription()))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"El rol debe ser '{UserRole.Admin.GetDescription()}' o '{UserRole.Student.GetDescription()}'",
                        Data = null,
                        Errors = new[] { "Rol inválido." }
                    });
                }

                // Validar que no se cree un admin si ya hay uno (opcional - para limitar admins)
                if (user.UserRole == UserRole.Admin)
                {
                    var adminCount = await _context.Users.CountAsync(u => u.UserRole == UserRole.Admin && u.IsActive);
                    if (adminCount >= 5) // Límite de 5 administradores
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Se ha alcanzado el límite máximo de administradores (5)",
                            Data = null,
                            Errors = new[] { "Límite de administradores alcanzado." }
                        });
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

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new ApiResponse<object>
                {
                    Success = true,
                    Message = "Usuario creado exitosamente.",
                    Data = createdUser,
                    Errors = null
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al crear el usuario en la base de datos.",
                    Data = null,
                    Errors = new[] { ex.InnerException?.Message ?? ex.Message }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error inesperado al crear el usuario.",
                    Data = null,
                    Errors = new[] { ex.Message }
                });
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
        {
            // Buscar el usuario con sus relaciones de estudiante
            var user = await _context.Users
                .Include(u => u.ExamAttempts)
                    .ThenInclude(ea => ea.ExamAnswers)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Usuario no encontrado.",
                    Data = null,
                    Errors = new[] { "No existe un usuario con el ID proporcionado." }
                });
            }

            // Validar que no se elimine el último admin usando el enum
            if (user.UserRole == UserRole.Admin)
            {
                var adminCount = await _context.Users.CountAsync(u => u.UserRole == UserRole.Admin && u.IsActive);
                if (adminCount <= 1)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No se puede eliminar el último administrador del sistema.",
                        Data = null,
                        Errors = new[] { "No se puede eliminar el último administrador del sistema." }
                    });
                }
            }

            // Verificar si el usuario ha creado exámenes (no debería ser el caso para estudiantes)
            var hasCreatedExams = await _context.Exams.AnyAsync(e => e.CreatedBy == id);
            if (hasCreatedExams)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No se puede eliminar un usuario que ha creado exámenes. Use el método DELETE completo para administradores.",
                    Data = null,
                    Errors = new[] { "No se puede eliminar un usuario que ha creado exámenes." }
                });
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

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Estudiante eliminado exitosamente.",
                    Data = new {
                        id = user.Id,
                        name = user.Name,
                        email = user.Email,
                        role = user.UserRole.GetDescription(),
                        deletedRelations = new {
                            examAttempts = user.ExamAttempts.Count,
                            examAnswers = studentExamAnswers.Count
                        }
                    },
                    Errors = null
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al eliminar el estudiante y sus relaciones.",
                    Data = null,
                    Errors = new[] { ex.InnerException?.Message ?? ex.Message }
                });
            }
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
} 