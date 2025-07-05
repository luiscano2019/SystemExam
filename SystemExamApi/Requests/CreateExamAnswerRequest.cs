using System;
using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class CreateExamAnswerRequest
{
    [Required]
    public Guid AttemptId { get; set; }

    [Required]
    public Guid QuestionId { get; set; }

    [Required]
    public string SelectedOptions { get; set; } = null!;
}