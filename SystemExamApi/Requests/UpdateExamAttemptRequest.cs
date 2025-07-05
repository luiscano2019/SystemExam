using System;

namespace SystemExamApi.Requests;

public class UpdateExamAttemptRequest
{
    public DateTime? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int? TotalPoints { get; set; }
    public int? EarnedPoints { get; set; }
    public string? Status { get; set; }
    public int? TimeSpent { get; set; }
}