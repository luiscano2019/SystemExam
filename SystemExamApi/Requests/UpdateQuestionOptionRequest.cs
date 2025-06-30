using System;
using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class UpdateQuestionOptionRequest
{
    [Required]
    public Guid QuestionId { get; set; }

    [Required]
    public string Text { get; set; } = null!;

    [Required]
    public bool IsCorrect { get; set; }

    [Required]
    public int OrderNumber { get; set; }
} 