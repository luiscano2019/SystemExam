using System;
using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class UpdateExamAnswerRequest
{
    [Required]
    public string SelectedOptions { get; set; } = null!;

    public bool? IsCorrect { get; set; }
    public int? PointsEarned { get; set; }
}