using System.ComponentModel.DataAnnotations;

namespace SystemExamWeb.Models.ViewModels
{
    public class UserViewModel: IValidatableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Name { get; set; } = string.Empty;

        public string? LastName { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        public string? Dni { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public UserRoleEnum UserRole { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastLogin { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

         public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if ( EndDate.Value <= StartDate.Value)
                {
                    yield return new ValidationResult(
                        "La fecha de fin debe ser mayor que la fecha de inicio.",
                        new[] { nameof(StartDate), nameof(EndDate) }
                    );
                }
            }
        }

    }
    public enum UserRoleEnum
    {
        Admin = 1,
        Student = 2
    }

}
