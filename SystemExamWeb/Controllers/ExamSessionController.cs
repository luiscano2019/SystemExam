using Microsoft.AspNetCore.Mvc;
using SystemExamWeb.Models;
using SystemExamWeb.Helpers;
using System.Text.Json;
using SystemExamWeb.Responses;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;

namespace SystemExamWeb.Controllers
{

    public class ExamSessionController : Controller
    {
        private readonly string _apiBaseUrl;

        public ExamSessionController(IConfiguration config)
        {
            _apiBaseUrl = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // Muestra la primera pregunta
        [HttpGet]
        public async Task<IActionResult> Start(Guid examId)
        {

           
            // Si no hay AttemptId en TempData, crear el intento
            Guid attemptId;
            if (TempData["AttemptId"] == null)
            {
                // Obtener el usuario actual (StudentId)
                //var userId = User.Claims.FirstOrDefault(c => c.Type == "nameidentifier")?.Value;
                //if (string.IsNullOrEmpty(userId))
                //    return RedirectToAction("Login", "Auth");

                var createAttempt = new
                {
                    ExamId = examId,
                    //StudentId = Guid.Parse(userId)
                };
                var apiUrl = $"{_apiBaseUrl}/api/examattempts/start";
                var token = TokenHelper.GetJwtToken(Request);
                using var httpClient = new HttpClient();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var content = new StringContent(JsonSerializer.Serialize(createAttempt), System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    TempData.Remove("AttemptId");
                    return RedirectToAction("Index", "Exams");
                }
                var json = await response.Content.ReadAsStringAsync();

                //var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<Exam>>>(json, new JsonSerializerOptions

             var apiResponse = JsonSerializer.Deserialize<ApiResponse<ExamAttempt>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
              
                if (apiResponse == null || apiResponse.Data == null)
                {
                    // Manejar error de respuesta nula o datos faltantes
                    throw new Exception("No se pudo obtener el intento del examen desde la API.");
                }

                attemptId = Guid.Parse(apiResponse.Data.Id.ToString());
                TempData["AttemptId"] = attemptId.ToString();
            }
            else
            {
                attemptId = Guid.Parse(TempData["AttemptId"].ToString());
            }
            TempData.Keep("AttemptId");
            return await ShowQuestion(examId, 0, attemptId);
        }

        [HttpPost]
        public async Task<IActionResult> Start(Guid examId, int questionIndex, string action, Guid attemptId, Guid selectedOption)
        {
            // Guardar la respuesta seleccionada llamando a la API
            var token = TokenHelper.GetJwtToken(Request);
            using var httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            // Construir el body para SubmitAnswersRequest
            var submitRequest = new
            {
                AttemptId = attemptId,
                Answers = new[]
                {
                    new {
                        QuestionId = await GetQuestionIdByIndex(examId, questionIndex - (action == "back" ? 1 : 0)),
                        SelectedOptionIds = new[] { selectedOption }
                    }
                }
            };
            var submitContent = new StringContent(JsonSerializer.Serialize(submitRequest), System.Text.Encoding.UTF8, "application/json");
            var submitUrl = $"{_apiBaseUrl}/api/examattempts/submit-answers";
            await httpClient.PostAsync(submitUrl, submitContent);

            // Obtener total de preguntas
            int totalQuestions = await GetTotalQuestions(examId);

            // Si es la última pregunta y presiona siguiente, finalizar
            if (action == "next" && questionIndex >= totalQuestions)
            {
                // Llamar a la API para finalizar el intento
                var finishRequest = new { Id = attemptId };
                var finishContent = new StringContent(JsonSerializer.Serialize(finishRequest), System.Text.Encoding.UTF8, "application/json");
                var finishUrl = $"{_apiBaseUrl}/api/examattempts/finish";
                var finishResponse = await httpClient.PostAsync(finishUrl, finishContent);
                TempData.Remove("AttemptId");

                if (finishResponse.IsSuccessStatusCode)
                {
                    var json = await finishResponse.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ExamAttempt>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    ViewBag.FinishDetails = apiResponse?.Data;
                    return View("Finish");
                }
                return RedirectToAction("Finish");
            }

            // Navegación
            if (action == "back")
            {
                questionIndex--;
            }
            // "next" es la acción por defecto

            TempData["AttemptId"] = attemptId.ToString();
            TempData.Keep("AttemptId");
            return await ShowQuestion(examId, questionIndex, attemptId);
        }

        private async Task<Guid> GetQuestionIdByIndex(Guid examId, int questionIndex)
        {
            var apiUrl = $"{_apiBaseUrl}/api/Question/by-exam/{examId}";
            var token = TokenHelper.GetJwtToken(Request);
            using var httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<QuestionResponse>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var questions = apiResponse?.Data?.ToList() ?? new List<QuestionResponse>();
                if (questionIndex >= 0 && questionIndex < questions.Count)
                    return questions[questionIndex].Id;
            }
            return Guid.Empty;
        }

        private async Task<int> GetTotalQuestions(Guid examId)
        {
            var apiUrl = $"{_apiBaseUrl}/api/Question/by-exam/{examId}";
            var token = TokenHelper.GetJwtToken(Request);
            using var httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<QuestionResponse>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var questions = apiResponse?.Data?.ToList() ?? new List<QuestionResponse>();
                return questions.Count;
            }
            return 0;
        }

        private async Task<IActionResult> ShowQuestion(Guid examId, int questionIndex, Guid attemptId)
        {
            var questions = new List<QuestionResponse>();
            var apiUrl = $"{_apiBaseUrl}/api/Question/by-exam/{examId}";

            var token = TokenHelper.GetJwtToken(Request);
            using var httpClient = new HttpClient();
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<QuestionResponse>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                questions = apiResponse?.Data?.ToList() ?? new List<QuestionResponse>();
            }

            if (!questions.Any() || questionIndex >= questions.Count || questionIndex < 0)
                return RedirectToAction("Index", "Exams"); // O a una página de finalización

            var currentQuestion = questions[questionIndex];
            ViewBag.ExamId = examId;
            ViewBag.QuestionIndex = questionIndex;
            ViewBag.TotalQuestions = questions.Count;
            ViewBag.AttemptId = attemptId;
            return View("ExamPage", currentQuestion);
        }

        public IActionResult Finish()
        {
            return View();
        }

        // Aquí puedes agregar métodos para guardar respuestas y avanzar a la siguiente pregunta
    }
}