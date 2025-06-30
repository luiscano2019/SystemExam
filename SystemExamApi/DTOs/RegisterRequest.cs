using SystemExamApi.Models;

namespace SystemExamApi.DTOs
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? LastName { get; set; }
        public string? Dni { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public UserRole UserRole { get; set; } = UserRole.Student; // Por defecto es estudiante
    }
}
