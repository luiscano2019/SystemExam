using System;

namespace SystemExamApi.Responses;

public class QuestionOptionResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public int OrderNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 