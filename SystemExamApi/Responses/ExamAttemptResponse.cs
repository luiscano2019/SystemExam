using System;

namespace SystemExamApi.Responses;

public class ExamAttemptResponse
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int? TotalPoints { get; set; }
    public int? EarnedPoints { get; set; }
    public string Status { get; set; } = null!;
    public int? TimeSpent { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}