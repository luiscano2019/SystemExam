using SystemExamApi.Models;

namespace SystemExamApi.Examples
{
    public static class UserRoleUsage
    {
        public static void Examples()
        {
            // Crear un usuario admin
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                Password = "admin123",
                Name = "Admin User",
                UserRole = UserRole.Admin, // Usando el enum directamente
                IsActive = true
            };

            // Crear un usuario estudiante
            var studentUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "student@example.com",
                Password = "student123",
                Name = "Student User",
                UserRole = UserRole.Student, // Usando el enum directamente
                IsActive = true
            };

            // Obtener la descripción del rol (string)
            string adminRoleString = UserRole.Admin.GetDescription(); // "admin"
            string studentRoleString = UserRole.Student.GetDescription(); // "student"

            // Convertir string a enum
            UserRole adminRole = UserRoleExtensions.FromDescription("admin"); // UserRole.Admin
            UserRole studentRole = UserRoleExtensions.FromDescription("student"); // UserRole.Student

            // Validar si un string es un rol válido
            bool isValidAdmin = UserRoleExtensions.IsValidRole("admin"); // true
            bool isValidStudent = UserRoleExtensions.IsValidRole("student"); // true
            bool isValidInvalid = UserRoleExtensions.IsValidRole("invalid"); // false

            // Comparaciones
            bool isAdmin = adminUser.UserRole == UserRole.Admin; // true
            bool isStudent = studentUser.UserRole == UserRole.Student; // true

            // Obtener todos los roles válidos
            var allRoles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => r.GetDescription())
                .ToArray(); // ["admin", "student"]
        }
    }
} 