using System;
using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class CreateCategoryRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public string Color { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}