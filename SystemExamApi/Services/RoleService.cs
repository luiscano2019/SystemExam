using SystemExamApi.Models;

namespace SystemExamApi.Services
{
    public interface IRoleService
    {
        string AdminRole { get; }
        string StudentRole { get; }
        string[] ValidRoles { get; }
        bool IsValidRole(string role);
        bool IsAdmin(string role);
        bool IsStudent(string role);
    }

    public class RoleService : IRoleService
    {
        private readonly IConfiguration _configuration;

        public RoleService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string AdminRole => _configuration["UserRoles:Admin"] ?? "admin";
        public string StudentRole => _configuration["UserRoles:Student"] ?? "student";
        
        public string[] ValidRoles => _configuration.GetSection("UserRoles:ValidRoles").Get<string[]>() ?? 
                                     new[] { AdminRole, StudentRole };

        public bool IsValidRole(string role)
        {
            return ValidRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsAdmin(string role)
        {
            return string.Equals(role, AdminRole, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsStudent(string role)
        {
            return string.Equals(role, StudentRole, StringComparison.OrdinalIgnoreCase);
        }
    }
} 