using System.ComponentModel;

namespace SystemExamApi.Models
{
    public enum UserRole
    {
        [Description("admin")]
        Admin = 1,
        
        [Description("student")]
        Student = 2
    }
    
    public static class UserRoleExtensions
    {
        public static string GetDescription(this UserRole role)
        {
            var field = role.GetType().GetField(role.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            
            return attribute?.Description ?? role.ToString().ToLower();
        }
        
        public static UserRole FromDescription(string description)
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (role.GetDescription().Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    return role;
                }
            }
            
            throw new ArgumentException($"Invalid role description: {description}");
        }
        
        public static bool IsValidRole(string roleDescription)
        {
            return Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Any(r => r.GetDescription().Equals(roleDescription, StringComparison.OrdinalIgnoreCase));
        }
    }
} 