namespace SystemExamApi.Responses;

public class ExamResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public int Duration { get; set; }
    public int TotalQuestions { get; set; }
    public int PassingScore { get; set; }
    public bool IsActive { get; set; }
    public bool RandomizeQuestions { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}