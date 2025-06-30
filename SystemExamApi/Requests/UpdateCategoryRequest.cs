using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class UpdateCategoryRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(255, ErrorMessage = "El nombre no puede exceder 255 caracteres")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "La descripción es requerida")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "El color es requerido")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe ser un código hexadecimal válido (ej: #3b82f6)")]
    public string Color { get; set; } = null!;

    public bool IsActive { get; set; }
}