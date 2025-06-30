using System;
using System.Collections.Generic;

namespace SystemExamApi.Models;

public partial class ExamAnswer
{
    public Guid Id { get; set; }

    public Guid AttemptId { get; set; }

    public Guid QuestionId { get; set; }

    public string SelectedOptions { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int PointsEarned { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ExamAttempt Attempt { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
