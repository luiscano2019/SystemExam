using System;
using System.Collections.Generic;

namespace SystemExamWeb.Responses
{
    public class QuestionResponse
    {
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Points { get; set; }
        public int OrderNumber { get; set; }
        public string? Explanation { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<QuestionOptionResponse> Options { get; set; } = new();
    }

    public class QuestionOptionResponse
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int OrderNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}