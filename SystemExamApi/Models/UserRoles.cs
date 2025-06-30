namespace SystemExamApi.Models
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string Student = "student";
        
        public static readonly string[] ValidRoles = { Admin, Student };
        
        public static bool IsValidRole(string role)
        {
            return ValidRoles.Contains(role);
        }
    }
} 