using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class CreateExamRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public Guid CategoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int Duration { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalQuestions { get; set; }

    [Range(0, 100)]
    public int PassingScore { get; set; }

    public bool IsActive { get; set; } = true;

    public bool RandomizeQuestions { get; set; } = false;
}