using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SystemExamApi.Models;
using SystemExamApi.Requests;
using SystemExamApi.Responses;

namespace SystemExamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamAttemptsController : ControllerBase
{
    private readonly ExamSystemDbContext _context;

    public ExamAttemptsController(ExamSystemDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ExamAttemptResponse>>> GetAttempt(Guid id)
    {
        var attempt = await _context.ExamAttempts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (attempt == null)
            return NotFound(new ApiResponse<ExamAttemptResponse>
            {
                Success = false,
                Message = "Intento no encontrado.",
                Data = null,
                Errors = new[] { "No existe un intento con el ID proporcionado." }
            });

        var response = new ExamAttemptResponse
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            EarnedPoints = attempt.EarnedPoints,
            Status = attempt.Status,
            TimeSpent = attempt.TimeSpent,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt
        };

        return Ok(new ApiResponse<ExamAttemptResponse>
        {
            Success = true,
            Message = "Intento obtenido correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpGet("by-exam/{examId}")]
    public async Task<ActionResult<ApiResponse<List<ExamAttemptResponse>>>> GetAttemptsByExam(Guid examId)
    {
        var attempts = await _context.ExamAttempts
            .AsNoTracking()
            .Where(a => a.ExamId == examId)
            .ToListAsync();

        var response = attempts.Select(attempt => new ExamAttemptResponse
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            EarnedPoints = attempt.EarnedPoints,
            Status = attempt.Status,
            TimeSpent = attempt.TimeSpent,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt
        }).ToList();

        return Ok(new ApiResponse<List<ExamAttemptResponse>>
        {
            Success = true,
            Message = "Intentos obtenidos correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<ApiResponse<List<ExamAttemptResponse>>>> GetAttemptsByStudent(Guid studentId)
    {
        var attempts = await _context.ExamAttempts
            .AsNoTracking()
            .Where(a => a.StudentId == studentId)
            .ToListAsync();

        var response = attempts.Select(attempt => new ExamAttemptResponse
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            EarnedPoints = attempt.EarnedPoints,
            Status = attempt.Status,
            TimeSpent = attempt.TimeSpent,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt
        }).ToList();

        return Ok(new ApiResponse<List<ExamAttemptResponse>>
        {
            Success = true,
            Message = "Intentos obtenidos correctamente.",
            Data = response,
            Errors = null
        });
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateAttempt(Guid id, [FromBody] UpdateExamAttemptRequest request)
    {
        var attempt = await _context.ExamAttempts.FirstOrDefaultAsync(a => a.Id == id);
        if (attempt == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Intento no encontrado.",
                Data = null,
                Errors = new[] { "No existe un intento con el ID proporcionado." }
            });

        attempt.CompletedAt = request.CompletedAt ?? attempt.CompletedAt;
        attempt.Score = request.Score ?? attempt.Score;
        attempt.TotalPoints = request.TotalPoints ?? attempt.TotalPoints;
        attempt.EarnedPoints = request.EarnedPoints ?? attempt.EarnedPoints;
        attempt.Status = request.Status ?? attempt.Status;
        attempt.TimeSpent = request.TimeSpent ?? attempt.TimeSpent;
        attempt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Intento actualizado exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAttempt(Guid id)
    {
        var attempt = await _context.ExamAttempts.FirstOrDefaultAsync(a => a.Id == id);
        if (attempt == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Intento no encontrado.",
                Data = null,
                Errors = new[] { "No existe un intento con el ID proporcionado." }
            });

        _context.ExamAttempts.Remove(attempt);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Intento eliminado exitosamente.",
            Data = null,
            Errors = null
        });
    }

    // POST /api/examattempts/start
    // Inicia un intento de examen para un estudiante y devuelve preguntas con opciones
    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<ExamAttemptResponse>>> CreateAttempt([FromBody] CreateExamAttemptRequest request)
    {

       
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<ExamAttemptResponse>
            {
                Success = false,
                Message = "Datos de entrada inv�lidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var student = await _context.Users.FirstOrDefaultAsync(a => a.Id == Guid.Parse(userId));

        //var student = await _context.Users.FindAsync(userId);
        if (student == null) {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Usuario no encontrado",
                Data = null,
                Errors = new[] { "No existe el usuario." }
            });

        }
          


        var attempt = new ExamAttempt
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            StudentId = student.Id,
            StartedAt = DateTime.UtcNow,
            Status = "in_progress",
            //IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            //UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ExamAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        var response = new ExamAttemptResponse
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            EarnedPoints = attempt.EarnedPoints,
            Status = attempt.Status,
            TimeSpent = attempt.TimeSpent,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt
        };

        return CreatedAtAction(nameof(GetAttempt), new { id = attempt.Id }, new ApiResponse<ExamAttemptResponse>
        {
            Success = true,
            Message = "Intento creado exitosamente.",
            Data = response,
            Errors = null
        });
    }   

    // POST /api/examattempts/submit-answers
    // Recibe las respuestas del alumno para cada pregunta y las guarda en la base de datos
    [HttpPost("submit-answers")]
    public async Task<IActionResult> SubmitAnswers(SubmitAnswersRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var attempt = await _context.ExamAttempts
            .Include(a => a.ExamAnswers)
            .Include(a => a.Exam)
            .ThenInclude(e => e.Questions)
            .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefaultAsync(a => a.Id == dto.AttemptId);

        if (attempt == null || attempt.Status != "in_progress")
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Intento inválido o no encontrado.",
                Data = null,
                Errors = new[] { "El intento no existe o no está en progreso." }
            });

        foreach (var answerDto in dto.Answers)
        {
            var question = attempt.Exam.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
            if (question == null) continue;

            var correctOptionIds = question.QuestionOptions
                .Where(o => o.IsCorrect)
                .Select(o => o.Id)
                .OrderBy(id => id)
                .ToList();

            var selectedIds = answerDto.SelectedOptionIds.OrderBy(id => id).ToList();
            var isCorrect = correctOptionIds.SequenceEqual(selectedIds);

            // Buscar si ya existe una respuesta para esta pregunta en este intento
            var existingAnswer = attempt.ExamAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

            if (existingAnswer != null)
            {
                // Actualizar la respuesta existente
                existingAnswer.SelectedOptions = string.Join(",", selectedIds);
                existingAnswer.IsCorrect = isCorrect;
                existingAnswer.PointsEarned = isCorrect ? question.Points : 0;
                existingAnswer.UpdatedAt = DateTime.UtcNow;
                _context.ExamAnswers.Update(existingAnswer);
            }
            else
            {
                // Crear nueva respuesta
                var examAnswer = new ExamAnswer
                {
                    Id = Guid.NewGuid(),
                    AttemptId = attempt.Id,
                    QuestionId = question.Id,
                    SelectedOptions = string.Join(",", selectedIds),
                    IsCorrect = isCorrect,
                    PointsEarned = isCorrect ? question.Points : 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ExamAnswers.Add(examAnswer);
            }
        }

        attempt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Respuestas enviadas correctamente.",
            Data = null,
            Errors = null
        });
    }

    // POST /api/examattempts/finish
    // Finaliza el intento de examen, calcula el puntaje total y tiempo empleado
    [HttpPost("finish")]
    public async Task<ActionResult<ApiResponse<object>>> FinishAttempt(ExamAttemptRequest request)
    {
        var attempt = await _context.ExamAttempts
            .Include(a => a.Exam)
            .Include(a => a.ExamAnswers)
            .FirstOrDefaultAsync(a => a.Id == request.Id);
        if (attempt == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Intento no encontrado.",
                Data = null,
                Errors = new[] { "No existe un intento con el ID proporcionado." }
            });
        }
        if (attempt.Status == "completed" || attempt.Status == "terminado")
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "El intento ya está finalizado.",
                Data = null,
                Errors = new[] { "El intento ya está finalizado." }
            });
        }

        // Evaluar el examen
        int totalPoints = 0;
        int earnedPoints = 0;
        int totalQuestions = 0;
        int correctAnswers = 0;

        // Obtener todas las preguntas del examen
        var questions = await _context.Questions
            .Where(q => q.ExamId == attempt.ExamId)
            .Include(q => q.QuestionOptions)
            .ToListAsync();

        totalQuestions = questions.Count;

        foreach (var question in questions)
        {
            totalPoints += question.Points;
            var answer = attempt.ExamAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
            if (answer != null)
            {
                // Verificar si la respuesta es correcta
                var correctOptionIds = question.QuestionOptions
                    .Where(o => o.IsCorrect)
                    .Select(o => o.Id.ToString())
                    .OrderBy(x => x)
                    .ToList();
                var selectedOptionIds = answer.SelectedOptions
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .OrderBy(x => x)
                    .ToList();
                bool isCorrect = correctOptionIds.SequenceEqual(selectedOptionIds);
                answer.IsCorrect = isCorrect;
                answer.PointsEarned = isCorrect ? question.Points : 0;
                if (isCorrect) correctAnswers++;
                earnedPoints += answer.PointsEarned;
            }
        }

        // Calcular nota sobre 100
        int score = totalPoints > 0 ? (int)Math.Round((earnedPoints * 100.0) / totalPoints) : 0;

        // Actualizar intento
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.Status = "completed";
        attempt.Score = score;
        attempt.TotalPoints = totalPoints;
        attempt.EarnedPoints = earnedPoints;
        attempt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Examen finalizado y evaluado correctamente.",
            Data = new
            {
                attempt.Id,
                attempt.CompletedAt,
                attempt.Status,
                attempt.Score,
                attempt.TotalPoints,
                attempt.EarnedPoints
            },
            Errors = null
        });
    }
}