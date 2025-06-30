using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class CreateQuestionRequest
{
    [Required]
    public Guid ExamId { get; set; }

    [Required]
    public string Text { get; set; } = null!;

    [Required]
    public string Type { get; set; } = null!;

    [Required]
    public int Points { get; set; }

    [Required]
    public int OrderNumber { get; set; }

    public string? Explanation { get; set; }

    public bool IsActive { get; set; } = true;

    public List<CreateQuestionOptionRequest>? Options { get; set; }
} 