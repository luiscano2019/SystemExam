using System;
using System.ComponentModel.DataAnnotations;

namespace SystemExamApi.Requests;

public class CreateExamAttemptRequest
{
    [Required]
    public Guid ExamId { get; set; }

    [Required]
    public Guid StudentId { get; set; }
}