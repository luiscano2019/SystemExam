using System;
using System.Collections.Generic;

namespace SystemExamApi.Responses;

public class QuestionResponse
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string Text { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int Points { get; set; }
    public int OrderNumber { get; set; }
    public string? Explanation { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<QuestionOptionResponse>? Options { get; set; }
} 