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
public class ExamAttemptRequest
{

    [Required]
    public Guid Id { get; set; }
}

// DTO para envío de todas las respuestas del alumno
public class SubmitAnswersRequest
{
    public Guid AttemptId { get; set; }
    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public class SubmitAnswerDto
{
    public Guid QuestionId { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = new();
}
